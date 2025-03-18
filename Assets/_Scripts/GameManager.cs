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

public class GameManager : MonoBehaviour {

    private DifficultySettingsJson currentDifficultySettings => defaultDifficulties[CurrentDifficulty];
    private Dictionary<Difficulty, DifficultySettingsJson> defaultDifficulties;

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
        SetupDefaultDifficulties();
        NewGame();
    }

    private void SetupDefaultDifficulties() {
        TextAsset jsonFile = Resources.Load<TextAsset>("defaultDifficulties");
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

    public void OnEnable() {
        GameEvents.OnCellClicked += OnCellClicked;
        GameEvents.OnCellRightClicked += OnCellRightClicked;
        GameEvents.OnDifficultyChanged += DifficultyChanged;
        GameEvents.OnNewGame += NewGame;
        GameEvents.OnMineCellRevealed += OnMineRevealed;
        GameEvents.OnGameWon += GameWon;
    }

    public void OnDisable() {
        GameEvents.OnCellClicked -= OnCellClicked;
        GameEvents.OnCellRightClicked -= OnCellRightClicked;
        GameEvents.OnDifficultyChanged -= DifficultyChanged;
        GameEvents.OnNewGame -= NewGame;
        GameEvents.OnMineCellRevealed -= OnMineRevealed;
        GameEvents.OnGameWon -= GameWon;
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
        if(StartNewGameOnDifficultyChange) {
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
            Grid.RevealCellCascading(cell);
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
        print("Game over sequence to be implemented");
    }

    public void GameWon() {
        print("Game won sequence to be implemented");
    }

    internal bool ShowHint() {
        MoveHint hint = Solver.GetHint(dequeue: false);
        if(hint == null) {
            Debug.Log("Unable to provide hint");
            return false;
        }

        List<Cell> cellsToHighlight = hint.GetAffectedCells();
        GameEvents.HighlightCells(cellsToHighlight.Select(cell => (cell.Row, cell.Col)).ToList());
        return true;
    }

    internal bool PerformHint() {
        MoveHint hint = Solver.GetHint();
        if(hint == null) {
            Debug.Log("No valid move possible");
            return false;
        }

        hint.Solve();

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
