using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;


public class MinesweeperSolver {
    private MinesweeperGrid grid;
    public HashSet<Cell> flaggableCells = new HashSet<Cell>();
    private HashSet<Cell> userPlacedFlags = new HashSet<Cell>();

    public HintPriorityQueue<MoveHint> hintQueue = new();
    public int WrongFlagsCount => hintQueue.PriorityCount(0);

    public int flaggableCellsCount => flaggableCells.Count;
    public MinesweeperSolver(MinesweeperGrid grid) {
        this.grid = grid;
    }
    private MoveHint CreateFlagsSatisfiedHint(Cell affectedCell) {
        return new FlagsSatisfiedHint(affectedCell, grid, this);
    }
    private MoveHint CreateFlagCellHint(Cell affectedCell) {
        return new FlagCellHint(affectedCell, grid, this);
    }
    private MoveHint CreateWrongFlagHint(Cell affectedCell) {
        return new WrongFlagHint(affectedCell, grid, this);
    }


    public MoveHint GetHint(bool dequeue = true) {
        FlushObsoleteHints();

        if(hintQueue.Count == 0) {
            GenerateHints();
        }
        if(hintQueue.Count != 0) {

            if(dequeue) {
                return hintQueue.Dequeue();
            }
            else {
                return hintQueue.Peek();
            }
        }

        return null;
    }

    public void GenerateHints() {
        hintQueue.Clear();
        ScanForFlaggableCells();
        ScanForWrongFlags();
        ScanForSolvableNumberCells();
    }

    private void FlushObsoleteHints() {
        DebugLog.Log("Flushing obsolete hints");
        int count = 0;
        while (hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Peek();

            if (!hint.IsObsolete()) break;
            count++;
            DebugLog.Log($"hint obsolete: {hint.ToString()}");
            hintQueue.Dequeue();
        }
        DebugLog.Log($"Flushed {count} hints");
    }

    private void ScanForSolvableNumberCells() {
        foreach(Cell cell in grid.activeCells) {
            if (cell.HasAllMinesFlagged && grid.GetUnrevealedNeighbours(cell).Count != 0) {
                MoveHint newHint = CreateFlagsSatisfiedHint(cell);
                hintQueue.Enqueue(newHint);
                DebugLog.Log($"enqueued reveal hint: {newHint}");
            }
        }
    }

    private void ScanForFlaggableCells() {
        for(int row = 0; row < grid.Rows; row++) {
            for(int col = 0; col < grid.Columns; col++) {
                Cell cell = grid.Fields[row, col];
                var unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell);


                if (!cell.IsRevealed) continue;
                if (cell.NeighbouringMines == 0) continue;
                if (unrevealedNeighbours.Count == 0) continue;
                if (cell.NeighbouringMines - ConfirmedCellNeighbouringFlags(cell) != unrevealedNeighbours.Count) continue;

                foreach (var neighbour in unrevealedNeighbours) {
                    MoveHint newHint = CreateFlagCellHint(neighbour);
                    flaggableCells.Add(neighbour);
                    hintQueue.Enqueue(newHint);
                    DebugLog.Log($"enqueued flaggable hint: {newHint}");
                }
            }
        }
    }

    private int ConfirmedCellNeighbouringFlags(Cell cell) {
        return grid.GetCellNeighbours(cell).Where(n => flaggableCells.Contains(n)).Count();
    }

    private void ScanForWrongFlags() {
        foreach(var flaggedCell in userPlacedFlags) {
            if (flaggableCells.Contains(flaggedCell)) continue;
            var newHint = CreateWrongFlagHint(flaggedCell);
            hintQueue.Enqueue(newHint);
            DebugLog.Log($"enqueued wrong flag hint: {newHint}");
        }
    }

    public void OnUserToggledFlag(Cell cell) {
        if (cell.IsFlagged) {
            userPlacedFlags.Add(cell);
        }
        else {
            userPlacedFlags.Remove(cell);
        }
    }
}
