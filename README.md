# DungeonProceduralGeneration
3D Procedural Generation of Dungeons and Mazes

# Description
Procedurally generate dungeons or maze like structures in the Unity game engine. This tool will allow you to
generate different kinds of procedurally generated structures that you can later on edit in the editor or any 3D modeling app.

## Basic Maze Generation
To start procedurally generating structures we'll have to start with the basics, Maze Generation.
Mazes are a can be generated in a couple of ways.
### Tree structure and DepthFirstSearch
The most basic approach is to start with a matrix of nodes. We can start at one corner or randomly choose a point in the matrix.
Then we proceed with a random depth first search where we first mark the node as visited and then choose randomly for one of it's 4 neighbors and
open a wall towards it and recursively enter it. If the random check fails we return.
```c#
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
```
If we implement a basic maze generation with some basic wall-like structures we get this.
![Maze basic structure](https://github.com/filipgolaboski/DungeonProceduralGeneration/blob/feature/readme/ReadmeImages/MazeBasic.png?raw=true)
