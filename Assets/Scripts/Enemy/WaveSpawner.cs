using System.Collections;
using UnityEngine;

public enum SeasonStage
{
    Primavara = 0, // 5 valuri
    Vara = 1,      // 10 valuri
    Toamna = 2,    // 15 valuri
    Iarna = 3      // 20 valuri + Boss
}

[System.Serializable]
public class SeasonWaveConfig
{
    public string seasonName = "Primavara";
    public int totalWaves = 5;
    public WaypointPath[] paths; // Pune Path_Primavara_Stanga si Path_Primavara_Dreapta

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject fastEnemyPrefab; // Optional (daca e gol, foloseste basic)
    public GameObject tankEnemyPrefab; // Optional (daca e gol, foloseste basic)
    public GameObject finalBossPrefab; // Pentru ultimul val din Iarna (sau fiecare anotimp)

    [Header("Wave Difficulty")]
    public int startingEnemiesCount = 5;
    public int extraEnemiesPerWave = 2;
    public float timeBetweenSpawns = 0.8f;
    public float healthMultiplierPerWave = 1.15f;
}

public class WaveSpawner : MonoBehaviour
{
    [Header("Seasons Progression (5 / 10 / 15 / 20 Waves)")]
    public SeasonWaveConfig[] seasons;
    public float timeBetweenWaves = 5f;
    public bool autoStartNextWave = true;

    private int currentSeasonIndex = 0;
    private int currentWaveNumber = 0; // 1..totalWaves
    private int aliveEnemies = 0;
    private bool isSpawning = false;
    private float countdown = 3f;
    private bool levelCompleted = false;

    public int CurrentWave => currentWaveNumber;
    public string CurrentSeasonName => (seasons != null && currentSeasonIndex < seasons.Length)
        ? seasons[currentSeasonIndex].seasonName : "";

    private void Update()
    {
        if (levelCompleted || seasons == null || seasons.Length == 0) return;

        // Cat timp se spawneaza sau mai sunt inamici in viata, asteptam
        if (isSpawning || aliveEnemies > 0) return;

        SeasonWaveConfig activeSeason = seasons[currentSeasonIndex];

        // Daca am terminat toate valurile din anotimpul curent
        if (currentWaveNumber >= activeSeason.totalWaves)
        {
            if (currentSeasonIndex < seasons.Length - 1)
            {
                currentSeasonIndex++;
                currentWaveNumber = 0;
                countdown = timeBetweenWaves;
                Debug.Log($"Anotimp nou deblocat: {seasons[currentSeasonIndex].seasonName}!");
            }
            else
            {
                levelCompleted = true;
                Debug.Log("VICTORIE! Ai invins toate anotimpurile si Boss-ul final!");
            }
            return;
        }

        if (autoStartNextWave)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                StartNextWave();
            }
        }
    }

    public void StartNextWave()
    {
        if (isSpawning || levelCompleted) return;
        currentWaveNumber++;
        StartCoroutine(SpawnWaveRoutine(seasons[currentSeasonIndex], currentWaveNumber));
    }

    private IEnumerator SpawnWaveRoutine(SeasonWaveConfig season, int waveIndex)
    {
        isSpawning = true;

        int enemyCount = season.startingEnemiesCount + (waveIndex - 1) * season.extraEnemiesPerWave;
        float hpScale = Mathf.Pow(season.healthMultiplierPerWave, waveIndex - 1);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefabToSpawn = PickEnemyForWave(season, waveIndex, i);
            SpawnSingleEnemy(prefabToSpawn, season.paths, hpScale);
            yield return new WaitForSeconds(season.timeBetweenSpawns);
        }

        // Daca este ultimul val din anotimp si avem Boss setat (ex: Valul 20 din Iarna)
        if (waveIndex == season.totalWaves && season.finalBossPrefab != null)
        {
            yield return new WaitForSeconds(1.2f);
            SpawnSingleEnemy(season.finalBossPrefab, season.paths, hpScale);
            Debug.Log("BOSS FINAL SPAWNAT!");
        }

        isSpawning = false;
        countdown = timeBetweenWaves;
    }

    private GameObject PickEnemyForWave(SeasonWaveConfig season, int waveIndex, int spawnOrder)
    {
        if (waveIndex >= 4 && season.tankEnemyPrefab != null && spawnOrder % 5 == 4)
            return season.tankEnemyPrefab;

        if (waveIndex >= 2 && season.fastEnemyPrefab != null && spawnOrder % 3 == 2)
            return season.fastEnemyPrefab;

        return season.basicEnemyPrefab;
    }

    private void SpawnSingleEnemy(GameObject prefab, WaypointPath[] paths, float hpMultiplier)
    {
        if (prefab == null || paths == null || paths.Length == 0) return;

        // Alege aleatoriu una dintre ramurile drumului (Stanga / Dreapta)
        WaypointPath chosenPath = paths[Random.Range(0, paths.Length)];
        if (chosenPath == null) return;

        GameObject obj = Instantiate(prefab);
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.maxHealth *= hpMultiplier;
            enemy.SetPath(chosenPath);
            aliveEnemies++;
            StartCoroutine(TrackEnemyDeath(obj));
        }
    }

    private IEnumerator TrackEnemyDeath(GameObject enemyObj)
    {
        while (enemyObj != null)
        {
            yield return null;
        }
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}