using DelaunayVoronoi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EdgeWeighted : IComparable
{
    public Vector2 startVertex;
    public Vector2 endVertex;
    public float weight;

    public EdgeWeighted(Point Point1, Point Point2)
    {
        startVertex = new Vector2((float)Point1.X, (float)Point1.Y);
        endVertex = new Vector2((float)Point2.X, (float)Point2.Y);
        weight = Vector2.Distance(startVertex, endVertex);
    }

    public int CompareTo(object obj)
    {
        if (obj == null)
        {
            return 1;
        }

        EdgeWeighted e = obj as EdgeWeighted;
        if (e != null)
        {
            return (int)Mathf.Clamp(weight - e.weight, -1, 1);
        }

        return 0;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;
        var edge = obj as EdgeWeighted;

        var samePoints = startVertex == edge.startVertex && endVertex == edge.endVertex;
        var samePointsReversed = startVertex == edge.endVertex && endVertex == edge.startVertex;
        return samePoints || samePointsReversed;
    }

    public override int GetHashCode()
    {
        int hCode = (int)startVertex.x ^ (int)startVertex.y ^ (int)endVertex.x ^ (int)endVertex.y;
        return hCode.GetHashCode();
    }
}

public class Room
{
    public Vector2 position;
    public float radius;
    public BaseDensityDescriptor baseDensityDescriptor;

    public bool Colliding(Room r)
    {
        float dist = Vector2.Distance(r.position, position);
        return dist < radius + r.radius;
    }
}

public class DungeonGenerator
{
    public Vector2Int dungeonSize;
    public Vector2Int generationDistance;
    public Vector2 randomRoomMovement;
    public float maxRoomRadius;
    public float minRoomRadius;

    public DungeonGenerator(Vector2Int dungeonSize, Vector2Int generationDistance, Vector2 randomMovement, float minRoomRadius ,float maxRoomRadius)
    {
        this.dungeonSize = dungeonSize;
        this.generationDistance = generationDistance;
        this.randomRoomMovement = randomMovement;
        this.maxRoomRadius = maxRoomRadius;
        this.minRoomRadius = minRoomRadius;
    }

    public List<EdgeWeighted>  GenerateCorridors(Room[] rooms)
    {
        DelaunayTriangulator dt = new DelaunayTriangulator();
        dt.Init(dungeonSize.x, dungeonSize.y);
        List<Point> points = GenerateRoomPoints(rooms);
        var triangles = dt.BowyerWatson(points);
        List<EdgeWeighted> edges = new List<EdgeWeighted>();
        foreach (var triangle in triangles)
        {
            EdgeWeighted e1 = new EdgeWeighted(triangle.Vertices[0], triangle.Vertices[1]);
            EdgeWeighted e2 = new EdgeWeighted(triangle.Vertices[1], triangle.Vertices[2]);
            EdgeWeighted e3 = new EdgeWeighted(triangle.Vertices[2], triangle.Vertices[0]);
            if (!edges.Contains(e1))
                edges.Add(e1);
            if (!edges.Contains(e2))
                edges.Add(e2);
            if (!edges.Contains(e3))
                edges.Add(e3);
        }

        edges.Sort();
        Dictionary<Vector2, Vector2> parents = new Dictionary<Vector2, Vector2>();
        foreach (var p in edges)
        {
            parents[p.startVertex] = p.startVertex;
            parents[p.endVertex] = p.endVertex;
        }


        List<EdgeWeighted> mst = new List<EdgeWeighted>();
        foreach (EdgeWeighted e in edges)
        {
            Vector2 s1 = FindVertexSet(e.startVertex, parents);
            Vector2 s2 = FindVertexSet(e.endVertex, parents);

            if (s1 != s2)
            {
                mst.Add(e);
                parents[s1] = s2;
            }
        }
        return mst;
    }

    List<Point> GenerateRoomPoints(Room[] rooms)
    {
        List<Point> points = new List<Point>();

        for (int i = 0; i < rooms.Length; i++)
        {
            points.Add(new Point(rooms[i].position.x, rooms[i].position.y));
        }
        return points;
    }

    public Room[] GenerateRooms(int roomNumbers)
    {
        Vector2 center = new Vector2(dungeonSize.x/2, dungeonSize.y/2);
        Room[] rooms = new Room[roomNumbers];
        for (int i = 0; i < roomNumbers; i++)
        {
            rooms[i] = new Room();
            rooms[i].position = RandomVector(center-generationDistance, center+generationDistance);
            rooms[i].radius = Random.Range(minRoomRadius,maxRoomRadius);
        }
        int itterations = 5;

        for (int i = 0; i < rooms.Length; i++)
        {
            itterations = 10000;
            while (rooms[i].position.x > dungeonSize.x / 8 && rooms[i].position.x < dungeonSize.x - dungeonSize.x / 8
                && rooms[i].position.y > dungeonSize.y / 8 && rooms[i].position.y < dungeonSize.y - dungeonSize.y / 8
                && itterations > 0)
            {

                rooms[i].position += (rooms[i].position - center).normalized + randomRoomMovement;
                itterations--;
            }

        }
        

        return rooms;
    }

    Vector2 RandomVector(Vector2 from, Vector2 to)
    {
        float x = UnityEngine.Random.Range(from.x, to.x);
        float y = UnityEngine.Random.Range(from.y, to.y);
        return new Vector2(x, y);
    }

    public Vector2 FindVertexSet(Vector2 vertex, Dictionary<Vector2, Vector2> parents)
    {
        if (parents[vertex] != vertex)
        {
            return FindVertexSet(parents[vertex], parents);
        }
        return parents[vertex];
    }
}
