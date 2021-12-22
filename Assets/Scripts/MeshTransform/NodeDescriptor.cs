using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeDescriptor 
{
    public Vector3 position;
    public Vector3[] vertices;
    public Vector2[] UV;
    public int[] triangles;

    public NodeDescriptor(Vector3 position){

        this.position = position;
        if(position.y > 0){
            GenerateVertexNeighbors();
            GenerateTrianglesNeighbours();

            UV = new Vector2[8]{
                new Vector2(0.25f,0f),
                new Vector2(0.75f,0.0f),
                new Vector2(0f,0.25f),
                new Vector2(0f,0.75f),
                new Vector2(0.25f,1f),
                new Vector2(0.75f,1f),
                new Vector2(1f,0.25f),
                new Vector2(1f,0.75f)
            };

        }else{
            GenerateVertex();
            GenerateTriangles();

            UV = new Vector2[4]{
                Vector2.up,
                Vector2.one,
                Vector2.zero,
                Vector2.right
            };
        }
    	
        
    }

    public void GenerateVertex()
    {
        vertices = new Vector3[4]{
                position,
                position + Vector3.forward,
                position + Vector3.right,
                position + Vector3.forward + Vector3.right,
            };
    }

    public void GenerateVertexNeighbors()
    {
        vertices = new Vector3[8]{
                position,
                position + Vector3.forward,
                position + Vector3.right,
                position + Vector3.forward + Vector3.right,
                position+ Vector3.down,
                position + Vector3.forward+Vector3.down,
                position + Vector3.right+Vector3.down,
                position + Vector3.forward + Vector3.right+Vector3.down,
            };
    }

    public void GenerateTriangles()
    {
        triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;
        triangles[3] = 3;
        triangles[4] = 2;
        triangles[5] = 0;
    }

    public void GenerateTrianglesNeighbours()
    {
        triangles = new int[30];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;
        triangles[3] = 3;
        triangles[4] = 2;
        triangles[5] = 0;

        triangles[6] = 0;
        triangles[7] = 2;
        triangles[8] = 6;
        triangles[9] = 6;
        triangles[10] = 4;
        triangles[11] = 0;

        triangles[12] = 7;
        triangles[13] = 3;
        triangles[14] = 1;
        triangles[15] = 1;
        triangles[16] = 5;
        triangles[17] = 7;

        triangles[18] = 2;
        triangles[19] = 3;
        triangles[20] = 7;
        triangles[21] = 7;
        triangles[22] = 6;
        triangles[23] = 2;

        triangles[24] = 5;
        triangles[25] = 1;
        triangles[26] = 0;
        triangles[27] = 0;
        triangles[28] = 4;
        triangles[29] = 5;

    }


    public Vector3[] GetVerts(){
        return vertices;
    }



    public int[] GetTriangles(int offset){
        int[] temp = triangles;

        for(int i=0;i<temp.Length;i++)
        {
            temp[i] += offset*vertices.Length;
        }

        return temp;
    }

}
