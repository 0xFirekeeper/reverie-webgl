using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 0 = Default (Orange)
// 1 = Yellow
// 2 = Brown
// 3 = Green
// 4 = Black

public class MaterialsManager : MonoBehaviour
{
    public Material[] orderedMaterials;

    public static MaterialsManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetDragonSkin(string Identifier)
    {
        int materialId;
        switch (Identifier)
        {
            case "Default":
                materialId = 0;
                break;
            case "Yellow":
                materialId = 1;
                break;
            case "Brown":
                materialId = 2;
                break;
            case "Green":
                materialId = 3;
                break;
            case "Black":
                materialId = 4;
                break;
            default:
                materialId = 0;
                break;
        }

        GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponentInChildren<SkinnedMeshRenderer>().material = orderedMaterials[materialId];
    }
}
