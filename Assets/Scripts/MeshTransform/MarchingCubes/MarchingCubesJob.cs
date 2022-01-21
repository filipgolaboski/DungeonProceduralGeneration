using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

public struct Position
{
    public int x, y, z;
    public Position(int x,int y,int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
[BurstCompile]
public struct MarchingCubesJob : IJob
{
    //INPUTS
    public float isoLevel;
    public int size;
    public NativeArray<float> densities;

    //OUTPUTS
    public NativeList<Vector3> vertices;
    public NativeList<Vector3> normals;
    public NativeList<int> triangles;

    public int CalculateCubeIndex(NativeArray<float> voxelDensity)
    {
        int cubeIndex = 0;
        if (voxelDensity[0] < isoLevel) cubeIndex |= 1;
        if (voxelDensity[1] < isoLevel) cubeIndex |= 2;
        if (voxelDensity[2] < isoLevel) cubeIndex |= 4;
        if (voxelDensity[3] < isoLevel) cubeIndex |= 8;
        if (voxelDensity[4] < isoLevel) cubeIndex |= 16;
        if (voxelDensity[5] < isoLevel) cubeIndex |= 32;
        if (voxelDensity[6] < isoLevel) cubeIndex |= 64;
        if (voxelDensity[7] < isoLevel) cubeIndex |= 128;
        return cubeIndex;
    }

    public float GetDensity(Vector3Int position)
    {
        return densities[position.x * size * size + position.y * size + position.z];
    }

    public NativeArray<float> GetCubeDensities(Vector3Int position, NativeArray<float> voxelDensity)
    {
        for (int i = 0; i < voxelDensity.Length; i++)
        {
            Vector3Int corner = position + MarchingCubesLookupTables.CubeCorners[i];
            voxelDensity[i] = GetDensity(corner);
        }
        return voxelDensity;
    }

    public NativeArray<int> GetEdgeIndices(NativeArray<int> edgeIndex, int cubeIndex)
    {
        for(int i = 0; i < 16; i++)
        {
            edgeIndex[i] = MarchingCubesLookupTables.triangulation[cubeIndex][i];
        }
        return edgeIndex;
    }

    public NativeArray<Vector3Int> GetVoxelCorners(NativeArray<Vector3Int> corners, Vector3Int position)
    {
        for (int i = 0; i < 8; i++)
        {
           corners[i] = position + MarchingCubesLookupTables.CubeCorners[i];
        }

        return corners;
    }

    public Vector3 CreateVertex(Vector3Int p1, Vector3Int p2)
    {
        float d1 = GetDensity(p1);
        float d2 = GetDensity(p2);
        float t = (isoLevel - d1) / (d2 - d1);

        return p1 + t * ((Vector3)p2 - p1);
    }

    public Vector3 CalculateNormal(Vector3Int p1)
    {

        float dx = GetDensity(p1 + Vector3Int.right) - GetDensity(p1 + Vector3Int.left);
        float dy = GetDensity(p1 + Vector3Int.up) - GetDensity(p1 + Vector3Int.down);
        float dz = GetDensity(p1 + Vector3Int.forward) - GetDensity(p1 + Vector3Int.back);
        return new Vector3(dx, dy, dz);
    }

    public Vector3 CreateNormal(Vector3Int p1, Vector3Int p2)
    {
        Vector3 n1 = CalculateNormal(p1);
        Vector3 n2 = CalculateNormal(p2);
        float d1 = GetDensity(p1);
        float d2 = GetDensity(p2);
        float t = (isoLevel - d1) / (d2 - d1);
        return n1 + t * (n2 - n1);
    }

    public void Execute()
    {
        NativeArray<float> voxelDensities = new NativeArray<float>(8, Allocator.Temp);
        NativeArray<int> edgeIndex = new NativeArray<int>(16, Allocator.Temp);
        NativeArray<Vector3Int> corners = new NativeArray<Vector3Int>(8, Allocator.Temp);

        for (int x = 1; x < size-2; x++)
        {
            for(int y = 1; y < size - 2; y++)
            {
                for(int z=1;z < size - 2; z++)
                {

                    
                    Vector3Int p = new Vector3Int(x, y, z);
                    voxelDensities = GetCubeDensities(p,voxelDensities);
                    int cubeIndex = CalculateCubeIndex(voxelDensities);
                    if (cubeIndex == 0 || cubeIndex == 255)
                    {
                        continue;
                    }

                    edgeIndex = GetEdgeIndices(edgeIndex, cubeIndex);
                    corners = GetVoxelCorners(corners,p);

                    for(int i=0; edgeIndex[i] != -1; i+=3)
                    {
                        int edgeIndexA = edgeIndex[i];
                        int a0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexA];
                        int a1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexA];

                        int edgeIndexB = edgeIndex[i + 1];
                        int b0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexB];
                        int b1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexB];

                        int edgeIndexC = edgeIndex[i + 2];
                        int c0 = MarchingCubesLookupTables.EdgeIndexTableA[edgeIndexC];
                        int c1 = MarchingCubesLookupTables.EdgeIndexTableB[edgeIndexC];

                        Vector3 v1 = CreateVertex(corners[a0], corners[a1]);
                        Vector3 v2 = CreateVertex(corners[b0], corners[b1]);
                        Vector3 v3 = CreateVertex(corners[c0], corners[c1]);
                        Vector3 n1 = CreateNormal(corners[a0], corners[a1]);
                        Vector3 n2 = CreateNormal(corners[b0], corners[b1]);
                        Vector3 n3 = CreateNormal(corners[c0], corners[c1]);
                        if (!v1.Equals(v2) && !v2.Equals(v3) && !v3.Equals(v1))
                        {
                            vertices.Add(v1);
                            normals.Add(n1);
                            triangles.Add(vertices.Length - 1);
                            vertices.Add(v2);
                            normals.Add(n2);
                            triangles.Add(vertices.Length - 1);
                            vertices.Add(v3);
                            normals.Add(n3);
                            triangles.Add(vertices.Length - 1);
                        }
                        
                    }

                }
            }
        }

        voxelDensities.Dispose();
        edgeIndex.Dispose();
        corners.Dispose();

    }
}
