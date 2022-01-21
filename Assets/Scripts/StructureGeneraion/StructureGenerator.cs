using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using System.Threading.Tasks;

public struct ChunkTaskData
{
    public List<NativeArray<float>> densitiesList;
    public List<Vector3> positions;
}

[ExecuteInEditMode]
public class StructureGenerator : MonoBehaviour
{

    public MarchingCubes marchingCubes;
    public Material baseStructureMaterial;
    public BaseDensityDescriptor densityDescriptor;

    public Vector3Int size;
    public int chunkSize = 64;
    public int subChunkSize = 16;
    public float isoLevel = 0.5f;
    public bool generateStructure;

    List<Vector3> localPositions = new List<Vector3>();
    bool separated = false;
    List<Vector3Int> offsets = new List<Vector3Int>();
    int chunkIndex = 0;
    List<Chunk> meshChunks = new List<Chunk>();

    public float[,,] densities_matrix;


    private void OnEnable()
    {
        EditorApplication.update += UpdateEditor;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
    }


    public void UpdateEditor()
    {
        if (generateStructure)
        {
            if (!separated)
            {
                offsets = SeparateStructure();
                separated = true;
            }
            else
            {
                GenerateChunks();
                generateStructure = false;
            }
        }
        else
        {
            separated = false;
        }
    }

    public async Task GenerateDensities()
    {
        List<Task<int>> densityTasks = new List<Task<int>>();
        for (int x = 0; x < size.x; x++)
        {
            int xT = x;
            densityTasks.Add(Task<int>.Run(async ()=> {return await GenerateDensityPlane(xT); }));
        }

        for(int i=0;i<densityTasks.Count;i++)
        {
            int x = await densityTasks[i];
        }
    }

    public async Task<int> GenerateDensityPlane(int x)
    {
        Debug.Log(x);
        for (int y = 0; y < size.y; y++)
        {
            for (int z = 0; z < size.z; z++)
            {
                densities_matrix[x, y, z] = densityDescriptor.GetDensity(new Vector3Int(x, y, z));
            }
        }
        return x;
    }

    async void GenerateChunks()
    {
        await GenerateDensities();
        List<Task<ChunkTaskData>> chunkTasks = new List<Task<ChunkTaskData>>();

        for (int i = 0; i < offsets.Count; i++)
        {
            Vector3Int offset = offsets[i];
            chunkTasks.Add(Task.Run(async ()=> {
                return await GenerateSubChunks(offset); 
            }));
        }


        for(int i=0;i<offsets.Count;i++)
        {
            ChunkTaskData c = await chunkTasks[i];

           // RegenerateChunk(c, offsets[i]);
            meshChunks[i].densitiesList = c.densitiesList;
            meshChunks[i].gameObject.transform.localPosition = (Vector3)(offsets[i] / chunkSize) * -(chunkSize / subChunkSize);
            meshChunks[i].Init(subChunkSize, chunkSize, offsets[i], c.positions, baseStructureMaterial);
            meshChunks[i].StartGeneration(isoLevel);
        }

    }
    

    public void RegenerateChunk(ChunkTaskData c, Vector3Int offset)
    {
        int tsizeY = size.y / chunkSize;
        int tsizeZ = size.z / chunkSize;
        Vector3Int t = offset / chunkSize;
        int index = t.x * tsizeZ * tsizeY + t.y * tsizeZ + t.z;
        meshChunks[index].densitiesList = c.densitiesList;
        meshChunks[index].gameObject.transform.localPosition = (Vector3)(offsets[index] / chunkSize) * -(chunkSize / subChunkSize);
        meshChunks[index].Init(subChunkSize, chunkSize, offsets[index], c.positions, baseStructureMaterial);
        meshChunks[index].StartGeneration(isoLevel);

    }

    public bool IsGenerating()
    {
        for(int i=0;i<meshChunks.Count;i++)
        {
            if(meshChunks[i].isGenerating)
            {
                return true;
            }
        }
        return false;
    }

    int Resize(int size)
    {
        if (size % chunkSize > 0)
        {
            size += chunkSize - size % chunkSize;
        }
        return size;
    }

    public List<Vector3Int> SeparateStructure()
    {
        densities_matrix = new float[size.x, size.y, size.z];
        localPositions.Clear();
        offsets.Clear();
        chunkIndex = 0;
        List<Vector3Int> coordinates = new List<Vector3Int>();
        if (size.x > chunkSize || size.y > chunkSize || size.z > chunkSize)
        {
            size.x = Resize(size.x);
            size.y = Resize(size.y);
            size.z = Resize(size.z);
            for (int x = 0; x < size.x; x += chunkSize)
            {
                for (int y = 0; y < size.y; y += chunkSize)
                {
                    for (int z = 0; z < size.z; z += chunkSize)
                    {
                        coordinates.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        else
        {
            coordinates.Add(new Vector3Int(0, 0, 0));
        }


        for (int i = 0; i < coordinates.Count; i++)
        {
            if(i > meshChunks.Count-1)
            {
                meshChunks.Add(null);
            }

            if(meshChunks[i] == null)
            {
                GameObject go = new GameObject("CHUNK_" + i);
                go.transform.SetParent(transform);
                meshChunks[i] = go.AddComponent<Chunk>();
            }
        }

        while (meshChunks.Count > coordinates.Count)
        {
            DestroyImmediate(meshChunks[meshChunks.Count - 1]);
            meshChunks.RemoveAt(meshChunks.Count - 1);
        }

        return coordinates;
    } 

    public async Task<ChunkTaskData> GenerateSubChunks(Vector3Int offset)
    {
        ChunkTaskData c = new ChunkTaskData();
        c.densitiesList = new List<NativeArray<float>>();
        c.positions = new List<Vector3>();
        int zpos = 0;
        int xpos = 0;
        int ypos = 0;

        int subChunkDataSize = subChunkSize + 2;
        for (int i = offset.x / subChunkSize; i < (chunkSize + offset.x) / subChunkSize; i++)
        {
            for (int j = offset.y / subChunkSize; j < (chunkSize + offset.y) / subChunkSize; j++)
            {
                for (int k = offset.z / subChunkSize; k < (chunkSize + offset.z) / subChunkSize; k++)
                {
                    c.positions.Add(new Vector3(i * subChunkSize - xpos, j * subChunkSize - ypos, k * subChunkSize - zpos));
                    NativeArray<float> f = await GetSubchunkDensities(subChunkDataSize, i, j, k);
                    c.densitiesList.Add(f);
                    zpos++;
                }
                zpos = 0;
                ypos++;
            }
            ypos = 0;
            xpos++;
        }

        return c;
    }

    async Task<NativeArray<float>> GetSubchunkDensities(int subChunkDataSize,int i,int j, int k)
    {
        NativeArray<float> densities = new NativeArray<float>(subChunkDataSize * subChunkDataSize * subChunkDataSize, Allocator.Persistent);
   
        for (int x = 0; x < subChunkDataSize; x++)
        {
            for (int y = 0; y < subChunkDataSize; y++)
            {
                for (int z = 0; z < subChunkDataSize; z++)
                { 
                    densities[x * subChunkDataSize * subChunkDataSize + y * subChunkDataSize + z] = densities_matrix[i * (subChunkSize - 1) + x, j * (subChunkSize - 1) + y, k * (subChunkSize - 1) + z];
                }
            }
        }
        return densities;
    }

}


