using System.Collections.Generic;
using UnityEngine;

public class LODObject : MonoBehaviour
{
    public GameObject[] objectsToDisableInLOD1;

    private LODGroup lodGroup;
    private int currentLOD = -1;

    void Start()
    {
        lodGroup = GetComponent<LODGroup>();
        InitializeLOD1Objects();
    }

    void Update()
    {
        int lodIndex = GetCurrentLODIndex();

        if (lodIndex != currentLOD)
        {
            currentLOD = lodIndex;

            if (lodIndex != 0)
            {
                Debug.Log("0아님");
            }
            // LOD 0 외에는 모두 비활성화
            SetObjectsActive(lodIndex != 0);
        }
    }

    private void InitializeLOD1Objects()
    {
        if (lodGroup == null) return;

        LOD[] lods = lodGroup.GetLODs();
        if (lods.Length > 1)
        {
            List<GameObject> list = new List<GameObject>();

            foreach (Transform child in lods[1].renderers[0].transform)
            {
                if (child != null)
                {
                    list.Add(child.gameObject);
                }
            }
            
            objectsToDisableInLOD1 = list.ToArray();
        }
    }
    
    private void SetObjectsActive(bool active)
    {
        foreach (var obj in objectsToDisableInLOD1)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
    
    private int GetCurrentLODIndex()
    {
        Camera camera = Camera.main;
        if (lodGroup == null || camera == null)
            return -1;

        float relativeHeight = GetWorldSpaceRelativeHeight(camera, lodGroup);
        LOD[] lods = lodGroup.GetLODs();

        float accumulated = 1f;
        for (int i = 0; i < lods.Length; i++)
        {
            if (relativeHeight >= accumulated - lods[i].screenRelativeTransitionHeight)
                return i;
            accumulated -= lods[i].screenRelativeTransitionHeight;
        }

        return lods.Length - 1; // 마지막 LOD
    }

    private float GetWorldSpaceRelativeHeight(Camera camera, LODGroup group)
    {
        float distance = Vector3.Distance(camera.transform.position, group.transform.position);
        float height = camera.pixelHeight;
        
        return height / (distance * Mathf.Tan(0.5f * camera.fieldOfView * Mathf.Deg2Rad));
    }
}
