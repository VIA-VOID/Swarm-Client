using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridMapScannerDOTS : MonoBehaviour
{
    public float tileSize = 1f;
    public float minHeight = -168f;
    public float maxHeight = -163.5f;
    public string[] blockTags = { "Prop", "Tree", "Rock" }; // 장애물 태그만 유지

    private NativeArray<byte> resultMap;
    private NativeArray<Bounds> obstacleBounds;
    private NativeArray<float> heightMap;

    private byte[] resultMapCopy;

    public Renderer texturePreviewRenderer; // Quad의 MeshRenderer
    public GameObject textureQuad;  

    private Terrain terrain;
    private int mapWidth;
    private int mapHeight;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        mapWidth = Mathf.RoundToInt(terrain.terrainData.size.x / tileSize);
        mapHeight = Mathf.RoundToInt(terrain.terrainData.size.z / tileSize);
        Scan();
    }

    void Scan()
    {
        // 1. 장애물 오브젝트 찾기 (태그 기준 필터링)
        List<Bounds> boundsList = new List<Bounds>();
        float checkRange = Mathf.Max(mapWidth, mapHeight) * tileSize * 1.5f; // 여유를 두고
        Vector3 center = terrain.transform.position + new Vector3(mapWidth / 2f, 0, mapHeight / 2f);

        foreach (string tag in blockTags)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in taggedObjects)
            {
                float dist = Vector3.Distance(center, obj.transform.position);
                if (dist > checkRange) continue;
                Collider col = obj.GetComponent<Collider>();
                Debug.Log($"[TagCheck] {obj.name} 태그={obj.tag}, collider={(col != null ? "O" : "X")}");

                if (col != null)
                {
                    boundsList.Add(col.bounds);

#if UNITY_EDITOR
                    // 시각화
                    debugBounds.Add((col.bounds.center, col.bounds.size, Color.red));
#endif
                }
                else
                {
                    // Collider가 없더라도 해당 위치를 장애물로 간주
                    Bounds fallbackBounds = new Bounds(obj.transform.position + Vector3.up * 1.5f, new Vector3(tileSize, 3f, tileSize));
                    boundsList.Add(fallbackBounds);

#if UNITY_EDITOR
                    debugBounds.Add((fallbackBounds.center, fallbackBounds.size, Color.yellow));
#endif
                }
            }
        }

        obstacleBounds = new NativeArray<Bounds>(boundsList.ToArray(), Allocator.TempJob);
        resultMap = new NativeArray<byte>(mapWidth * mapHeight, Allocator.TempJob);
        heightMap = new NativeArray<float>(mapWidth * mapHeight, Allocator.TempJob);

        // Terrain 높이 미리 샘플링
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 worldPos = terrain.transform.position + new Vector3(x * tileSize, 0, y * tileSize);
                float height = terrain.SampleHeight(worldPos) + terrain.transform.position.y;
                heightMap[y * mapWidth + x] = height;
            }
        }

        GridCheckJob job = new GridCheckJob
        {
            mapWidth = mapWidth,
            tileSize = tileSize,
            startPosition = terrain.transform.position,
            obstacleBounds = obstacleBounds,
            result = resultMap,
            heightMap = heightMap,
            minHeight = minHeight,
            maxHeight = maxHeight
        };

        JobHandle handle = job.Schedule(resultMap.Length, 64);
        handle.Complete();

        Debug.Log("맵 스캔 완료!");

        resultMapCopy = new byte[resultMap.Length];
        resultMap.CopyTo(resultMapCopy);

        SaveResultToJson();
        CreateTexturePreview();

        obstacleBounds.Dispose();
        heightMap.Dispose();
        resultMap.Dispose();
    }

    void SaveResultToJson()
    {
        StringBuilder json = new StringBuilder();

        json.AppendLine("{");
        json.AppendLine($"\t\"mapSize\": {{");
        json.AppendLine($"\t\t\"width\": {mapWidth},");
        json.AppendLine($"\t\t\"height\": {mapHeight}");
        json.AppendLine("\t},");
        json.AppendLine("\t\"mapData\": [");

        for (int y = 0; y < mapHeight; y++)
        {
            json.Append("\t\t\"");
            for (int x = mapWidth - 1; x >= 0; x--) // 좌우 대칭으로 저장
            {
                json.Append(resultMap[y * mapWidth + x] == 1 ? "0" : "1");
            }
            json.AppendLine(y < mapHeight - 1 ? "\"," : "");
        }

        json.AppendLine("\t]");
        json.AppendLine("}");

        string path = Application.dataPath + "/mapData.json";
        File.WriteAllText(path, json.ToString());

        Debug.Log("맵 JSON 저장 완료: " + path);
    }

    [BurstCompile]
    public struct GridCheckJob : IJobParallelFor
    {
        public int mapWidth;
        public float tileSize;
        public Vector3 startPosition;
        public float minHeight;
        public float maxHeight;

        [ReadOnly] public NativeArray<Bounds> obstacleBounds;
        [ReadOnly] public NativeArray<float> heightMap;
        public NativeArray<byte> result;

        public void Execute(int index)
        {
            int x = index % mapWidth;
            int y = index / mapWidth;
            
            float sampledHeight = heightMap[y * mapWidth + x];
            
            Vector3 tileCenter = startPosition + new Vector3(x + 0.5f, 0, y + 0.5f) * tileSize;
            Vector3 boundsCenter = new Vector3(tileCenter.x, sampledHeight + 1.5f, tileCenter.z); // 지형 높이에 맞춰 올림
            Vector3 boundsSize = new Vector3(tileSize, 3f, tileSize);

            Bounds tileBounds = new Bounds(boundsCenter, boundsSize);

            for (int i = 0; i < obstacleBounds.Length; i++)
            {
                if (tileBounds.Intersects(obstacleBounds[i]))
                {
                    result[index] = 1;
                    return;
                }
            }

            if (sampledHeight > maxHeight || sampledHeight < minHeight)
            {
                result[index] = 1;
                return;
            }

            result[index] = 0;
        }
    }

    void CreateTexturePreview()
    {
        Texture2D tex = new Texture2D(mapWidth, mapHeight, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Point;

        Color[] colors = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int index = y * mapWidth + x;
                byte value = resultMapCopy[index];
                colors[index] = value == 1 ? Color.red : Color.green;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();

        if (texturePreviewRenderer != null)
        {
            texturePreviewRenderer.sharedMaterial.mainTexture = tex;
        }

        if (textureQuad != null)
        {
            textureQuad.transform.localScale = new Vector3(mapWidth, mapHeight, 1);
            textureQuad.transform.position = terrain.transform.position + new Vector3(mapWidth / 2f, 0, mapHeight / 2f);
            textureQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
    
#if UNITY_EDITOR
    private List<(Vector3 center, Vector3 size, Color color)> debugBounds = new();

    void OnDrawGizmos()
    {
        if (debugBounds == null) return;

        foreach (var (center, size, color) in debugBounds)
        {
            Handles.color = color;
            Handles.DrawWireCube(center, size);
        }
    }
#endif
}
