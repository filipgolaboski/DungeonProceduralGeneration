using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class DungeonCreator : MonoBehaviour
{
    public MazeGeneratorCellular mazeGeneratorCellular;
    public MarchingCubes marchingCubes;

    public Vector2Int size;

    public bool generate;


    // Update is called once per frame
    void Update()
    {
            if(generate){
                
                CreateDungeon();
                generate = false;
            }
    }

    public void CreateDungeon()
    {
        mazeGeneratorCellular.size = size;
        mazeGeneratorCellular.Generate();
        marchingCubes.InitMeshGeneration(new Vector3Int(size.x*4,16,size.x*4),mazeGeneratorCellular.GenerateDensities());
        marchingCubes.GenerateMesh();
    }

}
