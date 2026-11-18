
using UnityEngine;

public class TowerPedestal : MonoBehaviour
{
    [Header("Tower Prefabs")]
    public GameObject towerLevel1;
    public GameObject towerLevel2;
    public GameObject towerLevel3;

    [Header("Placement")]
    public Transform towerSpawnPoint;
    public float heightOffset = 1.5f;
    public float towerScale = 2f;

    private GameObject currentTower;
    private int currentLevel = 0;

    private bool menuOpen = false;
    private Vector2 menuPosition;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found!");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
        {
            TowerPedestal clickedPedestal =
                hit.collider.GetComponentInParent<TowerPedestal>();

            if (clickedPedestal == this)
            {
                menuPosition = new Vector2(
                    Input.mousePosition.x,
                    Screen.height - Input.mousePosition.y
                );

                menuOpen = true;
            }
            else
            {
                menuOpen = false;
            }
        }
        else
        {
            menuOpen = false;
        }
    }

    private void OnGUI()
    {
        if (!menuOpen)
            return;

        float x = Mathf.Clamp(
            menuPosition.x, 0, Screen.width - 200
        );

        float y = Mathf.Clamp(
            menuPosition.y, 0, Screen.height - 130
        );

        GUILayout.BeginArea(
            new Rect(x, y, 200, 130),
            GUI.skin.box
        );

        GUILayout.Label("Tower Menu");

        if (currentLevel == 0)
        {
            if (GUILayout.Button("Build Tower"))
            {
                BuildTower();
                menuOpen = false;
            }
        }
        else if (currentLevel < 3)
        {
            GUILayout.Label("Level " + currentLevel);

            if (GUILayout.Button(
                "Upgrade to Level " + (currentLevel + 1)))
            {
                UpgradeTower();
                menuOpen = false;
            }
        }
        else
        {
            GUILayout.Label("Level 3 - MAX LEVEL");
        }

        if (GUILayout.Button("Close"))
        {
            menuOpen = false;
        }

        GUILayout.EndArea();
    }

    private void BuildTower()
    {
        if (currentLevel != 0)
            return;

        if (CreateTower(towerLevel1, 1))
        {
            Debug.Log("Tower built! Level 1");
        }
    }

    private void UpgradeTower()
    {
        if (currentLevel == 0 || currentLevel >= 3)
            return;

        int nextLevel = currentLevel + 1;

        GameObject nextPrefab =
            nextLevel == 2 ? towerLevel2 : towerLevel3;

        if (nextPrefab == null)
        {
            Debug.LogError(
                "Missing prefab for level " + nextLevel
            );
            return;
        }

        // Create the new tower before deleting the old one.
        GameObject oldTower = currentTower;

        if (CreateTower(nextPrefab, nextLevel))
        {
            if (oldTower != null)
                Destroy(oldTower);

            Debug.Log(
                "Tower upgraded to Level " + nextLevel
            );
        }
    }

    private bool CreateTower(GameObject prefab, int level)
    {
        if (prefab == null)
        {
            Debug.LogError(
                "Tower prefab is missing for level " + level
            );
            return false;
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (towerSpawnPoint != null)
        {
            spawnPosition = towerSpawnPoint.position;
            spawnRotation = towerSpawnPoint.rotation;
        }
        else
        {
            spawnPosition = transform.TransformPoint(
                new Vector3(0f, heightOffset, 0f)
            );

            spawnRotation = transform.rotation;
        }

        GameObject newTower = Instantiate(
            prefab,
            spawnPosition,
            spawnRotation
        );

        if (newTower == null)
            return false;

        newTower.transform.localScale *= towerScale;

        // Make the tower a child of its pedestal.
        // This also lets right-clicks on tower colliders
        // find the corresponding TowerPedestal.
        newTower.transform.SetParent(transform, true);

        newTower.name = "BuiltTower_Level" + level;

        currentTower = newTower;
        currentLevel = level;

        return true;
    }

    public int GetTowerLevel()
    {
        return currentLevel;
    }
}
