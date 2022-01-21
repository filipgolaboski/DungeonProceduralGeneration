using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;


public class DensityChunkData{
    public BaseDensityDescriptor chunkDensity;
    public Vector3Int position;
    public Vector3Int size;

    public float ISOLevel;

}

[ExecuteInEditMode]
public class MarchingCubes : MonoBehaviour
{
    BaseDensityDescriptor densities;
    public Vector3Int size = new Vector3Int(64,16,64);
    public Vector3Int chunk_size = new Vector3Int(16,16,16);
    public float ISOLevel = .5f;


    List<DensityChunkData> chunkDatabase;

    public DensityChunkData SplitChunk(Vector3Int position, Vector3Int chunkSize){
        DensityChunkData densityChunkData=new DensityChunkData();
        densityChunkData.chunkDensity = densities;
        densityChunkData.position = position;
        densityChunkData.size = chunkSize;
        densityChunkData.ISOLevel = ISOLevel;
        return densityChunkData;
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


    public async Task<List<ChunkData>> StartMarchingCubesAsync(){
        SplitChunks(chunk_size);
        MarchingCubesChunk chunk_march = new MarchingCubesChunk();
        List<Task<ChunkData>> chunkTasks = new List<Task<ChunkData>>();
        List<ChunkData> chunksData = new List<ChunkData>();
        for (int i = 0; i < chunkDatabase.Count; i++)
        {
            Debug.Log(i);
            chunk_march.Init(chunkDatabase[i]);
            chunkTasks.Add(chunk_march.Generate());
        }

        for(int i = 0; i < chunkTasks.Count; i++)
        {
            ChunkData d = await chunkTasks[i];
            chunksData.Add(d);
        }

        return chunksData;
    }

    public void InitMeshGeneration(Vector3Int size, BaseDensityDescriptor densities)
    {
        chunkDatabase = new List<DensityChunkData>();
        this.size = size;
        this.densities = densities;
    }

}
