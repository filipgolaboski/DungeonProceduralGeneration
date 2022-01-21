using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/LineDensityDescriptor")]
public class LineDensityDescriptor : BaseDensityDescriptor
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float radius;

    public override float GetDensity(Vector3Int point)
    {
        Vector3 pSposition = point - startPosition;
        Vector3 startEnd = endPosition - startPosition;
        float h = Mathf.Clamp(Vector3.Dot(pSposition, startEnd) / Vector3.Dot(startEnd, startEnd), 0, 1);

        return ((Vector3.Distance(pSposition, startEnd * h) - radius) / radius + 1) * 0.5f;
    }

    public float GetDensity(Vector3Int point, Vector3Int startPosition, Vector3Int endPosition)
    {
        Vector3 pSposition = point - startPosition;
        Vector3 startEnd = endPosition - startPosition;
        float h = Mathf.Clamp(Vector3.Dot(pSposition, startEnd) / Vector3.Dot(startEnd, startEnd), 0, 1);

        return ((Vector3.Distance(pSposition, startEnd * h) - radius) / radius + 1) * 0.5f;
    }
}
