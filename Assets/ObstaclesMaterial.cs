using UnityEngine;

public class ObstacleMaterial : MonoBehaviour
{
    public enum MaterialType
    {
        Wood,
        Brick,
        Concrete,
        Glass,
        Metal
    }

    public MaterialType materialType;

    public float GetLoss()
    {
        switch (materialType)
        {
            case MaterialType.Wood: return 5f;
            case MaterialType.Brick: return 8f;
            case MaterialType.Concrete: return 15f;
            case MaterialType.Glass: return 3f;
            case MaterialType.Metal: return 30f;
            default: return 10f;
        }
    }
}