using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Difficulty {
    Easy, Medium, Hard, Extreme
}

public class GameManager {

    private Dictionary<Difficulty, DifficultySettingsJson> defaultDifficulties;
    private DifficultySettingsJson currentDifficultySettings => defaultDifficulties[CurrentDifficulty];

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

    public int GridRows => currentDifficultySettings.Rows;
    public int GridCols => currentDifficultySettings.Cols;
    public int Mines => currentDifficultySettings.Mines;

    [SerializeField]
    private bool StartNewGameOnDifficultyChange = true;

    public MinesweeperGrid Grid;
    public GridGenerator GridGenerator;
    public MinesweeperSolver Solver;

    private bool GameStarted = false;

    public GameManager(string defaultDifficultiesPath) {
        SetupDefaultDifficulties(defaultDifficultiesPath);
    }

    private void SetupDefaultDifficulties(string defaultDifficultiesPath) {
        TextAsset jsonFile = Resources.Load<TextAsset>(defaultDifficultiesPath);
        if (jsonFile == null) {
            Debug.LogError("Failed to load JSON file.");
            return;
        }
        var difficultyOptions = JsonUtility.FromJson<DefaultDifficultySettingsJson>(jsonFile.text);

        if (difficultyOptions == null || difficultyOptions.Items == null) {
            Debug.LogError("difficultyOptions is NULL or empty.");
            return;
        }

        defaultDifficulties = difficultyOptions.Items.ToDictionary(d => Enum.Parse<Difficulty>(d.Name), d => d);
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
        GridGenerator = new GridGenerator(Mines);
        Grid = new MinesweeperGrid(GridRows, GridCols, GridGenerator);
        Grid.InitializeCells();
        GameEvents.GridInitialized(GridRows, GridCols, CurrentDifficulty);
        GameEvents.FlagCounterUpdate(Grid.MinesLeft);

        Solver = new MinesweeperSolver(Grid);
        GameStarted = false;
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
        DebugLog.Log("Game over sequence to be implemented");
    }

    public void GameWon() {
        DebugLog.Log("Game won sequence to be implemented");
    }

    internal void ShowHint() {
        MoveHint hint = Solver.GetHint(dequeue: false);
        if (hint == null) {
            Debug.Log("Unable to provide hint");
            return;
        }

        List<Cell> cellsToHighlight = hint.GetAffectedCells();
        GameEvents.HighlightCells(cellsToHighlight.Select(cell => (cell.Row, cell.Col)).ToList());
    }

    internal void PerformHint() {
        MoveHint hint = Solver.GetHint();
        if (hint == null) {
            Debug.Log("No valid move possible");
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
