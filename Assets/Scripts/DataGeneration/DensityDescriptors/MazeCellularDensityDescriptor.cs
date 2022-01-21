using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/CellularMaze")]
public class MazeCellularDensityDescriptor : BaseDensityDescriptor
{
    public BaseDensityDescriptor baseDescriptor;
    public List<BaseDensityDescriptor> carverDescriptors;
    public BaseDensityDescriptor noiseDescriptor;
    public int carverHeight;
    public int distanceMultiplier;
    public float carverSubstractionSmooth = 1.3f;
    public bool addNoise = false;
    float[,,] density_map;


    public Vector2Int cellMapSize = new Vector2Int(64, 64);
    public MazeCell[,] mazeGraph;

    public bool startGenerating;
    [SerializeField]
    private bool isGenerating = false;
    public int numberOfGenerations;
    public int generationMultiplier = 32;
    int currentGeneration;


    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= EditorUpdate;
    }


    public void Generate()
    {
        SeedGraph();
        currentGeneration = numberOfGenerations;
        while (currentGeneration > 0)
        {
            LifeCycle();
            currentGeneration--;
        }
    }

    void EditorUpdate()
    {
        if (!isGenerating)
        {
            isGenerating = startGenerating;
            startGenerating = false;
            if (isGenerating)
            {
                SeedGraph();
                currentGeneration = numberOfGenerations;
            }
        }

        if (isGenerating)
        {
            if (currentGeneration > 0)
            {
                LifeCycle();
                currentGeneration--;
                Debug.Log(currentGeneration);
            }
            else
            {
                isGenerating = false;
                //Instantiate();
                GenerateDensity();
            }
        }
    }


    public void LifeCycle()
    {
        for (int counter = 0; counter < generationMultiplier; counter++)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    if (!mazeGraph[i, j].isBorn())
                    {
                        mazeGraph[i, j].Birth(mazeGraph, i, j);
                    }
                    else
                    {
                        mazeGraph[i, j].Death(mazeGraph, i, j);
                    }


                }
            }
        }

    }

    public void SeedGraph()
    {
        mazeGraph = new MazeCell[size.x, size.y];
        Vector2Int halfSize = cellMapSize / 2;
        for (int i = halfSize.x - halfSize.x / 2; i < halfSize.x + halfSize.x / 2; i++)
        {
            for (int j = halfSize.y - halfSize.y / 2; j < halfSize.y + halfSize.y / 2; j++)
            {
                mazeGraph[i, j] = new MazeCell(Random.Range(0f, 1f) > 0.5f);
            }
        }

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (mazeGraph[i, j] == null)
                {
                    mazeGraph[i, j] = new MazeCell(false);
                }
            }
        }
    }



    private void GenerateDensity()
    {

        int xStep = size.x / cellMapSize.x;
        int zStep = size.z / cellMapSize.y;

        density_map = new float[size.x, size.y, size.z];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    density_map[x, y, z] = baseDescriptor.GetDensity(new Vector3Int(x,y,z));
                }
            }
        }


        for (int i=0;i<cellMapSize.x;i++)
        {
            for(int j=0;j<cellMapSize.y;j++)
            {
                if (mazeGraph[i, j].isAlive())
                {
                    CarveTerrain(i,j,ref density_map);
                }
            }
        }

       
    }

    public void CarveTerrain(int mapX,int mapZ, ref float[,,] densities)
    {
        int cellSizeX = size.x / cellMapSize.x;
        int cellSizeZ = size.z / cellMapSize.y;
        int centerX = mapX * cellSizeX;
        int centerZ = mapZ * cellSizeZ;
        int startX = centerX - (cellSizeX / 2) - mapX*distanceMultiplier;
        int startZ = centerZ - (cellSizeZ / 2) - mapZ*distanceMultiplier;
        BaseDensityDescriptor carverDescriptor = carverDescriptors[Random.Range(0, carverDescriptors.Count)];
        for(int x = startX; x < startX + cellSizeX; x++)
        {
            for (int y = carverHeight; y < carverHeight + carverDescriptor.size.y; y++)
            {
                for (int z = startZ; z < startZ + cellSizeZ; z++)
                {
                    if (x >= 0 && x < size.x && y>=0 && y<size.y && z>=0 && z < size.z)
                    {
                        float result = densities[x, y, z];
                        float sphereDensity = carverDescriptor.GetDensity(new Vector3Int(x - startX, y - carverHeight, z - startZ), new Vector3Int(carverDescriptor.size.x / 2, carverDescriptor.size.y / 2, carverDescriptor.size.z / 2)); ;
                        if (addNoise)
                        {
                            Utilities.Intersection_Smooth(noiseDescriptor.GetDensity(new Vector3Int(x, y, z)), sphereDensity, 1.3f, out sphereDensity);
                        }
                        
                        Utilities.Substraction_Smooth(sphereDensity,densities[x, y, z], carverSubstractionSmooth, out result);
                        densities[x, y, z] = result;
                    }
                }
            }
        }

    }
  
    public override float GetDensity(Vector3Int point)
    {
        return density_map[point.x, point.y, point.z];
    }
}
