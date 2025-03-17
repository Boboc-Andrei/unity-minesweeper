using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class MinesweeperSolver {
    private MinesweeperGrid grid;
    public HashSet<Cell> flaggableCells = new HashSet<Cell>();
    public HashSet<CellGroup> groups = new HashSet<CellGroup>();

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
    private MoveHint CreateRevealCellHint(Cell affectedCell) {
        return new RevealCellHint(affectedCell, grid, this);
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
        GenerateGroups();
        ScanForSingleGroupHints();
    }

    private void ScanForSingleGroupHints() {
        foreach(var group in groups) {
            if (group.IsMineGroup) {
                foreach (var neighbour in group.Cells) {
                    var newHint = CreateFlagCellHint(neighbour);
                    EnqueueIfUnique(newHint);
                    flaggableCells.Add(neighbour);
                }
            }
            else if (group.IsRevealable) {
                var newHint = CreateFlagsSatisfiedHint(group.Owner);
                EnqueueIfUnique(newHint);
            }
        }
    }

    private void GenerateGroups() {
        groups.Clear();

        foreach(var cell in grid.ActiveCells) {
            if (!cell.IsRevealed) continue;
            List<Cell> unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell, includeFlagged: false);

            CellGroup newCellGroup = new CellGroup(
                unrevealedNeighbours,
                cell,
                cell.NeighbouringMines - cell.NeighbouringFlags,
                this);

            foreach(CellGroup group in groups) {

                if (!newCellGroup.HasOverlapWith(group)) continue;

                //(var majorGroup, var minorGroup) = newCellGroup.MinesInGroup > group.MinesInGroup || newCellGroup.Cells.Count > group.Cells.Count ? (newCellGroup, group) : (group, newCellGroup);
                CellGroup majorGroup, minorGroup;


                if (newCellGroup.MinesInGroup > group.MinesInGroup) {
                    majorGroup = newCellGroup;
                    minorGroup = group;
                }
                else if (group.MinesInGroup > newCellGroup.MinesInGroup) {
                    majorGroup = group;
                    minorGroup = newCellGroup;
                }
                else if (newCellGroup.IsContainedWithin(group.Cells)) {
                    majorGroup = group;
                    minorGroup = newCellGroup;
                }
                else if (group.IsContainedWithin(newCellGroup.Cells)) {
                    majorGroup = newCellGroup;
                    minorGroup = group;
                }
                else continue;

                var revealGroup = minorGroup.SubstractFrom(majorGroup);
                if(revealGroup.IsRevealable) {
                    DebugLog.Log($"group of ({majorGroup.Owner.Row},{majorGroup.Owner.Col}) has {majorGroup.MinesInGroup} mines," +
                        $"overlaps with group of ({minorGroup.Owner.Row},{minorGroup.Owner.Col}) with {minorGroup.MinesInGroup} mines." +
                        $"difference group has {revealGroup.Cells.Count} cells to reveal:");
                    foreach (var revealableCell in revealGroup.Cells) {
                        DebugLog.Log($"({revealableCell.Row},{revealableCell.Col})");
                        var newHint = CreateRevealCellHint(revealableCell);
                        EnqueueIfUnique(newHint);
                    }
                }

                var minesGroup = majorGroup.SubstractFrom(minorGroup);
                if(minesGroup.IsMineGroup) {
                    foreach (var mineCell in minesGroup.Cells) {
                        var newHint = CreateFlagCellHint(mineCell);
                        EnqueueIfUnique(newHint);
                        flaggableCells.Add(mineCell);
                    }
                }
            }
            groups.Add(newCellGroup);           
        }
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

    public void OnUserToggledFlag(Cell cell) {
        GenerateHints();
        if (cell.IsFlagged) {
            if (flaggableCells.Contains(cell)) return;
            var newHint = CreateWrongFlagHint(cell);
            if(EnqueueIfUnique(newHint))
                DebugLog.Log($"enqueued wrong flag hint: {newHint}");
        }
    }

    private bool EnqueueIfUnique(MoveHint hint) {
        if (allHintsSet.Contains(hint)) { DebugLog.Log($"hint already exists: {hint}"); return false; }
        allHintsSet.Add(hint);
        hintQueue.Enqueue(hint);
        return true;
    }

    [Obsolete("ScanForSolvableNumberCells is deprecated, use GenerateGroups instead")]
    private void ScanForSolvableNumberCells() {
        foreach (Cell cell in grid.ActiveCells) {
            if (cell.HasAllMinesFlagged && grid.GetUnrevealedNeighbours(cell).Count != 0) {
                MoveHint newHint = CreateFlagsSatisfiedHint(cell);
                if (EnqueueIfUnique(newHint))
                    DebugLog.Log($"enqueued reveal hint: {newHint}");
            }
        }
    }

    [Obsolete("ScanForFlaggableCells is deprecated, use GenerateGroups instead")]
    private void ScanForFlaggableCells() {
        foreach (Cell cell in grid.ActiveCells) {
            var unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell);

            if (cell.NeighbouringMines == 0 ||
                !cell.IsRevealed ||
                unrevealedNeighbours.Count == 0 ||
                cell.NeighbouringMines - cell.NeighbouringFlags != unrevealedNeighbours.Count) {
                continue;
            }

            foreach (var neighbour in unrevealedNeighbours) {
                MoveHint newHint = CreateFlagCellHint(neighbour);
                if (EnqueueIfUnique(newHint))
                    DebugLog.Log($"enqueued flaggable hint: {newHint}");
                flaggableCells.Add(neighbour);
            }
        }
    }
}

