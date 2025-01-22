using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Difficulty {
    Easy, Medium, Hard, Extreme
}

public class GameManager : MonoBehaviour {

    public DifficultySettings currentDifficultySettings => defaultDifficulties[currentDifficulty];

    public DifficultySettings defaultEasy;
    public DifficultySettings defaultMedium;
    public DifficultySettings defaultHard;
    public DifficultySettings defaultExtreme;

    public Difficulty currentDifficulty = Difficulty.Extreme;

    public Dictionary<Difficulty, DifficultySettings> defaultDifficulties;

    public int GridRows => currentDifficultySettings.Rows;
    public int GridCols => currentDifficultySettings.Cols;
    public int Mines => currentDifficultySettings.Mines;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public UIManager UIManager;

    private bool GameStarted = false;

    void Start() {
        defaultDifficulties = new() {
            { Difficulty.Easy, defaultEasy },
            { Difficulty.Medium, defaultMedium },
            { Difficulty.Hard, defaultHard },
            { Difficulty.Extreme, defaultExtreme},
        };

        UIManager.difficultyDropDown.RegisterCallback<ChangeEvent<Enum>>((evt) => {
            currentDifficulty = (Difficulty)evt.newValue;
            print(evt.newValue);
        });

        NewGame();
    }

    public void NewGame() {
        UIManager.InitializeGridUI(GridRows, GridCols);
        InitializeGrid(GridRows, GridCols);
        UpdateMineCount();
        GameStarted = false;
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
        else if (!cell.IsFlagged) {
            RevealCell(cell);
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
            if (currentCell.IsRevealed || currentCell.IsFlagged) continue;

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
            Grid.FlaggedCells--;
            Grid.ToggleFlag(cell, false);
            UIManager.UnflagCell(cell.Row, cell.Col);
        }
        else {
            Grid.FlaggedCells++;
            Grid.ToggleFlag(cell, true);
            UIManager.FlagCell(cell.Row, cell.Col);
        }
        UIManager.UpdateMineCounter(Grid.TotalMines - Grid.FlaggedCells);
    }

    private void TryRevealNeighbours(Cell cell) {
        if (cell.NeighbouringFlags != cell.NeighbouringMines || cell.IsMine) return;

        foreach (Cell neighbour in Grid.GetCellNeighbours(cell)) {
            if (!neighbour.IsRevealed && !neighbour.IsFlagged) {
                RevealCell(neighbour);
            }
        }
    }
    private void UpdateMineCount() {
        UIManager.UpdateMineCounter(Grid.TotalMines - Grid.FlaggedCells);
    }
}
