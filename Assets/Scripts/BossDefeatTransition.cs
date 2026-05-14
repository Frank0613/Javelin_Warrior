using UnityEngine;

public class BossDefeatTransition : MonoBehaviour
{
    [Header("Terrain Trees")]
    public Terrain terrain;
    public int deadTreeIndexA = 6;
    public int deadTreeIndexB = 7;
    public int lushTreeIndexA = 2;
    public int lushTreeIndexB = 3;

    [Header("River")]
    public GameObject river;

    [Header("Fire Particles")]
    public GameObject[] fireParticles;

    [Header("Skybox / Fog")]
    public Material nightSkybox;
    public Material daySkybox;
    public Color nightFogColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    public Color dayFogColor = new Color(0.7f, 0.8f, 0.9f, 1f);

    [Header("Input")]
    public KeyCode triggerKey = KeyCode.C;

    private bool transitioned = false;
    private TreeInstance[] originalTreeInstances;

    void Awake()
    {
        CacheOriginalTrees();
    }

    void Start()
    {
        ApplyInitialState();
    }

    void OnApplicationQuit()
    {
        RestoreOriginalTrees();
    }

    void OnDestroy()
    {
        RestoreOriginalTrees();
    }

    void CacheOriginalTrees()
    {
        if (terrain == null || terrain.terrainData == null) return;
        TreeInstance[] src = terrain.terrainData.treeInstances;
        originalTreeInstances = new TreeInstance[src.Length];
        System.Array.Copy(src, originalTreeInstances, src.Length);
    }

    void RestoreOriginalTrees()
    {
        if (terrain == null || terrain.terrainData == null || originalTreeInstances == null) return;
        terrain.terrainData.treeInstances = originalTreeInstances;
        terrain.Flush();
    }

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            TriggerTransition();
        }
    }

    public void TriggerTransition()
    {
        if (transitioned) return;
        transitioned = true;
        ApplyDayState();
    }

    void ApplyInitialState()
    {
        SwapTreeIndices(lushTreeIndexA, deadTreeIndexA, lushTreeIndexB, deadTreeIndexB);

        if (river != null) river.SetActive(false);

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] != null) fireParticles[i].SetActive(true);
        }

        if (nightSkybox != null) RenderSettings.skybox = nightSkybox;
        RenderSettings.fogColor = nightFogColor;
        DynamicGI.UpdateEnvironment();
    }

    void ApplyDayState()
    {
        SwapTreeIndices(deadTreeIndexA, lushTreeIndexA, deadTreeIndexB, lushTreeIndexB);

        if (river != null) river.SetActive(true);

        for (int i = 0; i < fireParticles.Length; i++)
        {
            if (fireParticles[i] != null) fireParticles[i].SetActive(false);
        }

        if (daySkybox != null) RenderSettings.skybox = daySkybox;
        RenderSettings.fogColor = dayFogColor;
        DynamicGI.UpdateEnvironment();
    }

    void SwapTreeIndices(int fromA, int toA, int fromB, int toB)
    {
        if (terrain == null || terrain.terrainData == null) return;

        TerrainData data = terrain.terrainData;
        TreeInstance[] instances = data.treeInstances;

        for (int i = 0; i < instances.Length; i++)
        {
            int idx = instances[i].prototypeIndex;
            if (idx == fromA)
            {
                TreeInstance t = instances[i];
                t.prototypeIndex = toA;
                instances[i] = t;
            }
            else if (idx == fromB)
            {
                TreeInstance t = instances[i];
                t.prototypeIndex = toB;
                instances[i] = t;
            }
        }

        data.treeInstances = instances;
        terrain.Flush();
    }
}
