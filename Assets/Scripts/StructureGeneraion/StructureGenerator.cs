using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class StructureGenerator : MonoBehaviour
{

    public MarchingCubes marchingCubes;
    public Material baseStructureMaterial;
    public BaseDensityDescriptor densityDescriptor;
    public Vector3Int size;
    public bool generateStructure;
    List<MeshFilter> meshFilters = new List<MeshFilter>();

    public virtual void Generate()
    {
        float[,,] densityData = new float[size.x,size.y,size.z];
        densityDescriptor.InsertDensity(Vector3Int.zero,ref densityData);
        marchingCubes.InitMeshGeneration(size,densityData);
        List<ChunkData> chunksData = marchingCubes.StartMarchingCubes();
        GenerateMesh(chunksData);
    }


    private void Update() {
        if(generateStructure)
        {
            generateStructure = false;
            Generate();
        }
    }


    public void GenerateMesh(List<ChunkData> chunksData)
    {
        if(meshFilters.Count < chunksData.Count){
            while(meshFilters.Count < chunksData.Count){
                GameObject go = new GameObject("MESH_"+(meshFilters.Count));
                go.transform.parent = transform;
                MeshFilter filter = go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<MeshRenderer>().sharedMaterial = baseStructureMaterial;
                meshFilters.Add(filter);
            }
        }

        if(meshFilters.Count > chunksData.Count){
            for(int i=chunksData.Count;i<meshFilters.Count;i++){
                DestroyImmediate(meshFilters[i].gameObject);
                meshFilters.RemoveAt(i);
            }
        }

        for(int i=0;i<chunksData.Count;i++){
            Mesh m = new Mesh();
            m.Clear();
            m.SetVertices(chunksData[i].vertexData);
            m.SetTriangles(chunksData[i].triangleData,0);
            m.SetNormals(chunksData[i].normalData);
            m.RecalculateBounds();
            m.RecalculateTangents();
            if(chunksData[i].vertexData.Count > 0){
               Unwrapping.GenerateSecondaryUVSet(m);
            }
 
            meshFilters[i].sharedMesh = m;
            meshFilters[i].transform.localPosition = chunksData[i].chunkPosition;
        }
    }


}


