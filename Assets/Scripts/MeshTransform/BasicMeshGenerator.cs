using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[ExecuteInEditMode]
public class BasicMeshGenerator : MonoBehaviour
{
    MeshFilter meshFilter;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable() {
        if(!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    }

    public void GenerateMesh(MazeCell[,] graph, Vector2Int size){
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        Dictionary<Vector3,int> vertIndexer = new Dictionary<Vector3, int>();

        NodeDescriptor[,] nodeGraph = new NodeDescriptor[size.x,size.y];


        for(int i=0;i<size.x;i++)
        {
            for(int j=0;j<size.x;j++)
            {
                Vector3 pos = new Vector3(i,(graph[i,j].isAlive()?0:1),j);
                NodeDescriptor n = new NodeDescriptor(pos);
                nodeGraph[i,j] = n;
            }
        }

        for(int i=0;i<size.x;i++)
        {
            for(int j=0;j<size.x;j++)
            {
                NodeDescriptor n = nodeGraph[i,j];
                for(int k=0;k<n.vertices.Length;k++){
                    if(!vertIndexer.ContainsKey(n.vertices[k])){
                        verts.Add(n.vertices[k]);
                        vertIndexer.Add(n.vertices[k], verts.Count-1);
                    }
                }

                for(int k=0;k<n.triangles.Length;k++)
                {
                    int value = 0;
                    vertIndexer.TryGetValue(n.vertices[n.triangles[k]],out value);
                    triangles.Add(value);
                }
            }
        }
        /* List<Vector2> uvs = new List<Vector2>();
        int vertscount = verts.Count;
        for(int i=0;i<vertscount;i++){
                uvs.Add(new Vector2((verts[i].x+verts[i].y)/10, (verts[i].z + verts[i].y)/10));
        } */


        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.SetVertices(verts.ToArray());
        meshFilter.sharedMesh.SetTriangles(triangles.ToArray(),0);
        //meshFilter.sharedMesh.SetUVs(0,uvs);
        Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateTangents();
        meshFilter.sharedMesh.RecalculateBounds();

    }


    // Update is called once per frame
    void Update()
    {
        /* if(reset)
        {
            reset = false;

            Debug.Log(meshFilter.sharedMesh.vertices);

            Vector3[] newVert = new Vector3[(size.x+1)*(size.y+1)];
            int[] indices = new int[(size.x+1)*(size.y+1)*6];
        	Vector2[] UV = new Vector2[newVert.Length];

            for(int i=0;i<size.x+1;i++)
            {
                for(int j=0;j<size.y+1;j++)
                {
                    newVert[(i*(size.x+1))+j] = new Vector3(i,0,j);
                    UV[(i*(size.x+1))+j] = new Vector2(i/size.x, j/size.y);
                }
            }
            
            for(int ti = 0, vi = 0; vi<size.x*size.y; vi++){
                for(int xi = 0;xi<size.x;ti+=6,vi++, xi++)
                {
                    indices[ti] = vi;
                    indices[ti+3] = indices[ti+1] = vi+1;
                    indices[ti + 5] = indices[ti+2] = vi+size.x+1;
                    indices[ti+4] = vi+size.x+2;
                }
            }
            
            List<Vector3> verts = new List<Vector3>();
            List<int> triangles = new List<int>();
            Dictionary<Vector3,int> vertIndexer = new Dictionary<Vector3, int>();
            for(int i=0;i<size.x;i++)
            {
                for(int j=0;j<size.y;j++)
                {
                    int height = (Random.Range(0f,1f)>0.5f ? 1:0);
                    NodeDescriptor n = new NodeDescriptor(new Vector3(i,height,j));

                    for(int k=0;k<n.vertices.Length;k++){
                        if(!vertIndexer.ContainsKey(n.vertices[k])){
                            verts.Add(n.vertices[k]);
                            vertIndexer.Add(n.vertices[k], verts.Count-1);
                        }
                    }

                    for(int k=0;k<n.triangles.Length;k++)
                    {
                        int value = 0;
                        vertIndexer.TryGetValue(n.vertices[n.triangles[k]],out value);
                        triangles.Add(value);
                    }
                }
            }


            Vector3 test1= Vector3.one;
            Vector3 test2 = new Vector3(1,1,1);
            Debug.Log(test1.GetHashCode() + "  " +test2.GetHashCode());

            meshFilter.sharedMesh.Clear();
            meshFilter.sharedMesh.SetVertices(verts.ToArray());
            meshFilter.sharedMesh.SetTriangles(triangles.ToArray(),0);
            meshFilter.sharedMesh.RecalculateNormals();
            meshFilter.sharedMesh.RecalculateTangents();
            meshFilter.sharedMesh.RecalculateBounds();
            

        } */
    }
}
