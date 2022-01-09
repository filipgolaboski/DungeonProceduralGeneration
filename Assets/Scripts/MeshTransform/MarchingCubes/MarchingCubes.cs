using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class DensityChunkData{
    public float[,,] chunkDensity;
    public Vector3Int position;
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


    List<DensityChunkData> chunkDatabase;

    public DensityChunkData SplitChunk(Vector3Int position, Vector3Int chunkSize){
        DensityChunkData densityChunkData=new DensityChunkData();
        Vector3Int newSize = chunkSize+Vector3Int.one*2;
        float[,,] local_density = new float[newSize.x,newSize.y,newSize.z];
        for(int x=position.x-1, lx=0;x < position.x+newSize.x-1; x++,lx++){
            for(int y=position.y-1, ly=0;y < position.y+newSize.y-1; y++, ly++)
            {
                for(int z=position.z-1, lz=0; z<position.z+newSize.z-1;z++, lz++){
                    if(z<0 || z>=size.z || y<0 || y>=size.y || x<0 || x>=size.x){
                        local_density[lx,ly,lz] = 0;
                    }else{
                        local_density[lx,ly,lz] = densities[x,y,z];
                    }
                    
                }
            }
        }
        densityChunkData.chunkDensity = local_density;
        densityChunkData.position = position;
        densityChunkData.size = newSize;
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


    public List<ChunkData> StartMarchingCubes(){
        SplitChunks(chunk_size);
        MarchingCubesChunk chunk_march = new MarchingCubesChunk();
        List<ChunkData> chunksData = new List<ChunkData>();
        for(int i=0;i<chunkDatabase.Count;i++){
            chunk_march.Init(chunkDatabase[i]);
            chunksData.Add(chunk_march.Generate());
        }
        return chunksData;
    }

    public void InitMeshGeneration(Vector3Int size, float[,,] densities)
    {
        chunkDatabase = new List<DensityChunkData>();
        this.size = size;
        this.densities = densities;
    }

}
