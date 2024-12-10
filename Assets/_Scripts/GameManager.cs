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

        if (!GameStarted) {
            GameStarted = true;
            Grid.PlaceMines(guaranteedFree: cell);
        }

        if (UIManager == null) {
            Debug.LogError("UIManager is NULL");
        }
        if (cell == null) {
            Debug.LogError("Cell is NULL");
        }

        if (cell.IsRevealed) {
            TryRevealNeighbours(cell);
        }
        else {
            if (!cell.IsFlagged) {
                RevealCell(cell);
            }
        }
    }

    internal void OnCellRightClicked(int row, int col) {
        Cell cell = Grid.Fields[row, col];
        if (!cell.IsRevealed) {
            ToggleCellFlag(cell);
        }
    }

    public void RevealCell(Cell cell) {
        if (cell.IsMine) {
            cell.IsRevealed = true;
            UIManager.RevealMineCell(cell.Row, cell.Col);
            GameOver();
        }
        else {
            CascadeReveal(cell);
        }
    }

    private void CascadeReveal(Cell cell) {
        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(cell);

        while (queue.Count > 0) {
            Cell currentCell = queue.Dequeue();
            if (currentCell.IsRevealed) continue;

            currentCell.IsRevealed = true;
            UIManager.RevealEmptyCell(currentCell.Row, currentCell.Col, currentCell.NeighbouringMines);

            if (currentCell.NeighbouringMines == 0) {
                foreach (Cell neighbour in Grid.GetCellNeighbours(currentCell)) {
                    queue.Enqueue(neighbour);
                }
            }
        }
    }

    public void GameOver() {
        print("Game over sequence to be implemented");
    }

    private void ToggleCellFlag(Cell cell) {

        if (cell.IsFlagged) {
            Grid.ToggleFlag(cell, false);
            UIManager.UnflagCell(cell.Row, cell.Col);
        }
        else {
            Grid.ToggleFlag(cell, true);
            UIManager.FlagCell(cell.Row, cell.Col);
        }
    }

    private void TryRevealNeighbours(Cell cell) {
        if (cell.NeighbouringFlags != cell.NeighbouringMines) return;
        if (cell.IsMine) return;

        foreach (Cell neighbour in Grid.GetCellNeighbours(cell)) {
            if (!neighbour.IsRevealed && !neighbour.IsFlagged) {
                RevealCell(neighbour);
            }
        }
    }
}
