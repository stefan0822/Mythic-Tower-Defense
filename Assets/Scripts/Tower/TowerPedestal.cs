using UnityEngine;

[System.Serializable]
public class TowerOption
{
    public string towerName = "Archer";
    public Texture2D icon; // Optional: lasa gol acum pentru text, adauga imagini mai tarziu
    public GameObject level1Prefab;
    public GameObject level2Prefab;
    public GameObject level3Prefab;
}

public class TowerPedestal : MonoBehaviour
{
    [Header("Available Tower Types")]
    public TowerOption[] towerOptions;

    [Header("Placement")]
    public Transform towerSpawnPoint;
    public float heightOffset = 1.5f;
    public float towerScale = 2f;

    [Header("Radial Menu Settings")]
    public float ringRadius = 95f;
    public Vector2 cardSize = new Vector2(88f, 64f);
    public float autoCloseRadius = 165f;

    private GameObject currentTower;
    private TowerOption selectedOption;
    private int currentLevel = 0;

    private bool menuOpen = false;
    private Vector2 menuCenterGUI;

    // Stiluri UI si cerc de raza pe sol
    private GUIStyle cardStyle;
    private GUIStyle ringStyle;
    private Texture2D cardBgTex;
    private Texture2D cardHoverTex;
    private Texture2D ringTex;
    private LineRenderer rangeCircle;

