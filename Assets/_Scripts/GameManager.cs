using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Difficulty {
    Easy, Medium, Hard, Extreme
}

public class GameManager : MonoBehaviour {

    public DifficultySettings currentDifficultySettings => defaultDifficulties[currentDifficulty];

    [SerializeField]
    public DifficultySettings defaultEasy;
    [SerializeField]
    public DifficultySettings defaultMedium;
    [SerializeField]
    public DifficultySettings defaultHard;
    [SerializeField]
    public DifficultySettings defaultExtreme;
    [SerializeField]
    enum SolveSpeed { Slow, Fast, Instant};
    [SerializeField]
    private SolveSpeed solveSpeed;


    public Difficulty currentDifficulty = Difficulty.Extreme;

    public Dictionary<Difficulty, DifficultySettings> defaultDifficulties;

    public int GridRows => currentDifficultySettings.Rows;
    public int GridCols => currentDifficultySettings.Cols;
    public int Mines => currentDifficultySettings.Mines;
    private bool StartNewGameOnDifficultyChange = true;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public UIManager UIManager;
    public MinesweeperSolver Solver;

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
            if(StartNewGameOnDifficultyChange) {
                NewGame();
            }
        });

        NewGame();
    }

    public void NewGame() {
        UIManager.InitializeGridUI(GridRows, GridCols);

        GridGenerator = new GridGenerator(Mines);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        Grid.InitializeCells();

        UpdateMineCount();
        Solver = new MinesweeperSolver(Grid);
        GameStarted = false;
    }

    public void OnCellClicked(int row, int col) {

        Cell cell = Grid.Fields[row, col];
        bool firstClick = false;

        if (!GameStarted) {
            GameStarted = true;
            firstClick = true;
            Grid.PlaceMines(guaranteedFree: cell);
        }

        if (UIManager == null) { Debug.LogError("UIManager is NULL"); }
        if (cell == null) { Debug.LogError("Cell is NULL"); }

        if (cell.IsRevealed) {
            RevealNeighbours(cell);
        }
        else if (!cell.IsFlagged) {
            RevealCell(cell);
        }

        if(firstClick) {
            Solver.GenerateHints();
        }
    }

    internal void OnCellRightClicked(int row, int col) {
        Cell cell = Grid.Fields[row, col];
        if (!cell.IsRevealed) {
            ToggleFlag(cell);
            Solver.OnUserToggledFlag(cell);
        }
    }

    public void RevealCell(Cell cell) {
        if (cell.IsMine) {
            Grid.RevealCell(cell);
            UIManager.RevealMineCell(cell.Row, cell.Col);
            GameOver();
        }
        else {
            CascadeReveal(cell);
            if(Grid.AllEmptyCellsRevealed) {
                GameWon();
            }
        }
    }

    private void CascadeReveal(Cell cell) {
        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(cell);

        while (queue.Count > 0) {
            Cell currentCell = queue.Dequeue();
            if (currentCell.IsRevealed || currentCell.IsFlagged) continue;

            Grid.RevealCell(currentCell);
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

    public void GameWon() {
        print("Game won sequence to be implemented");
    }

    private void SetFlag(Cell cell, bool isFlag) {
        if (isFlag == cell.IsFlagged) return;
        Grid.SetFlag(cell, isFlag);
        UIManager.SetFlag(cell.Row, cell.Col, isFlag);
        UIManager.UpdateMineCounter(Grid.TotalMines - Grid.FlaggedCells);
    }
    private void ToggleFlag(Cell cell) {
        SetFlag(cell, !cell.IsFlagged);
    }

    private void RevealNeighbours(Cell cell) {
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

    internal bool ShowHint() {
        MoveHint hint = Solver.PeekHint();
        if(hint == null) {
            Debug.Log("Unable to provide hint");
            return false;
        }

        List<Cell> cellsToHighlight = new List<Cell>();
        switch (hint.Type) {
            case HintType.FlagsSatisfied:
                cellsToHighlight = Grid.GetUnrevealedNeighbours(hint.AffectedCell, true);
                cellsToHighlight.Add(hint.AffectedCell);
                UIManager.HighlightCells(cellsToHighlight);
                break;
            case HintType.FlagCell:
                cellsToHighlight.Add(hint.AffectedCell);
                UIManager.HighlightCells(cellsToHighlight);
                break;
            case HintType.WrongFlag:
                cellsToHighlight.Add(hint.AffectedCell);
                UIManager.HighlightCells(cellsToHighlight);
                Debug.Log($"Cell ({hint.AffectedCell.Row}, {hint.AffectedCell.Col}) has wrong flag");
                break;
        }
        return true;
    }

    internal bool PerformHint() {
        MoveHint hint = Solver.DequeueHint();
        if(hint == null) {
            Debug.Log("No valid move possible");
            return false;
        }

        List<Cell> cellsToHighlight = new List<Cell>();
        switch (hint.Type) {
            case HintType.FlagsSatisfied:
                RevealNeighbours(hint.AffectedCell);
                break;
            case HintType.FlagCell:
                SetFlag(hint.AffectedCell, true);
                break;
            case HintType.WrongFlag:
                cellsToHighlight.Add(hint.AffectedCell);
                SetFlag(hint.AffectedCell, false);
                break;
        }
        return true;
    }

    public void SolveGrid() {
        switch(solveSpeed) {
            case SolveSpeed.Instant:
                SolveGridInstant();
                break;
            case SolveSpeed.Fast:
                StartCoroutine(SolveGridDelayed(0f));
                break;
            case SolveSpeed.Slow:
                StartCoroutine(SolveGridDelayed(.2f));
                break;
        }
    }

    public IEnumerator SolveGridDelayed(float delay) {
        bool hasSolvableMoves = true;
        while (hasSolvableMoves) {
            hasSolvableMoves = PerformHint();
            yield return new WaitForSeconds(delay);
        }
    }

    public void SolveGridInstant() {
        bool hasSolvableMoves = true;
        while (hasSolvableMoves) {
            hasSolvableMoves = PerformHint();
        }
    }
}
