using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PedestalAutoSetupEditor
{
    private const string PedestalFolder = "Assets/TowerPedestalKit/Prefabs/";

    [MenuItem("Tools/Towers/Auto Setup Selected Pedestals")]
    public static void SetupSelected()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Iesi din Play Mode inainte de configurare.", "OK");
            return;
        }

        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Selecteaza pedestalurile", "Selecteaza in Hierarchy obiectul parinte Pedestal sau pedestalurile dorite.", "OK");
            return;
        }

        int configured = 0;
        int skipped = 0;
        int missing = 0;

        // Traverse only selected scene objects, never assets in Project.
        foreach (GameObject selected in Selection.gameObjects)
        {
            if (EditorUtility.IsPersistent(selected))
                continue;

            foreach (Transform t in selected.GetComponentsInChildren<Transform>(true))
            {
                GameObject go = t.gameObject;
                if (PrefabUtility.GetNearestPrefabInstanceRoot(go) != go)
                    continue;

                string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                if (string.IsNullOrEmpty(source) ||
                    !source.StartsWith(PedestalFolder, StringComparison.OrdinalIgnoreCase))
                    continue;

                string season = System.IO.Path.GetFileNameWithoutExtension(source);
                if (season != "Primavara" && season != "Vara" &&
                    season != "Toamna" && season != "Iarna")
                {
                    skipped++;
                    continue;
                }

                GameObject lv1 = FindTower(season, 1);
                GameObject lv2 = FindTower(season, 2);
                GameObject lv3 = FindTower(season, 3);

                if (lv1 == null || lv2 == null || lv3 == null)
                {
                    Debug.LogWarning("Prefab turn lipsa pentru " + season + " - " + go.name);
                    missing++;
                    continue;
                }

                TowerPedestal pedestal = go.GetComponent<TowerPedestal>();
                if (pedestal == null)
                    pedestal = Undo.AddComponent<TowerPedestal>(go);

                Undo.RecordObject(pedestal, "Configure Tower Pedestal");

                // Populam prima optiune din lista towerOptions daca nu a fost setata manual
                if (pedestal.towerOptions == null || pedestal.towerOptions.Length == 0)
                {
                    pedestal.towerOptions = new TowerOption[]
                    {
                        new TowerOption
                        {
                            towerName = "Turn " + season,
                            level1Prefab = lv1,
                            level2Prefab = lv2,
                            level3Prefab = lv3
                        }
                    };
                }
                else
                {
                    if (pedestal.towerOptions[0].level1Prefab == null) pedestal.towerOptions[0].level1Prefab = lv1;
                    if (pedestal.towerOptions[0].level2Prefab == null) pedestal.towerOptions[0].level2Prefab = lv2;
                    if (pedestal.towerOptions[0].level3Prefab == null) pedestal.towerOptions[0].level3Prefab = lv3;
                }

                if (pedestal.towerSpawnPoint == null)
                {
                    Transform spawn = go.transform.Find("TowerSpawnPoint");
                    if (spawn == null)
                    {
                        GameObject point = new GameObject("TowerSpawnPoint");
                        Undo.RegisterCreatedObjectUndo(point, "Create Tower Spawn Point");
                        Undo.SetTransformParent(point.transform, go.transform, "Parent Tower Spawn Point");
                        point.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                        point.transform.localRotation = Quaternion.identity;
                        spawn = point.transform;
                    }
                    pedestal.towerSpawnPoint = spawn;
                }

                // Existing setup is preserved. New components get scale 2 by default.
                if (Mathf.Approximately(pedestal.towerScale, 0f))
                    pedestal.towerScale = 2f;

                EditorUtility.SetDirty(pedestal);
                configured++;
            }
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Pedestals configured",
            "Configurate/verificate: " + configured + "\nPrefaburi lipsa: " + missing +
            "\nAlte prefaburi ignorate: " + skipped +
            "\nVerifica orientarea si inaltimea TowerSpawnPoint pe fiecare pedestal.", "OK");
    }

    private static GameObject FindTower(string season, int level)
    {
        string name = season == "Toamna" ? "Turn_lvl" + level : "Turn_lvl" + level + "_" + season;
        string seasonLower = season.ToLowerInvariant();

        string[] paths = {
            "Assets/TowerPrefab/" + seasonLower + "/" + name + ".prefab",
            "Assets/Towers/Prefabs/" + season + "/" + name + ".prefab"
        };

        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;
        }
        return null;
    }
}