    private void Awake()
    {
        SetupRangeCircle();
    }

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Click dreapta pe piedestal deschide meniul radial centrat pe piedestal
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
            {
                TowerPedestal clickedPedestal = hit.collider.GetComponentInParent<TowerPedestal>();

                if (clickedPedestal == this)
                {
                    OpenMenu();
                }
                else
                {
                    CloseMenu();
                }
            }
            else
            {
                CloseMenu();
            }
        }

        if (menuOpen)
        {
            // Actualizam centrul pe ecran in caz ca misti camera
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            menuCenterGUI = new Vector2(screenPos.x, Screen.height - screenPos.y);

            Vector2 guiMousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

            // Daca mouse-ul iese in afara cercului meniului, se inchide singur
            if (Vector2.Distance(guiMousePos, menuCenterGUI) > autoCloseRadius)
            {
                CloseMenu();
            }
        }
    }

    private void OpenMenu()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
        menuCenterGUI = new Vector2(screenPos.x, Screen.height - screenPos.y);
        menuOpen = true;

        UpdateRangeVisual(true);
    }

    private void CloseMenu()
    {
        menuOpen = false;
        UpdateRangeVisual(false);
    }

    private void OnGUI()
    {
        if (!menuOpen) return;

        InitStyles();

        // Desenam inelul albastru din jurul piedestalului
        float ringDiameter = ringRadius * 2f;
        Rect ringRect = new Rect(
            menuCenterGUI.x - ringRadius,
            menuCenterGUI.y - ringRadius,
            ringDiameter,
            ringDiameter
        );
        GUI.Box(ringRect, GUIContent.none, ringStyle);

        if (currentLevel == 0)
        {
            DrawBuildCards();
        }
        else
        {
            DrawUpgradeAndSellCards();
        }
    }

    private void DrawBuildCards()
    {
        if (towerOptions == null || towerOptions.Length == 0) return;

        // Pozitii radiale ca in referinta: Stanga, Sus, Dreapta
        Vector2[] offsets = new Vector2[]
        {
            new Vector2(-ringRadius, 0f),  // Stanga
            new Vector2(0f, -ringRadius),  // Sus
            new Vector2(ringRadius, 0f),   // Dreapta
            new Vector2(0f, ringRadius)    // Jos (daca adaugi al 4-lea turn)
        };

        int count = Mathf.Min(towerOptions.Length, offsets.Length);
        for (int i = 0; i < count; i++)
        {
            TowerOption option = towerOptions[i];
            Rect cardRect = GetCardRect(menuCenterGUI + offsets[i]);

            GUIContent content = option.icon != null
                ? new GUIContent(option.towerName, option.icon)
                : new GUIContent(option.towerName);

            if (GUI.Button(cardRect, content, cardStyle))
            {
                BuildTower(option);
                CloseMenu();
            }
        }
    }

    private void DrawUpgradeAndSellCards()
    {
        // Card SUS: UPGRADE (sau MAX)
        Rect topRect = GetCardRect(menuCenterGUI + new Vector2(0f, -ringRadius));
        if (currentLevel < 3)
        {
            string upgradeLabel = $"UPGRADE\nLvl {currentLevel + 1}";
            if (GUI.Button(topRect, upgradeLabel, cardStyle))
            {
                UpgradeTower();
                CloseMenu();
            }
        }
        else
        {
            GUI.Box(topRect, "MAX\nLEVEL", cardStyle);
        }

        // Card JOS: SELL
        Rect bottomRect = GetCardRect(menuCenterGUI + new Vector2(0f, ringRadius));
        if (GUI.Button(bottomRect, "SELL", cardStyle))
        {
            SellTower();
            CloseMenu();
        }
    }

    private Rect GetCardRect(Vector2 center)
    {
        return new Rect(
            center.x - cardSize.x * 0.5f,
            center.y - cardSize.y * 0.5f,
            cardSize.x,
            cardSize.y
        );
    }

    private void BuildTower(TowerOption option)
    {
        if (currentLevel != 0 || option == null) return;

        if (CreateTower(option.level1Prefab, 1))
        {
            selectedOption = option;
        }
    }

    private void UpgradeTower()
    {
        if (currentLevel == 0 || currentLevel >= 3 || selectedOption == null) return;

        int nextLevel = currentLevel + 1;
        GameObject nextPrefab = nextLevel == 2 ? selectedOption.level2Prefab : selectedOption.level3Prefab;

        if (nextPrefab == null)
        {
            Debug.LogError($"Missing prefab for {selectedOption.towerName} level {nextLevel}");
            return;
        }

        GameObject oldTower = currentTower;
        if (CreateTower(nextPrefab, nextLevel))
        {
            if (oldTower != null)
                Destroy(oldTower);
        }
    }

    private void SellTower()
    {
        if (currentTower != null)
            Destroy(currentTower);

        currentTower = null;
        selectedOption = null;
        currentLevel = 0;
    }

    private bool CreateTower(GameObject prefab, int level)
    {
        if (prefab == null)
        {
            Debug.LogError("Tower prefab is missing for level " + level);
            return false;
        }

        Vector3 spawnPosition = towerSpawnPoint != null
            ? towerSpawnPoint.position
            : transform.TransformPoint(new Vector3(0f, heightOffset, 0f));

        Quaternion spawnRotation = towerSpawnPoint != null
            ? towerSpawnPoint.rotation
            : transform.rotation;

        GameObject newTower = Instantiate(prefab, spawnPosition, spawnRotation);
        if (newTower == null) return false;

        newTower.transform.localScale *= towerScale;
        newTower.transform.SetParent(transform, true);
        newTower.name = "BuiltTower_Level" + level;

        currentTower = newTower;
        currentLevel = level;

        return true;
    }

    private void SetupRangeCircle()
    {
        rangeCircle = GetComponent<LineRenderer>();
        if (rangeCircle == null)
        {
            rangeCircle = gameObject.AddComponent<LineRenderer>();
        }

        if (rangeCircle == null) return;

        rangeCircle.useWorldSpace = false;
        rangeCircle.loop = true;
        rangeCircle.startWidth = 0.25f;
        rangeCircle.endWidth = 0.25f;

        Shader lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (lineShader == null) lineShader = Shader.Find("Unlit/Color");
        if (lineShader == null) lineShader = Shader.Find("Sprites/Default");

        if (lineShader != null)
        {
            rangeCircle.material = new Material(lineShader);
        }

        rangeCircle.startColor = new Color(1f, 0.55f, 0f, 0.85f);
        rangeCircle.endColor = new Color(1f, 0.55f, 0f, 0.85f);
        rangeCircle.enabled = false;
    }

    private void UpdateRangeVisual(bool show)
    {
        if (rangeCircle == null) return;

        if (!show || currentTower == null)
        {
            rangeCircle.enabled = false;
            return;
        }

        TowerBase tower = currentTower.GetComponent<TowerBase>();
        float radius = tower != null ? tower.Range : 15f;

        int segments = 48;
        rangeCircle.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            rangeCircle.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.6f, Mathf.Sin(angle) * radius));
        }

        rangeCircle.enabled = true;
    }

    private void InitStyles()
    {
        if (cardStyle != null) return;

        cardBgTex = MakeBorderedTexture(64, 64, new Color(0.55f, 0.72f, 0.88f, 0.95f), new Color(0.14f, 0.25f, 0.45f, 1f), 4);
        cardHoverTex = MakeBorderedTexture(64, 64, new Color(0.70f, 0.85f, 1f, 1f), new Color(0.95f, 0.65f, 0.15f, 1f), 4);
        ringTex = MakeRingTexture(128, new Color(0.16f, 0.30f, 0.55f, 0.9f), 6);

        cardStyle = new GUIStyle(GUI.skin.button);
        cardStyle.normal.background = cardBgTex;
        cardStyle.hover.background = cardHoverTex;
        cardStyle.active.background = cardHoverTex;
        cardStyle.normal.textColor = new Color(0.08f, 0.12f, 0.20f, 1f);
        cardStyle.hover.textColor = Color.black;
        cardStyle.fontStyle = FontStyle.Bold;
        cardStyle.fontSize = 13;
        cardStyle.alignment = TextAnchor.MiddleCenter;
        cardStyle.wordWrap = true;
        cardStyle.imagePosition = ImagePosition.ImageAbove;

        ringStyle = new GUIStyle();
        ringStyle.normal.background = ringTex;
    }

    private Texture2D MakeBorderedTexture(int w, int h, Color fill, Color border, int borderSize)
    {
        Texture2D tex = new Texture2D(w, h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = x < borderSize || x >= w - borderSize || y < borderSize || y >= h - borderSize;
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        }
        tex.Apply();
        return tex;
    }

    private Texture2D MakeRingTexture(int size, Color color, int thickness)
    {
        Texture2D tex = new Texture2D(size, size);
        float center = size * 0.5f;
        float outerR = center - 2f;
        float innerR = outerR - thickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, (dist <= outerR && dist >= innerR) ? color : Color.clear);
            }
        }
        tex.Apply();
        return tex;
    }

    public int GetTowerLevel() => currentLevel;
}