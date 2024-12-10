using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public int GridRows;
    public int GridCols;

    public int Mines;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public UIManager UIManager;


    void Start() {
        UIManager.InitializeGridUI(GridRows, GridCols);
        InitializeGrid(GridRows, GridCols);
    }

    private void InitializeGrid(int GridRows, int GridCols) {
        GridGenerator = new GridGenerator(Mines);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        Grid.GenerateMines();
    }

    public void OnCellClicked(int row, int col) {

        Cell cell = Grid.Fields[row, col];
        if(UIManager == null) {
            print("UIMANAGER NULL");
        }
        if (cell.IsMine) {
            print($"Clicked on mine at {row}, {col}");
            UIManager.RevealMineCell(row, col);
        }
        else {
            print($"Clicked empty cell at {row}, {col}");
            UIManager.RevealEmptyCell(row, col);
        }
    }
}
