using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public enum Difficulty {
    Easy, Medium, Hard, Extreme
}

public class GameManager {

    private Dictionary<Difficulty, DifficultySettings> defaultDifficulties;
    private DifficultySettings currentDifficultySettings => defaultDifficulties[CurrentDifficulty];

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

    private DateTime startTime;
    public TimeSpan runningTime => DateTime.Now - startTime;
    public static TimeSpan hintTimePenalty = new TimeSpan(0, 0, 10);

    public int GridRows => currentDifficultySettings.Rows;
    public int GridCols => currentDifficultySettings.Cols;
    public int Mines => currentDifficultySettings.Mines;

    private bool StartNewGameOnDifficultyChange = true;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public MinesweeperSolver Solver;


    private bool GameStarted = false;
    private int HintsUsed = 0;

    private IDifficultyLoader difficultyLoader;

    public GameManager(IDifficultyLoader _difficultyLoader) {
        difficultyLoader = _difficultyLoader;
        defaultDifficulties = difficultyLoader.Load();
    }

    public void SubscribeToEvents() {
        GameEvents.OnCellClicked += OnCellClicked;
        GameEvents.OnCellRightClicked += OnCellRightClicked;
        GameEvents.OnDifficultyChanged += DifficultyChanged;
        GameEvents.OnNewGame += NewGame;
        GameEvents.OnMineCellRevealed += OnMineRevealed;
        GameEvents.OnGameWon += GameWon;
        GameEvents.OnShowHintClicked += ShowHint;
        GameEvents.OnPerformHintClicked += PerformHint;
        GameEvents.OnSolveGridClicked += SolveGrid;
    }

    public void UnSubscribeToEvents() {
        GameEvents.OnCellClicked -= OnCellClicked;
        GameEvents.OnCellRightClicked -= OnCellRightClicked;
        GameEvents.OnDifficultyChanged -= DifficultyChanged;
        GameEvents.OnNewGame -= NewGame;
        GameEvents.OnMineCellRevealed -= OnMineRevealed;
        GameEvents.OnGameWon -= GameWon;
        GameEvents.OnShowHintClicked -= ShowHint;
        GameEvents.OnPerformHintClicked -= PerformHint;
        GameEvents.OnSolveGridClicked -= SolveGrid;
    }

    public void NewGame() {
        GridGenerator = new GridGenerator(currentDifficultySettings);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        GameEvents.GridInitialized(GridRows, GridCols, CurrentDifficulty);
        GameEvents.FlagCounterUpdate(Grid.MinesLeft);
        Solver = new MinesweeperSolver(Grid);
        GameStarted = false;
        HintsUsed = 0;
    }

    public void DifficultyChanged(Difficulty newDifficulty) {
        if (newDifficulty == CurrentDifficulty) return;
        CurrentDifficulty = newDifficulty;
        if (StartNewGameOnDifficultyChange) {
            NewGame();
        }
    }

    public void OnCellClicked(int row, int col) {

        Cell cell = Grid.Fields[row, col];

        if (!GameStarted) {
            GameStarted = true;
            Grid.PlaceMines(guaranteedFree: cell);
            startTime = DateTime.Now;
        }

        if (cell.IsRevealed && cell.HasAllMinesFlagged) {
            Grid.RevealNeighbours(cell);
        }
        else if (!cell.IsFlagged) {
            Grid.RevealCell(cell);
        }

        Solver.GenerateHints();
    }

    private void OnMineRevealed(int row, int col) {
        GameOver();
    }

    internal void OnCellRightClicked(int row, int col) {
        Cell cell = Grid.Fields[row, col];
        if (!cell.IsRevealed) {
            Grid.ToggleFlag(cell);
            Solver.OnUserToggledFlag(cell);
            GameEvents.FlagSet(cell.Row, cell.Col, cell.IsFlagged);
        }
    }

    public void GameOver() {
        LogGameStats(false);
    }

    public void GameWon() {
        LogGameStats(true);

    }

    private void LogGameStats(bool gameWon) {
        var serializedGrid = MinesweeperGridSerializer.Serialize(Grid);
        MinesweeperGameRecord log = new MinesweeperGameRecord() {
            GridId = serializedGrid.Id,
            Time = runningTime,
            IsGameWon = gameWon,
            HintsUsed = HintsUsed
        };
        DebugLog.Log($"Game with Id {serializedGrid.Id} log:\nGame won: {gameWon}\nUsed hints: {HintsUsed}\n Time: {runningTime}");
    }

    internal void ShowHint() {
        HintsUsed += 1;
        MoveHint hint = Solver.GetHint(dequeue: false);
        if (hint == null) {
            DebugLog.Log("Unable to provide hint");
            return;
        }

        List<Cell> cellsToHighlight = hint.GetAffectedCells();
        GameEvents.HighlightCells(cellsToHighlight.Select(cell => (cell.Row, cell.Col)).ToList());
    }

    internal void PerformHint() {
        MoveHint hint = Solver.GetHint();
        if (hint == null) {
            DebugLog.Log("No valid move possible");
            return;
        }
        hint.Solve();
    }

    public void SolveGrid() {
        while (Solver.GetHint() != null) {
            PerformHint();
        }
    }
}
