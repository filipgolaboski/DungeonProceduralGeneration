using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MazeGenerator : MonoBehaviour
{

    public Vector2Int size;
    public GameObject TestRepresentation;
    MazeNode[,]graph;
    public float randomChance = 0.4f;
    public bool generate;

    // Update is called once per frame
    void Update()
    {
        if(generate){
            generate = false;
            PopulateGenerator();
            GenerateMazeData(0,0);
            //GenerateMaze();
        }
    }

    public void PopulateGenerator(){
        graph = new MazeNode[size.x,size.y];

        for(int i=0; i<size.x;i++){
            for(int j=0;j<size.y; j++){
                GameObject node = Instantiate(TestRepresentation,transform);
                node.transform.localPosition = new Vector3(i,0,j);
                graph[i,j] = node.GetComponent<MazeNode>();
                graph[i,j].position = new Vector2(i,j);
            }
        }
    }

    public void GenerateMazeData(int x, int y)
    {
        List<MazeNode> nodes = GetNeighbors(x,y);
        MazeNode node = graph[x,y];
        node.Visit();
        for(int i=0;i<nodes.Count;i++){
            if(nodes[i]){
                if(!nodes[i].isVisted() && Random.Range(0f,1f) > randomChance){
                    node.OpenWall(i);
                    if(i%2!=0){
                        nodes[i].OpenWall(i-1);
                    }else{
                        nodes[i].OpenWall(i+1);
                    }
                GenerateMazeData((int)nodes[i].position.x, (int)nodes[i].position.y);
                }
            }
            
        }

    }



    List<MazeNode> GetNeighbors(int x, int y){
        int[,] possibleNeighbors = new int[4,2]{
            {x,y+1},
            {x,y-1},
            {x+1,y},
            {x-1,y}
        };
        List<MazeNode> ValidNeighbors = new List<MazeNode>();
        for(int i=0; i<4;i++)
        {
            if(isValidNeighbor(possibleNeighbors[i,0], possibleNeighbors[i,1])){
                ValidNeighbors.Add(graph[possibleNeighbors[i,0],possibleNeighbors[i,1]]);
            }else{
                ValidNeighbors.Add(null);
            }
        }

        return ValidNeighbors;
    }

    bool isValidNeighbor(int x, int y){
        return x < size.x && x >= 0 && y < size.y && y >= 0;
    }
}
