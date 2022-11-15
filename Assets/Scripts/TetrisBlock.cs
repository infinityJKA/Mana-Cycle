using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TetrisBlock : MonoBehaviour
{

    public Vector3 rotationPoint;
    private float previousTime;
    [SerializeField] private float fallTime = 0.8f;

    public static int height = 20;
    public static int width = 10;
    private static Transform[,] grid = new Transform[width, height];


    void Update(){
        if(Input.GetKeyDown(KeyCode.UpArrow)){
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0,0,1), 90);
            if(!ValidMove()){
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0,0,1), -90);
            }
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)){
            transform.position += new Vector3(-1,0,0);
            if(!ValidMove()){
                transform.position -= new Vector3(-1,0,0);
            }
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)){
            transform.position += new Vector3(1,0,0);
            if(!ValidMove()){
                transform.position -= new Vector3(1,0,0);
            }
        }

        if(Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime/10 : fallTime)){
            transform.position += new Vector3(0,-1,0);
            if(!ValidMove()){
                transform.position -= new Vector3(0,-1,0);
                AddToGrid();
                this.enabled = false;
                CheckForLines();
                FindObjectOfType<Spawn>().NewTetromino();
            }
            previousTime = Time.time;
        }
    }

    void AddToGrid(){
        foreach (Transform children in transform){
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            grid[roundedX, roundedY] = children;
        }

    }

    bool ValidMove(){  // Checks if position of pieces are in the grid size
        foreach (Transform children in transform){
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if(roundedX < 0 || roundedX >= width || roundedX >= width || roundedY < 0 || roundedY >= height){
                return false;
            }
            if(grid[roundedX,roundedY] != null){
                return false;
            }
        }
        return true;
    }

    void CheckForLines(){
        for(int row = height-1; row >= 0; row--){
            if(HasLine(row)){
                DeleteLine(row);
                RowDown(row);
            }
        }
    }

    bool HasLine(int row){
        for(int col = 0; col < width; col++){
            if(grid[col,row] == null){
                return false;
            }
        }
        return true;
    }

    void DeleteLine(int row){
        for(int col = 0; col < width; col++){
            Destroy(grid[col,row].gameObject);
            grid[col,row] = null;
        }
    }

    void RowDown(int row){
        for(int y = row; y < height; y++){
            for (int col = 0; col < width; col++){
                if(grid[col,y] != null){
                    grid[col,y-1] = grid [col,y];
                    grid[col,y] = null;
                    grid[col,y-1].transform.position -= new Vector3(0,1,0);
                }
            }
        }
    }

}
