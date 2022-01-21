using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/Union")]
public class UnionDensityDescriptor : BaseDensityDescriptor
{
    public BaseDensityDescriptor lhsDensityDescriptor;
    public BaseDensityDescriptor rhsDensityDescriptor;
    public float smoothing=1;

    public override float GetDensity(Vector3Int point)
    {
        float densityOne = lhsDensityDescriptor.GetDensity(point);
        float densityTwo = rhsDensityDescriptor.GetDensity(point);
        float h = Mathf.Clamp(0.5f + .5f * (densityTwo - densityOne) / smoothing,0f,1f);
        return Mathf.Lerp(densityTwo, densityOne, h) + smoothing * h * (1 - h);

    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        float densityOne = lhsDensityDescriptor.GetDensity(point,position);
        float densityTwo = rhsDensityDescriptor.GetDensity(point,position);
        float h = Mathf.Clamp(0.5f + .5f * (densityTwo - densityOne) / smoothing, 0f, 1f);
        return Mathf.Lerp(densityTwo, densityOne, h) + smoothing * h * (1 - h);
    }

}
