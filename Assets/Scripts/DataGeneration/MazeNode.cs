using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeNode : MonoBehaviour
{
    public Vector2 position;
    bool visited;

    public GameObject[] walls;

    public MazeNode(int x, int y){
        position = new Vector2(x,y);
    }

    public void OpenWall(int wall){
        if(wall >= 0 && wall <walls.Length){
            walls[wall].gameObject.SetActive(false);
        }
    }

    public bool isVisted(){
        return visited;
    }

    public void Visit(){
        visited = true;
    }

}
