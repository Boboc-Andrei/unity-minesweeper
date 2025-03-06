using System;
using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public enum HintType {
    FlagCell, FlagsSatisfied, WrongFlag
}

public class MoveHint {
    public HintType Type { get; set; }
    public Cell AffectedCell { get; set; }

    public MoveHint(HintType type, Cell affectedCell) {
        Type = type;
        AffectedCell = affectedCell;
    }

    public override bool Equals(object? obj) {
        if(obj is MoveHint other) {
            return Type == other.Type && AffectedCell == other.AffectedCell;
        }
        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Type, AffectedCell);
    }
}

public class MinesweeperSolver {
    private MinesweeperGrid grid;
    private Queue<MoveHint> hintQueue = new Queue<MoveHint>();
    private HashSet<MoveHint> hintSet = new HashSet<MoveHint>();
    private List<MoveHint> wrongFlagsHints = new List<MoveHint>();
    private HashSet<Cell> flaggableCells = new HashSet<Cell>();

    public int flaggableCellsCount => flaggableCells.Count;
    public MinesweeperSolver(MinesweeperGrid grid) {
        this.grid = grid;
    }

    public MoveHint DequeueHint() {
        if(wrongFlagsHints.Count != 0) {
            MoveHint wrongFlagHint = wrongFlagsHints[0];
            wrongFlagsHints.Remove(wrongFlagHint);
            return wrongFlagHint;
        }

        if(hintQueue.Count == 0) {
            GenerateHints();
        }

        while(hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Dequeue();

            switch(hint.Type) {
                case HintType.FlagCell:
                    if(hint.AffectedCell.IsFlagged) {
                        continue;
                    }
                    else {
                        return hint;
                    }
                default:
                    return hint;
            }
        }
        return null;
    }

    public MoveHint PeekHint() {
        if (wrongFlagsHints.Count != 0) {
            MoveHint wrongFlagHint = wrongFlagsHints[0];
            return wrongFlagHint;
        }

        if (hintQueue.Count == 0) {
            GenerateHints();
        }

        bool foundValidHint = false;
        while (!foundValidHint || hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Peek();

            switch (hint.Type) {
                case HintType.FlagCell:
                    if (hint.AffectedCell.IsFlagged) {
                        hintQueue.Dequeue();
                        continue;
                    }
                    else {
                        return hint;
                    }
                default:
                    return hint;
            }
        }
        return null;
    }

    public void GenerateHints() {
        hintQueue.Clear();
        hintSet.Clear();
        ScanForSolvableNumberCells();
        ScanForFlaggableCells();
    }

    private void ScanForSolvableNumberCells() {
        for (int row = 0; row < grid.Rows; row++) {
            for (int col = 0; col < grid.Columns; col++) {
                Cell cell = grid.Fields[row, col];
                if (cell.HasAllMinesFlagged && grid.GetUnrevealedNeighbours(cell).Count != 0) {
                    MoveHint hint = new MoveHint(HintType.FlagsSatisfied, cell);
                    if (hintSet.Contains(hint)) continue;
                    hintQueue.Enqueue(hint);
                    hintSet.Add(hint);
                }
            }
        }
    }

    private void ScanForFlaggableCells() {
        for (int row = 0; row < grid.Rows; row++) {
            for (int col = 0; col < grid.Columns; col++) {
                Cell cell = grid.Fields[row, col];
                var unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell);

                if(cell.NeighbouringMines == 0 ||
                    !cell.IsRevealed ||
                    unrevealedNeighbours.Count == 0 ||
                    cell.NeighbouringMines - cell.NeighbouringFlags != unrevealedNeighbours.Count) {
                    continue;
                }

                foreach(var neighbour in unrevealedNeighbours) {
                    MoveHint hint = new MoveHint(HintType.FlagCell, neighbour);
                    if (!flaggableCells.Contains(neighbour)) flaggableCells.Add(neighbour);
                    if (hintSet.Contains(hint)) continue;
                    hintQueue.Enqueue(hint);
                    hintSet.Add(hint);
                }
            }
        }
    }

    public void OnUserToggledFlag(Cell cell) {
        if(cell.IsFlagged) {
            if (flaggableCells.Contains(cell)) return;
            wrongFlagsHints.Add(new MoveHint(HintType.WrongFlag, cell));
        }
        else {
            MoveHint possibleWrongFlag = new MoveHint(HintType.WrongFlag, cell);
            if (wrongFlagsHints.Contains(possibleWrongFlag)) {
                wrongFlagsHints.Remove(possibleWrongFlag);
            }
        }
    }


}