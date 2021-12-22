using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DensityChunkData{
    public float[,,] chunkDensity;
    public Vector3 position;
    public Vector3Int size;

    public float ISOLevel;

}

[ExecuteInEditMode]
public class MarchingCubes : MonoBehaviour
{
    public float[,,] densities;
    public Vector3Int size = new Vector3Int(64,16,64);
    public Vector3Int chunk_size = new Vector3Int(16,16,16);
    public float ISOLevel = .5f;
    public bool startMarchingCubes = false;
    public MazeGeneratorCellular mazeGen;

    public Material baseMeshMaterial;

    List<DensityChunkData> chunkDatabase;

    public float[,,] GenerateDensities(Vector3Int size){
        float[,,] densities = new float[size.x+4,size.y+4,size.z+4];

        for(int i=0;i<size.x;i++)
        {
            for(int j=0;j<size.y;j++)
            {
                for(int k=0;k<size.z;k++)
                {
                    float t = (float)j/(size.y);
                    if(Random.Range(0f,1f) >.5f){
                        densities[i,j,k] = t+Random.Range(0,.1f);
                    }else{
                        densities[i,j,k] = t;
                    }
                    
                }
            }
        }

        return densities;
    }


    public DensityChunkData SplitChunk(Vector3Int position, Vector3Int size){
        DensityChunkData chunkData=new DensityChunkData();
        float[,,] local_density = new float[size.x,size.y,size.z];
        for(int x=position.x, lx=0;x < position.x+size.x; x++,lx++){
            for(int y=position.y, ly=0;y < position.y+size.y; y++, ly++)
            {
                for(int z=position.z, lz=0; z<position.z+size.z;z++, lz++){
                    local_density[lx,ly,lz] = densities[x,y,z];
                }
            }
        }
        chunkData.chunkDensity = local_density;
        chunkData.position = position;
        chunkData.size = size;
        chunkData.ISOLevel = ISOLevel;
        return chunkData;
    }

    public void SplitChunks(Vector3Int chunk_size){
        int xcount = 0;
        int zcount = 0;
        int ycount =0;
        for(int x=0;x<size.x;x+=chunk_size.x){
            for(int y=0;y<size.x;y+=chunk_size.y){
                for(int z=0;z<size.z;z+=chunk_size.z){
                    chunkDatabase.Add(SplitChunk(new Vector3Int(x-xcount,y-ycount,z-zcount), chunk_size));
                    zcount++;
                }
                zcount = 0;
                ycount++;
            }
            ycount=0;
            xcount++;
        }
    }


    public List<VoxelData> StartMarchingCubes(){
        SplitChunks(chunk_size);
        MarchingCubesChunk chunk_march = new MarchingCubesChunk();
        List<VoxelData> voxelDatas = new List<VoxelData>();
        for(int i=0;i<chunkDatabase.Count;i++){
            chunk_march.Init(chunkDatabase[i]);
            voxelDatas.Add(chunk_march.Generate());
        }
        return voxelDatas;
    }

    public void GenerateMesh(List<VoxelData> voxelDatas, List<DensityChunkData> densityChunks)
    {
        for(int i=0;i<voxelDatas.Count;i++){
            Mesh m = new Mesh();
            m.Clear();
            m.SetVertices(voxelDatas[i].vertexData);
            m.SetTriangles(voxelDatas[i].triangleData,0);
            m.SetNormals(voxelDatas[i].normalData);
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();
            if(voxelDatas[i].vertexData.Count > 0){
                Unwrapping.GenerateSecondaryUVSet(m);
            }
 

            GameObject go = new GameObject("MESH_"+i);
            go.transform.parent = transform;
            MeshFilter filter = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshRenderer>().sharedMaterial = baseMeshMaterial;
            filter.sharedMesh = m;
            go.transform.localPosition = densityChunks[i].position;
            Debug.Log("MESH_"+i);
        }
    }

    public void InitMeshGeneration(Vector3Int size, float[,,] densities)
    {
        chunkDatabase = new List<DensityChunkData>();
        this.size = size;
        this.densities = densities;
    }

    public void GenerateMesh(){
        densities = mazeGen.GenerateDensities();
        List<VoxelData> voxelDatas = StartMarchingCubes();
        GenerateMesh(voxelDatas,chunkDatabase);
    }


    void Update()
    {
        if(startMarchingCubes)
        {
            chunkDatabase = new List<DensityChunkData>();
            size = new Vector3Int(mazeGen.size.x*4,16,mazeGen.size.y*4);
            densities = mazeGen.GenerateDensities();
            List<VoxelData> voxelDatas = StartMarchingCubes();
            GenerateMesh(voxelDatas,chunkDatabase);
            startMarchingCubes = false;
        }
    }
}
