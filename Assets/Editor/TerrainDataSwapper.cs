using UnityEngine;
using UnityEditor;

public class TerrainDataSwapper : EditorWindow
{
    TerrainData newTerrainData;

    [MenuItem("Tools/Terrain Data Swapper")]
    static void Open()
    {
        GetWindow<TerrainDataSwapper>("Terrain Data Swapper");
    }

    void OnGUI()
    {
        GUILayout.Label("選取 Terrain GameObject 後，指定新的 TerrainData");
        newTerrainData = (TerrainData)EditorGUILayout.ObjectField(
            "New Terrain Data", newTerrainData, typeof(TerrainData), false);

        if (GUILayout.Button("Swap"))
        {
            GameObject go = Selection.activeGameObject;
            Terrain t = go.GetComponent<Terrain>();
            TerrainCollider tc = go.GetComponent<TerrainCollider>();

            t.terrainData = newTerrainData;
            tc.terrainData = newTerrainData;

            EditorUtility.SetDirty(go);
            Debug.Log("換完了：" + newTerrainData.name);
        }
    }
}