using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    bool live;
    bool born;

    public MazeCell(bool isAlive){
        live = isAlive;
        born = isAlive;
    }

    public bool isAlive(){
        return live;
    }

    public bool isBorn(){
        return born;
    }

    public void Death(MazeCell[,] graph, int x, int y)
    {
        int neighborCount = NeighbourCount(graph,x,y);
        if(neighborCount > 4)
        {
            live = false;
        }
    }

    public void Birth(MazeCell[,] graph, int x, int y)
    {
        int neighborCount = NeighbourCount(graph,x,y);
        if(neighborCount == 3)
        {
            born = true;
            live = true;
        }
    }

    public int NeighbourCount(MazeCell[,] graph, int x, int y){
        int[,] neighbors = new int[8,2]{
            {x+1,y},
            {x+1,y+1},
            {x+1,y-1},
            {x-1,y},
            {x-1,y+1},
            {x-1,y-1},
            {x, y-1},
            {x, y+1}
        };

        int count = 0;

        for(int i=0;i<8;i++)
        {
            if(isValidNeighbor(graph,neighbors[i,0],neighbors[i,1])){
                if(graph[neighbors[i,0],neighbors[i,1]].isAlive())
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool isValidNeighbor(MazeCell[,] graph, int x, int y){
        return x < graph.GetLength(0) && x >= 0 && y < graph.GetLength(1) && y >= 0;
    }

}
