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
![Maze basic structure](https://github.com/filipgolaboski/DungeonProceduralGeneration/blob/81fdb3474d73f96be0e3ad86f0492690b99e3aa5/ReadmeImages/MazeBasic.png)

### Cellular Maze Generation - Conway's game of life
Another approach to maze generation is by using Conway's game of life to simulate maze-like generation.
Every cell in the matrix is a cell which is alive or dead. In each lifecycle of the cells we choose if
each individual cell should be alive or dead. Choosing which cell is alive or dead is based on a set of predefined rules.
If the cell has exactly 3 neighbors it is alive and it will stay alive until there are no more than 5 neighbors. Once there
are more than 5 neighbors the cell dies. This is the rulestring B3/S12345 and besides that there's a rulestring B3/S1234
in which the cell dies if it has more than 4 neighbors.

```c#
    public void Death(MazeCell[,] graph, int x, int y)
    {
        int neighborCount = NeighbourCount(graph,x,y);
        if(neighborCount > 5)
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
```

The resulting structure looks like this.


![Maze cellular structure][maze_cellular]

### TODO List
* Improve maze generating algorithms
  * Check different rulesets for the cellular algorithm
  * Test different approaches in generating structures using different search algorithms.
* Implement an algorithm that will generate rooms and paths towards them and/or elevation changes.
* Implement curved paths

## Mesh Generation
Because of performance reasons and the look of the resulting structure we can't instantiate primitives
or other objects to generate the final look. Instead we'll have to generate a mesh.
### Basic Mesh
A basic mesh can be generated somewhat like in the cellular maze structure. It is created with 
the BasicMeshGenerator class.It works by creating a NodeDescriptor for every cell in the matrix. 
That NodeDescriptor holds the vertices for every corner of a cube and we offset them based on 
which cell it is on the grid by the X and Z positions and offset them by Y based on their life state.
When we add the individual vertices to the mesh we check in a dictionary and see if there are duplicates.
If there are we index them in the triangles array.

```c#
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
```

With this approach we find ourselves with a single mesh generated which is easier for the GPU to handle.
There are two issues which we face with this type of approach both which are solvable.

First it's the texturing of the resulting model. I tried implementing some way of generating the UV's for
the resulting model but soon realised that it will be almost imposible or extremely hard to generate UV's 
for complex shapes. That's why i decided it would be far easier to use a Triplanar shader that doesn't 
check for UV coordinates but instead projects the texture depending on which side it faces in world coordinates.

The second problem is much harder and that is handling complex shapes.

## Marching Cubes
To generate mode complex shapes on the screen we'll use a more sophisticated algorithm called Marching Cubes.
Marching cubes much like it's namesake loops a 3 dimensional matrix and generates vertices based on density data.
Each voxel of the structure is defined by 8 corners all of which have what is called and ISO level.
Inside this imaginary cube we can have up to 256 possible combinations between all the corners of the cube which define a triangle.
There is already a predefined list of all combinations. To get the desired combination we first need to calculate the index of the first edge.
We do this by bit masking the corners of the voxels which have an ISO lower than the surface ISO. All of those give use a cube index which we multiply
by 15. Then we iterate until we generate all the triangles in the voxel.

Cube index calculation
```c#
    public int CalculateCubeIndex(float[] voxelDensity, float isoLevel)
    {
        int cubeIndex = 0;
        if(voxelDensity[0] < isoLevel) cubeIndex |= 1;
        if(voxelDensity[1] < isoLevel) cubeIndex |= 2;
        if(voxelDensity[2] < isoLevel) cubeIndex |= 4;
        if(voxelDensity[3] < isoLevel) cubeIndex |= 8;
        if(voxelDensity[4] < isoLevel) cubeIndex |= 16;
        if(voxelDensity[5] < isoLevel) cubeIndex |= 32;
        if(voxelDensity[6] < isoLevel) cubeIndex |= 64;
        if(voxelDensity[7] < isoLevel) cubeIndex |= 128;
        return cubeIndex;
    }
```

Polygon generation.
```c#
        for(int i=0;MarchingCubesLookupTables.TriangleTable[rowIndex+i] != -1 && i<15; i+=3){
            Vector3 vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i]];
            Vector3 vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
            Vector3 vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];
            Vector3 normal = Vector3.Normalize(Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1));
            if(!vertex1.Equals(vertex2) && 
                !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3)){
                    voxelData.vertexData.Add(vertex1);
                    voxelData.normalData.Add(normal);
                    voxelData.triangleData.Add(voxelData.vertexData.Count-1);
                    voxelData.vertexData.Add(vertex2);
                    voxelData.normalData.Add(normal);
                    voxelData.triangleData.Add(voxelData.vertexData.Count-1);
                    voxelData.vertexData.Add(vertex3);
                    voxelData.normalData.Add(normal);
                    voxelData.triangleData.Add(voxelData.vertexData.Count-1);
            }
        }

```

We add all of these polygons to the Unity Mesh class to get the generated terrain as shown in this image.

![Randomly generated terrain][rng_terrain]

If we create a basic density function in our cellular generation algorithm we get the following mesh.

![Cellular generated terrain][cell_terrain]

## TODO
* Generate different densities that implement randomness or noise for the different maze and dungeon algorithms.
* Add editor sliders for different densities and noise strenght
* Improve performance by using unity's BURST compiler and ECS
* Try to improve the performance of the generated mesh by using the basic mesh generator's indexing capabilities(idea!!)

[rng_terrain]:https://github.com/filipgolaboski/DungeonProceduralGeneration/blob/81fdb3474d73f96be0e3ad86f0492690b99e3aa5/ReadmeImages/RandomMarchingCubes.png
[cell_terrain]:https://github.com/filipgolaboski/DungeonProceduralGeneration/blob/81fdb3474d73f96be0e3ad86f0492690b99e3aa5/ReadmeImages/CellMeshMarchingCubes.png
[maze_cellular]:https://github.com/filipgolaboski/DungeonProceduralGeneration/blob/81fdb3474d73f96be0e3ad86f0492690b99e3aa5/ReadmeImages/CellMeshGen.png
