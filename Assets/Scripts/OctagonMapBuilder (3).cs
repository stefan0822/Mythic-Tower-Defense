using UnityEngine;

public class OctagonMapBuilder : MonoBehaviour
{
    [Header("Piese")]
    public GameObject wallPiece, cornerTower, gatePiece;

    [Header("Dimensiuni")]
    public Vector3 mapCenter;
    [Min(1)] public int outerPiecesPerSide = 12, innerPiecesPerSide = 2;
    [Min(0.01f)] public float wallLength = 24f;
    public float wallYawOffset, gateYawOffset;

    [Header("Porti")]
    public bool outerGates = true, innerGates = true;
    [Min(1)] public int outerGateSlots = 1, innerGateSlots = 1;

    [Header("Inaltimi")]
    public float wallY, towerY, gateY;

    [ContextMenu("Construieste octogoanele")]
    public void BuildBothOctagons()
    {
        if (!wallPiece || outerPiecesPerSide <= innerPiecesPerSide)
        {
            Debug.LogError("Verifica piesele si dimensiunile.", this);
            return;
        }

        RemoveGenerated();
        Construieste("Exterior_Octagon", outerPiecesPerSide, outerGates, outerGateSlots);
        Construieste("Central_Castle_Octagon", innerPiecesPerSide, innerGates, innerGateSlots);
    }

    [ContextMenu("Sterge octogoanele generate")]
    public void RemoveGenerated()
    {
        foreach (string nume in new[] { "Exterior_Octagon", "Central_Castle_Octagon" })
        {
            Transform copil = transform.Find(nume);
            if (!copil) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.DestroyObjectImmediate(copil.gameObject);
            else Destroy(copil.gameObject);
#else
            Destroy(copil.gameObject);
#endif
        }
    }

    GameObject Adauga(GameObject model, Transform parinte, string nume, Vector3 pozitie, float rotatie)
    {
        GameObject obiect = Instantiate(model, pozitie, Quaternion.Euler(0, rotatie, 0), parinte);
        obiect.name = nume;
        obiect.SetActive(true);
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(obiect, "Adauga piesa");
#endif
        return obiect;
    }

    float Latime(GameObject obiect, Vector3 directie)
    {
        float minim = float.PositiveInfinity, maxim = float.NegativeInfinity;
        foreach (MeshFilter filtru in obiect.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!filtru.sharedMesh) continue;
            Bounds b = filtru.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = b.center + Vector3.Scale(b.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1,
                                (i & 2) == 0 ? -1 : 1,
                                (i & 4) == 0 ? -1 : 1));
                float x = Vector3.Dot(filtru.transform.TransformPoint(p), directie);
                minim = Mathf.Min(minim, x);
                maxim = Mathf.Max(maxim, x);
            }
        }
        return float.IsInfinity(minim) ? 0 : maxim - minim;
    }

    void Construieste(string nume, int bucati, bool cuPorti, int locuriPoarta)
    {
        GameObject grup = new GameObject(nume);
        grup.transform.SetParent(transform, false);
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(grup, "Creeaza octogon");
#endif
        float latura = bucati * wallLength;
        float raza = latura / (2 * Mathf.Sin(Mathf.PI / 8));
        Vector3[] colturi = new Vector3[8];
        Vector3 centru = transform.TransformPoint(mapCenter);

        for (int i = 0; i < 8; i++)
        {
            float unghi = (22.5f + 45 * i) * Mathf.Deg2Rad;
            colturi[i] = centru + raza * new Vector3(Mathf.Cos(unghi), 0, Mathf.Sin(unghi));
        }

        for (int i = 0; i < 8; i++)
        {
            Vector3 a = colturi[i], b = colturi[(i + 1) % 8];
            Vector3 directie = (b - a).normalized;
            float lungime = Vector3.Distance(a, b);
            float rotatie = -Mathf.Atan2(directie.z, directie.x) * Mathf.Rad2Deg + wallYawOffset;
            bool arePoarta = cuPorti && gatePiece && i % 2 == 1;
            float latimePoarta = 0;

            if (arePoarta)
            {
                Vector3 poz = (a + b) / 2 + Vector3.up * gateY;
                GameObject poarta = Adauga(gatePiece, grup.transform, $"Poarta_{i + 1}", poz, rotatie + gateYawOffset);
                latimePoarta = Latime(poarta, directie);
                if (latimePoarta >= lungime)
                {
                    Debug.LogWarning("Poarta este prea lata pentru latura " + (i + 1), this);
                    latimePoarta = lungime;
                }
            }

            int perJumatate = arePoarta ? Mathf.Max(0, (bucati - Mathf.Clamp(locuriPoarta, 1, bucati)) / 2) : 0;
            int numarZiduri = arePoarta ? perJumatate * 2 : bucati;
            float latimeZid = arePoarta && numarZiduri > 0
                ? (lungime - latimePoarta) / numarZiduri : lungime / bucati;

            for (int j = 0; j < numarZiduri; j++)
            {
                float distanta;
                if (arePoarta)
                {
                    bool stanga = j < perJumatate;
                    int index = stanga ? j : j - perJumatate;
                    distanta = stanga
                        ? (index + 0.5f) * latimeZid
                        : (lungime + latimePoarta) / 2 + (index + 0.5f) * latimeZid;
                }
                else distanta = (j + 0.5f) * latimeZid;

                GameObject zid = Adauga(wallPiece, grup.transform, $"Zid_{i + 1}_{j + 1}",
                    a + directie * distanta + Vector3.up * wallY, rotatie);
                float latimeModel = Latime(zid, directie);
                if (latimeModel > 0.001f)
                {
                    float extra = 2f;

                    if (arePoarta && (j == perJumatate - 1 || j == perJumatate))
                        extra = 6f;

                    Vector3 scara = zid.transform.localScale;
                    scara.x *= (latimeZid + extra) / latimeModel;
                    zid.transform.localScale = scara;
                }
            }

            if (cornerTower)
                Adauga(cornerTower, grup.transform, $"Turn_{i + 1}", a + Vector3.up * towerY, 0);
        }
    }
}
