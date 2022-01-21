using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{

    List<MeshFilter> subChunks = new List<MeshFilter>();
    Material baseMaterial;


    List<JobHandle> jobHandles = new List<JobHandle>();
    List<MarchingCubesJob> jobs = new List<MarchingCubesJob>();
    List<Vector3> localPositions = new List<Vector3>();


    public List<NativeArray<float>> densitiesList = new List<NativeArray<float>>();
    List<NativeList<Vector3>> vertexList = new List<NativeList<Vector3>>();
    List<NativeList<Vector3>> normalsList = new List<NativeList<Vector3>>();
    List<NativeList<int>> trianglesList = new List<NativeList<int>>();
    List<int> finishedJobs = new List<int>();

    int subChunkSize;
    int chunkSize;
    Vector3 offset;
    int jobCount;
    int currentJobCount;
    int finishedJobCount;
    public bool isGenerating = false;

    private void OnEnable()
    {
        EditorApplication.update += UpdateEditor;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
    }

    public void Init(int subChunkSize, int chunkSize,Vector3 offset, List<Vector3> localPositions,Material baseMaterial)
    {
        if (!isGenerating)
        {
            this.subChunkSize = subChunkSize;
            this.chunkSize = chunkSize;
            this.offset = offset;
            this.localPositions = localPositions;
            currentJobCount = 0;
            finishedJobCount = 0;
            this.baseMaterial = baseMaterial;
            jobCount = densitiesList.Count;
            jobs.Clear();
            finishedJobs.Clear();

            for (int i = subChunks.Count; i < jobCount; i++)
            {
                CreateSubChunk(localPositions[i], i);
            }

            while (subChunks.Count > jobCount)
            {
                DestroyImmediate(subChunks[subChunks.Count - 1]);
                subChunks.RemoveAt(subChunks.Count - 1);
            }
        }
        
    }


    public void StartGeneration(float isoLevel)
    {
        if (!isGenerating)
        {
            for (int i = 0; i < densitiesList.Count; i++)
            {
                NativeList<Vector3> vertex = new NativeList<Vector3>(Allocator.Persistent);
                NativeList<Vector3> normals = new NativeList<Vector3>(Allocator.Persistent);
                NativeList<int> triangles = new NativeList<int>(Allocator.Persistent);

                MarchingCubesJob marchJob = new MarchingCubesJob();
                marchJob.densities = densitiesList[i];
                marchJob.vertices = vertex;
                marchJob.triangles = triangles;
                marchJob.normals = normals;
                marchJob.isoLevel = isoLevel;
                marchJob.size = subChunkSize + 2;

                jobs.Add(marchJob);
                JobHandle handle = marchJob.Schedule();
                jobHandles.Add(handle);

                vertexList.Add(vertex);
                normalsList.Add(normals);
                trianglesList.Add(triangles);
            }
        }
    }

    private void RegenerateSubChunk(NativeList<Vector3> vertices, NativeList<Vector3> normals, NativeList<int> triangles, Vector3 localPos,int subChunkIndex)
    {
        MeshFilter m = subChunks[subChunkIndex];
        if (vertexList[subChunkIndex].Length > 0)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.SetVertices(vertexList[subChunkIndex].ToArray());
            mesh.SetNormals(normalsList[subChunkIndex].ToArray());
            mesh.SetTriangles(trianglesList[subChunkIndex].ToArray(), 0);

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            m.sharedMesh = mesh;
            m.transform.localPosition = localPos;

            m.GetComponent<MeshRenderer>().material = baseMaterial;
            m.GetComponent<MeshCollider>().sharedMesh = m.sharedMesh;
        }
        else
        {
            m.sharedMesh.Clear();
            m.GetComponent<MeshCollider>().sharedMesh.Clear();
        }
        
        vertexList[subChunkIndex].Dispose();
        normalsList[subChunkIndex].Dispose();
        trianglesList[subChunkIndex].Dispose();
        densitiesList[subChunkIndex].Dispose();
    }


    public void CreateSubChunk(Vector3 localPos, int subChunkIndex)
    {
        GameObject go = new GameObject("SUB_CHUNK_" + subChunkIndex);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        MeshFilter m = go.AddComponent<MeshFilter>();
        m.sharedMesh = new Mesh();
        go.AddComponent<MeshRenderer>();
        subChunks.Add(m);
        go.AddComponent<MeshCollider>();
    }

    void TryToFinishJobs()
    {
        for (int i = 0; i < jobCount; i++)
        {
            if (jobHandles[i].IsCompleted)
            {
                if (!finishedJobs.Contains(i) ){
                    finishedJobs.Add(i);
                    currentJobCount++;
                }
            }
        }
    }

    private void UpdateEditor()
    {
        if(currentJobCount < jobCount)
        {
            isGenerating = true;
            TryToFinishJobs();
        }
        
        if(finishedJobs.Count > 0 && finishedJobCount < finishedJobs.Count)
        {
            isGenerating = true;
            int job = finishedJobs[finishedJobCount];
            jobHandles[job].Complete();
            RegenerateSubChunk(vertexList[job], normalsList[job], trianglesList[job], localPositions[job], job);
            finishedJobCount++;
        }

        if(finishedJobCount==jobCount && currentJobCount == jobCount)
        {
            isGenerating = false;
            trianglesList.Clear();
            vertexList.Clear();
            normalsList.Clear();
            jobHandles.Clear();
            densitiesList.Clear();
            jobs.Clear();
        }
    }


}
