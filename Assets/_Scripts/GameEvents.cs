using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class GameEvents {
    public static event Action<int, int> OnCellClicked;
    public static event Action<int, int> OnCellRightClicked;

    public static event Action<int, int, int> OnEmptyCellRevealed;
    public static event Action<int, int, bool> OnFlagSet;
    public static event Action<int> OnFlagCounterUpdate;

    public static event Action<int, int> OnMineCellRevealed;
    public static event Action OnGameWon;
        
    public static event Action<List<(int, int)>> OnCellsHighlighted;
    public static event Action OnShowHintClicked;
    public static event Action OnPerformHintClicked;
    public static event Action OnSolveGridClicked;

    public static event Action OnNewGame;
    public static event Action<int, int, Difficulty> OnGridInitialized;
    public static event Action<Difficulty> OnDifficultyChanged;

    public static event Action<List<(int, int)>> OnUpdateActiveCells;

    public static void CellClicked(int row, int col) => OnCellClicked?.Invoke(row, col);
    public static void CellRightClicked(int row, int col) => OnCellRightClicked?.Invoke(row, col);
    public static void EmptyCellRevealed(int row, int col, int neighbouringMines) => OnEmptyCellRevealed?.Invoke(row, col, neighbouringMines);
    public static void MineCellRevealed(int row, int col) => OnMineCellRevealed?.Invoke(row, col);
    public static void DifficultyChanged(Difficulty newDifficulty) => OnDifficultyChanged?.Invoke(newDifficulty);
    public static void NewGame() => OnNewGame?.Invoke();
    public static void GridInitialized(int rows, int cols, Difficulty difficulty) => OnGridInitialized?.Invoke(rows, cols, difficulty);
    public static void FlagSet(int row, int col, bool isFlagged) => OnFlagSet?.Invoke(row, col, isFlagged);
    public static void FlagCounterUpdate(int newValue) => OnFlagCounterUpdate?.Invoke(newValue);
    public static void HighlightCells(List<(int, int)> cellsToHighlight) => OnCellsHighlighted?.Invoke(cellsToHighlight);
    public static void GameWon() => OnGameWon?.Invoke();
    public static void UpdateActiveCells(List<(int, int)> activeCells) => OnUpdateActiveCells?.Invoke(activeCells);
    public static void ShowHintClicked() => OnShowHintClicked?.Invoke();
    public static void PerformHintClicked() => OnPerformHintClicked?.Invoke();
    public static void SolveGridClicked() => OnSolveGridClicked?.Invoke();
}
