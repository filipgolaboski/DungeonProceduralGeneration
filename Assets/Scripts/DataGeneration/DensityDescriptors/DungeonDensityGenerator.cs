using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunayVoronoi;
using UnityEditor;
using System;

[CreateAssetMenu(menuName = "TEST")]
public class DungeonDensityGenerator : BaseDensityDescriptor
{
    float[,,] densityMap;
    public LineDensityDescriptor line;
    public BaseDensityDescriptor[] room;
    public bool generate;
    Room[] rooms;
    List<EdgeWeighted> mst;
    public Vector2Int generationRange = new Vector2Int(64, 64);
    public Vector2 centerDistancing = new Vector2(0.01f, 0.01f);
    public float minRoomSize = 3;
    public float maxRoomSize = 15;
    public int roomsNumber = 20;
    public Vector3Int offsetRoad = new Vector3Int(10, 32, 10);
    public Vector3Int offsetRoom = new Vector3Int(10, 38, 10);

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= EditorUpdate;
    }

    public void EditorUpdate()
    {
        if (generate)
        {
            GenerateDungeon();
            generate = false;
        }
    }

    void GenerateDungeon()
    {
        densityMap = new float[size.x, size.y, size.z];
        
        DungeonGenerator dg = new DungeonGenerator(new Vector2Int(size.x,size.z),generationRange, centerDistancing, minRoomSize, maxRoomSize);
        rooms = dg.GenerateRooms(roomsNumber);
        mst = dg.GenerateCorridors(rooms);

        for(int i=0;i<rooms.Length;i++)
        {
            float currentDiff = float.MaxValue;
            for(int j=0;j<room.Length;j++)
            {
                if(Mathf.Abs(room[j].size.x - rooms[i].radius) < currentDiff)
                {
                    rooms[i].baseDensityDescriptor = room[j];
                    currentDiff = room[j].size.x - rooms[i].radius;
                }

                if (Mathf.Abs(room[j].size.z - rooms[i].radius) < currentDiff)
                {
                    rooms[i].baseDensityDescriptor = room[j];
                    currentDiff = room[j].size.z - rooms[i].radius;
                }
            }
        }

        List<Vector2> supraTriangleVertices = new List<Vector2>()
        {
            Vector2.zero,
            new Vector2(size.x,0),
            new Vector2(size.x,size.y),
            new Vector2(size.y,0)
        };

        List<EdgeWeighted> tempEdge = new List<EdgeWeighted>();
        for(int i=0;i<mst.Count;i++)
        {
            if (!supraTriangleVertices.Contains(mst[i].startVertex) && !supraTriangleVertices.Contains(mst[i].endVertex))
            {
                tempEdge.Add(mst[i]);
            }
        }

        mst = tempEdge;

    }




    public override float GetDensity(Vector3Int point)
    {
        Vector3Int point1z = new Vector3Int((int)mst[0].startVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[0].startVertex.y + offsetRoad.z);
        Vector3Int point2z = new Vector3Int((int)mst[0].endVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[0].endVertex.y + offsetRoad.z);
        Vector3Int point3z = new Vector3Int((int)mst[1].startVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[1].startVertex.y + offsetRoad.z);
        Vector3Int point4z = new Vector3Int((int)mst[1].endVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[1].endVertex.y + offsetRoad.z);
        float val = Mathf.Min(line.GetDensity(point, point1z, point2z),
            line.GetDensity(point, point3z, point4z)
            );
        for (int i = 2; i < mst.Count; i++)
        {
            Vector3Int point1Next = new Vector3Int((int)mst[i].startVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[i].startVertex.y + offsetRoad.z);
            Vector3Int point2Next = new Vector3Int((int)mst[i].endVertex.x + offsetRoad.x, offsetRoad.y, (int)mst[i].endVertex.y + offsetRoad.z);
            val = Mathf.Min(val, line.GetDensity(point, point1Next, point2Next));
        }


        for (int i = 0; i < rooms.Length; i++)
        {
            val = Mathf.Min(val, rooms[i].baseDensityDescriptor.GetDensity(point, new Vector3Int((int)rooms[i].position.x + offsetRoom.x, offsetRoom.y, (int)rooms[i].position.y + offsetRoom.z)));
        }

        return val;
    }

}
