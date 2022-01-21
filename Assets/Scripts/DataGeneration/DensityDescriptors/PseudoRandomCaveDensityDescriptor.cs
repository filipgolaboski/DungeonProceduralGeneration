using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/PseudoRandomCaves")]
public class PseudoRandomCaveDensityDescriptor : BaseDensityDescriptor
{
    public SimplexNoiseDensityDescriptor noiseGenerator;
    public SimplexNoiseDensityDescriptor noiseDescriptor;
    public BaseDensityDescriptor[] rooms;

    public bool generate;
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= EditorUpdate;
    }

    public void EditorUpdate()
    {
        if (generate)
        {
            Generate();
            generate = false;
        }
    }


    public void Generate()
    {
        List<Vector3Int> points = new List<Vector3Int>();

        for(int x = 0; x < noiseGenerator.size.x; x++)
        {
            for (int y = 0; y < noiseGenerator.size.x; y++)
            {
                for (int z = 0; z < noiseGenerator.size.x; z++)
                {
                    float val = noiseGenerator.GetDensity(new Vector3Int(x, y, z));
                    if (val < 0.7f && val > 0.4f)
                    {
                        points.Add(new Vector3Int(x, y, z));
                        
                    }
                }
            }
        }

        for(int i = 0; i < rooms.Length; i++)
        {
            rooms[i].position = points[Random.Range(0, points.Count)];
        }
    }

    public override float GetDensity(Vector3Int point)
    {
        float val = rooms[0].GetDensity(point);

        for(int i = 1; i < rooms.Length; i++)
        {
            val = Mathf.Min(val, rooms[i].GetDensity(point));
        }

        return val;
    }


}
