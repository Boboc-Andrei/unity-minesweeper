using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public int GridRows;
    public int GridCols;

    public int Mines;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public UIManager UIManager;

    public bool GameStarted = false;

    void Start() {
        UIManager.InitializeGridUI(GridRows, GridCols);

        InitializeGrid(GridRows, GridCols);
    }

    private void InitializeGrid(int GridRows, int GridCols) {
        GridGenerator = new GridGenerator(Mines);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        Grid.InitializeFields();
    }

    public void OnCellClicked(int row, int col) {

        Cell cell = Grid.Fields[row, col];

        if(!GameStarted) {
            GameStarted = true;
            Grid.PlaceMines(guaranteedFree: cell);
        }

        if (UIManager == null) {
            Debug.LogError("UIManager is NULL");
        }
        if (cell == null) {
            Debug.LogError("Cell is NULL");
        }

        RevealCell(cell);
    }

    public void RevealCell(Cell cell) {
        if (cell.IsRevealed || cell.IsFlagged) return;

        int row = cell.Row;
        int col = cell.Col;


        if (cell.IsMine) {
            print($"Clicked on mine at {row}, {col}");
            UIManager.RevealMineCell(row, col);
            GameOver();
        }
        else {
            int neighbouringMines = Grid.Fields[row, col].NeighbouringMines;

            if (neighbouringMines == 0) {
                CascadeReveal(cell);
            }
            else {
                RevealNeighbouredCell(cell);
            }
        }
        cell.IsRevealed = true;
    }

    private void CascadeReveal(Cell cell) {
        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(cell);

        while (queue.Count > 0) {
            Cell currentCell = queue.Dequeue();
            if (currentCell.IsRevealed) continue;

            currentCell.IsRevealed = true;

            if (currentCell.NeighbouringMines == 0) {
                UIManager.RevealEmptyCell(currentCell.Row, currentCell.Col);
                foreach (Cell neighbour in Grid.GetCellNeighbours(currentCell)) {
                    queue.Enqueue(neighbour);
                }
            }
            else {
                RevealNeighbouredCell(currentCell);
            }

        }

    }

    private void RevealNeighbouredCell(Cell cell) {
        UIManager.RevealNeighbouredCell(cell.Row, cell.Col, cell.NeighbouringMines);
    }

    public void GameOver() {
        print("Game over sequence to be implemented");
    }

    internal void OnCellRightClicked(int row, int col) {
        Cell cell = Grid.Fields[row,col];
        if(cell.IsRevealed) {
            print("revealing vecinity of mine-adjacent cells to be implemented");
        }
        else {
            ToggleCellFlag(cell);
        }
    }

    private void ToggleCellFlag(Cell cell) {

        if(cell.IsFlagged) {
            cell.IsFlagged = false;
            UIManager.UnflagCell(cell.Row,cell.Col);
        }
        else {
            cell.IsFlagged = true;
            UIManager.FlagCell(cell.Row, cell.Col);
        }
    }
}
