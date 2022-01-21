using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]
public class MazeGeneratorCellular : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(50,50);
    public MazeCell[,] mazeGraph;
    
    public bool startGenerating;
    [SerializeField]
    private bool isGenerating = false;
    public int numberOfGenerations;
    public int generationMultiplier = 32;
    int currentGeneration;


    private void OnEnable() {
        EditorApplication.update += EditorUpdate; 
    }

    private void OnDestroy() {
        EditorApplication.update -= EditorUpdate; 
    }


    public void Generate(){
        SeedGraph();
        currentGeneration = numberOfGenerations;
        while(currentGeneration > 0){
            LifeCycle();
            currentGeneration--;
            Debug.Log(currentGeneration);
        }
        Debug.Log("FINISH");
    }

    void EditorUpdate()
    {
        if(!isGenerating){
            isGenerating = startGenerating;
            startGenerating = false;
            if(isGenerating){
                SeedGraph();
                currentGeneration = numberOfGenerations;
            }
        }

        if(isGenerating){
            if(currentGeneration > 0)
            {
                LifeCycle();
                currentGeneration--;
                Debug.Log(currentGeneration);
            }else{
                isGenerating = false;
                //Instantiate();
            }
        }
    }

    
    public void LifeCycle()
    {
        for(int counter = 0; counter < generationMultiplier; counter++){
            for(int i=0;i<size.x;i++)
            {
                for(int j=0;j<size.y;j++)
                {
                    if(!mazeGraph[i,j].isBorn())
                    {
                        mazeGraph[i,j].Birth(mazeGraph,i,j);
                    }else{
                        mazeGraph[i,j].Death(mazeGraph,i,j);
                    }
                    
                    
                }
            }
        }
        
    }

    public void SeedGraph()
    {
        mazeGraph = new MazeCell[size.x,size.y];
        Vector2Int halfSize = size/2;
        for(int i=halfSize.x-halfSize.x/2;i<halfSize.x+halfSize.x/2;i++)
        {
            for(int j=halfSize.y-halfSize.y/2;j<halfSize.y+halfSize.y/2;j++)
            {
                mazeGraph[i,j] = new MazeCell(Random.Range(0f,1f) > 0.5f);
            }
        }

        for(int i=0;i<size.x;i++)
        {
            for(int j=0;j<size.y;j++)
            {
                if(mazeGraph[i,j] == null){
                    mazeGraph[i,j] = new MazeCell(false);
                }
            }
        }
    }


}


