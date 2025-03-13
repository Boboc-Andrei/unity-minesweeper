using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Difficulty {
    Easy, Medium, Hard, Extreme
}

public class GameManager : MonoBehaviour {

    private DifficultySettings currentDifficultySettings => defaultDifficulties[CurrentDifficulty];

    [SerializeField]
    public DifficultySettings defaultEasy;
    [SerializeField]
    public DifficultySettings defaultMedium;
    [SerializeField]
    public DifficultySettings defaultHard;
    [SerializeField]
    public DifficultySettings defaultExtreme;
    [SerializeField]
    enum SolveSpeed { Slow, Fast, Instant };
    [SerializeField]
    private SolveSpeed solveSpeed;

    [SerializeField]
    private Difficulty _currentDifficulty = Difficulty.Medium;
    public Difficulty CurrentDifficulty {
        get {
            return _currentDifficulty;
        }
        set {
            _currentDifficulty = value;
            GameEvents.DifficultyChanged(value);
        }
    }

    public Dictionary<Difficulty, DifficultySettings> defaultDifficulties;

    public int GridRows => currentDifficultySettings.Rows;
    public int GridCols => currentDifficultySettings.Cols;
    public int Mines => currentDifficultySettings.Mines;
    [SerializeField]
    private bool StartNewGameOnDifficultyChange = true;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public MinesweeperSolver Solver;

    private bool GameStarted = false;

    void Start() {
        defaultDifficulties = new() {
            { Difficulty.Easy, defaultEasy },
            { Difficulty.Medium, defaultMedium },
            { Difficulty.Hard, defaultHard },
            { Difficulty.Extreme, defaultExtreme},
        };

        NewGame();
    }

    public void OnEnable() {
        GameEvents.OnCellClicked += OnCellClicked;
        GameEvents.OnCellRightClicked += OnCellRightClicked;
        GameEvents.OnDifficultyChanged += DifficultyChanged;
        GameEvents.OnNewGame += NewGame;
    }

    public void OnDisable() {
        
    }
    public void NewGame() {

        GridGenerator = new GridGenerator(Mines);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        Grid.InitializeCells();
        GameEvents.GridInitialized(GridRows, GridCols);
        GameEvents.FlagCounterUpdate(Grid.MinesLeft);

        Solver = new MinesweeperSolver(Grid);
        GameStarted = false;
    }

    public void DifficultyChanged(Difficulty newDifficulty) {
        if (newDifficulty == CurrentDifficulty) return;
        CurrentDifficulty = newDifficulty;
        if(StartNewGameOnDifficultyChange) {
            NewGame();
        }
    }

    public void OnCellClicked(int row, int col) {

        Cell cell = Grid.Fields[row, col];
        bool firstClick = false;

        if (!GameStarted) {
            GameStarted = true;
            firstClick = true;
            Grid.PlaceMines(guaranteedFree: cell);
        }

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
            GameEvents.FlagSet(cell.Row, cell.Col, cell.IsFlagged);
        }
    }

    public void RevealCell(Cell cell) {
        if (cell.IsMine) {
            Grid.RevealCell(cell);
            GameEvents.MineCellRevealed(cell.Row, cell.Col);
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
            GameEvents.EmptyCellRevealed(currentCell.Row, currentCell.Col, currentCell.NeighbouringMines);

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
        GameEvents.FlagSet(cell.Row, cell.Col, isFlag);
    }
    private void ToggleFlag(Cell cell) {
        SetFlag(cell, !cell.IsFlagged);
    }

    private void RevealNeighbours(Cell cell) {
        if (cell.NeighbouringFlags != cell.NeighbouringMines || cell.IsMine) return;

        foreach (Cell neighbour in Grid.GetCellNeighbours(cell)) {
            if (neighbour.IsRevealed || neighbour.IsFlagged) continue;
            RevealCell(neighbour);
        }
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
                break;
            case HintType.FlagCell:
                cellsToHighlight.Add(hint.AffectedCell);
                break;
            case HintType.WrongFlag:
                cellsToHighlight.Add(hint.AffectedCell);
                break;
        }
        GameEvents.CellsHighlighted(cellsToHighlight.Select(cell => (cell.Row, cell.Col)).ToList());
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
