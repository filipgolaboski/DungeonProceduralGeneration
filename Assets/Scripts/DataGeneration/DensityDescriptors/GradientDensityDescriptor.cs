using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/GradientDensityDescriptor")]
public class GradientDensityDescriptor : BaseDensityDescriptor
{
    public float scale;
    public Vector3Int direction;
    public bool inverted;

    public override float GetDensity(Vector3Int point)
    {
        Vector3 sizeStepped = (Vector3)size * scale;
        Vector3 truePostion = (!inverted ? (point + position) * direction : (size - point + position) * direction);
        Vector3 directedGrad = new Vector3(truePostion.x / sizeStepped.x, truePostion.y / sizeStepped.y, truePostion.z / sizeStepped.z);

        return (directedGrad.x + directedGrad.y + directedGrad.z)/(direction.x+direction.y+direction.z);
    }
}
