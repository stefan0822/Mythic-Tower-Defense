using UnityEngine;

public class ZiduriAnotimpuri : MonoBehaviour
{
    public float spatiuLaCapete = 5f;
    public float suprapunere = 1f;
    public float inaltime = 0f;

    [ContextMenu("Construieste zidurile despartitoare")]
    public void Construieste()
    {
        OctagonMapBuilder harta = GetComponent<OctagonMapBuilder>();
        if (harta == null || harta.wallPiece == null)
        {
            Debug.LogError("Lipseste OctagonMapBuilder sau modelul de zid.", this);
            return;
        }

        Transform vechi = transform.Find("Ziduri_Despartitoare");
        if (vechi != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.DestroyObjectImmediate(vechi.gameObject);
            else Destroy(vechi.gameObject);
#else
            Destroy(vechi.gameObject);
#endif
        }

        GameObject grup = new GameObject("Ziduri_Despartitoare");
        grup.transform.SetParent(transform, false);
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(grup, "Ziduri despartitoare");
#endif

        float factor = 1f / (2f * Mathf.Tan(Mathf.PI / 8f));
        float exterior = harta.outerPiecesPerSide * harta.wallLength * factor;
        float interior = harta.innerPiecesPerSide * harta.wallLength * factor;
        float inceput = interior + spatiuLaCapete;
        float sfarsit = exterior - spatiuLaCapete;
        if (sfarsit <= inceput)
        {
            Debug.LogWarning("Nu este loc intre cele doua octogoane.", this);
            return;
        }

        Vector3 centru = transform.TransformPoint(harta.mapCenter);
        int bucati = Mathf.Max(1, Mathf.RoundToInt((sfarsit - inceput) / harta.wallLength));
        float lungime = (sfarsit - inceput) / bucati;

        for (int i = 0; i < 4; i++)
        {
            float unghi = (45f + i * 90f) * Mathf.Deg2Rad;
            Vector3 directie = new Vector3(Mathf.Cos(unghi), 0, Mathf.Sin(unghi));
            float rotatie = -Mathf.Atan2(directie.z, directie.x) * Mathf.Rad2Deg + harta.wallYawOffset;

            for (int j = 0; j < bucati; j++)
            {
                Vector3 pozitie = centru + directie * (inceput + (j + 0.5f) * lungime);
                pozitie.y += inaltime + harta.wallY;
                GameObject zid = Instantiate(harta.wallPiece, pozitie, Quaternion.Euler(0, rotatie, 0), grup.transform);
                zid.name = $"Despartitor_{i + 1}_{j + 1}";
                zid.SetActive(true);

                float latime = Latime(zid, directie);
                if (latime > 0.001f)
                {
                    Vector3 scara = zid.transform.localScale;
                    scara.x *= (lungime + suprapunere) / latime;
                    zid.transform.localScale = scara;
                }
#if UNITY_EDITOR
                if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(zid, "Adauga zid");
#endif
            }
        }
    }

    float Latime(GameObject obiect, Vector3 directie)
    {
        float minim = float.PositiveInfinity, maxim = float.NegativeInfinity;
        foreach (MeshFilter filtru in obiect.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filtru.sharedMesh == null) continue;
            Bounds b = filtru.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 colt = b.center + Vector3.Scale(b.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1,
                                (i & 2) == 0 ? -1 : 1,
                                (i & 4) == 0 ? -1 : 1));
                float pozitie = Vector3.Dot(filtru.transform.TransformPoint(colt), directie);
                minim = Mathf.Min(minim, pozitie);
                maxim = Mathf.Max(maxim, pozitie);
            }
        }
        return float.IsInfinity(minim) ? 0f : maxim - minim;
    }
}
