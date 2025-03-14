using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;


public class MinesweeperSolver {
    private MinesweeperGrid grid;
    public HashSet<Cell> flaggableCells = new HashSet<Cell>();

    private HashSet<MoveHint> allHintsSet = new HashSet<MoveHint>();
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
        ScanForSolvableNumberCells();
        ScanForFlaggableCells();
    }

    private void FlushObsoleteHints() {
        DebugLog.Log("Flushing obsolete hints");
        int count = 0;
        while (hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Peek();

            if (!hint.IsObsolete()) break;
            count++;
            DebugLog.Log($"hint obsolete: {hint.ToString()}");
            allHintsSet.Remove(hint);
            hintQueue.Dequeue();
        }
        DebugLog.Log($"Flushed {count} hints");
    }

    private void ScanForSolvableNumberCells() {
        foreach(Cell cell in grid.activeCells) {
            if (cell.HasAllMinesFlagged && grid.GetUnrevealedNeighbours(cell).Count != 0) {
                MoveHint newHint = CreateFlagsSatisfiedHint(cell);
                if (allHintsSet.Contains(newHint)) { DebugLog.Log($"hint already exists, skipping: {newHint}"); continue; }
                allHintsSet.Add(newHint);
                hintQueue.Enqueue(newHint);
                DebugLog.Log($"enqueued reveal hint: {newHint}");
            }
        }
    }


    private void ScanForFlaggableCells() {
        foreach(Cell cell in grid.activeCells) {
            var unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell);

            if (cell.NeighbouringMines == 0 ||
                !cell.IsRevealed ||
                unrevealedNeighbours.Count == 0 ||
                cell.NeighbouringMines - cell.NeighbouringFlags != unrevealedNeighbours.Count) {
                continue;
            }

            foreach (var neighbour in unrevealedNeighbours) {
                MoveHint newHint = CreateFlagCellHint(neighbour);
                if (allHintsSet.Contains(newHint)) { DebugLog.Log($"hint already exists, skipping: {newHint}"); continue; }
                allHintsSet.Add(newHint);
                flaggableCells.Add(neighbour);
                hintQueue.Enqueue(newHint);
                DebugLog.Log($"enqueued flaggable hint: {newHint}");
            }
        }
    }

    public void OnUserToggledFlag(Cell cell) {
        GenerateHints();
        if (cell.IsFlagged) {
            if (flaggableCells.Contains(cell)) return;
            var newHint = CreateWrongFlagHint(cell);
            if (allHintsSet.Contains(newHint)) { DebugLog.Log($"hint already exists, skipping: {newHint}"); return; }
            allHintsSet.Add(newHint);
            hintQueue.Enqueue(newHint);
            DebugLog.Log($"enqueued wrong flag hint: {newHint}");
        }
    }
}
