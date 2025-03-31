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
    public int mapWidth = 5000;
    public int mapHeight = 5000;
    public float tileSize = 1f;
    public float maxSlopeAngle = 45f;
    public float maxHeightThreshold = 3f;

    public string[] waterTags = { "Water" };
    public string[] terrainTags = { "Terrain" };
    public string[] blockerTags = { "Prop", "Tree", "Rock" };

    public Renderer texturePreviewRenderer;
    public GameObject textureQuad;

    private NativeArray<Bounds> waterBounds;
    private NativeArray<Bounds> terrainBounds;
    private NativeArray<Bounds> blockerBounds;
    private NativeArray<byte> resultMap;
    private byte[] resultMapCopy;

    private Terrain terrain;
    private TerrainData terrainData;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;
        Scan();
    }

    void Scan()
    {
        List<Bounds> waterList = CollectBoundsByTags(waterTags);
        List<Bounds> terrainList = CollectBoundsByTags(terrainTags);
        List<Bounds> blockerList = CollectBoundsByTags(blockerTags);

        waterBounds = new NativeArray<Bounds>(waterList.ToArray(), Allocator.TempJob);
        terrainBounds = new NativeArray<Bounds>(terrainList.ToArray(), Allocator.TempJob);
        blockerBounds = new NativeArray<Bounds>(blockerList.ToArray(), Allocator.TempJob);
        resultMap = new NativeArray<byte>(mapWidth * mapHeight, Allocator.TempJob);

        GridCheckJob job = new GridCheckJob
        {
            mapWidth = mapWidth,
            mapHeight = mapHeight,
            tileSize = tileSize,
            startPosition = transform.position,
            maxSlopeAngle = maxSlopeAngle,
            maxHeight = maxHeightThreshold,
            waterBounds = waterBounds,
            terrainBounds = terrainBounds,
            blockerBounds = blockerBounds,
            result = resultMap
        };

        JobHandle handle = job.Schedule(mapWidth * mapHeight, 64);
        handle.Complete();

        Debug.Log("맵 스캔 완료!");

        resultMapCopy = new byte[resultMap.Length];
        resultMap.CopyTo(resultMapCopy);

        CreateTexturePreview();

        waterBounds.Dispose();
        terrainBounds.Dispose();
        blockerBounds.Dispose();
        resultMap.Dispose();
    }

    List<Bounds> CollectBoundsByTags(string[] tags)
    {
        List<Bounds> boundsList = new();
        foreach (string tag in tags)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (obj.TryGetComponent(out Collider col))
                    boundsList.Add(col.bounds);
            }
        }
        return boundsList;
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
                colors[index] = resultMapCopy[index] == 1 ? Color.red : Color.green;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        if (texturePreviewRenderer != null)
            texturePreviewRenderer.sharedMaterial.mainTexture = tex;

        if (textureQuad != null)
        {
            textureQuad.transform.localScale = new Vector3(mapWidth, mapHeight, 1);
            textureQuad.transform.position = transform.position + new Vector3(mapWidth / 2f, 0, mapHeight / 2f);
            textureQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    [BurstCompile]
    public struct GridCheckJob : IJobParallelFor
    {
        public int mapWidth;
        public int mapHeight;
        public float tileSize;
        public Vector3 startPosition;
        public float maxSlopeAngle;
        public float maxHeight;

        [ReadOnly] public NativeArray<Bounds> waterBounds;
        [ReadOnly] public NativeArray<Bounds> terrainBounds;
        [ReadOnly] public NativeArray<Bounds> blockerBounds;
        public NativeArray<byte> result;

        public void Execute(int index)
        {
            int x = index % mapWidth;
            int y = index / mapWidth;

            Vector3 worldPos = startPosition + new Vector3(x + 0.5f, 0f, y + 0.5f) * tileSize;
            Bounds tileBounds = new Bounds(worldPos + Vector3.up * 0.5f, Vector3.one * tileSize);

            // 1. Blocker (Prop, Tree, Rock)
            for (int i = 0; i < blockerBounds.Length; i++)
                if (tileBounds.Intersects(blockerBounds[i])) { result[index] = 1; return; }

            // 2. Water only?
            bool hasWater = false;
            for (int i = 0; i < waterBounds.Length; i++)
                if (tileBounds.Intersects(waterBounds[i])) { hasWater = true; break; }

            bool hasTerrain = false;
            for (int i = 0; i < terrainBounds.Length; i++)
                if (tileBounds.Intersects(terrainBounds[i])) { hasTerrain = true; break; }

            if (hasWater && !hasTerrain) { result[index] = 1; return; }

            // 3. Terrain 검사: 높이 + 경사도
            float height = SampleTerrainHeight(worldPos);
            float slope = SampleTerrainSlope(worldPos);

            if (height > maxHeight || slope > maxSlopeAngle)
            {
                result[index] = 1;
                return;
            }

            result[index] = 0;
        }

        float SampleTerrainHeight(Vector3 worldPos)
        {
#if UNITY_EDITOR
            Terrain terrain = Terrain.activeTerrain;
            return terrain.SampleHeight(worldPos);
#else
            return 0f;
#endif
        }

        float SampleTerrainSlope(Vector3 worldPos)
        {
#if UNITY_EDITOR
            Terrain terrain = Terrain.activeTerrain;
            TerrainData data = terrain.terrainData;
            Vector3 localPos = terrain.transform.InverseTransformPoint(worldPos);
            float normX = localPos.x / data.size.x;
            float normZ = localPos.z / data.size.z;
            Vector3 normal = data.GetInterpolatedNormal(normX, normZ);
            return Vector3.Angle(Vector3.up, normal);
#else
            return 0f;
#endif
        }
    }
}