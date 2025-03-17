using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine;


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
        var newHint = new FlagsSatisfiedHint(affectedCell, grid, this);
        EnqueueIfUnique(newHint);
        return newHint;
    }
    private MoveHint CreateFlagCellHint(Cell affectedCell) {
        var newHint = new FlagCellHint(affectedCell, grid, this);
        EnqueueIfUnique(newHint);
        flaggableCells.Add(affectedCell);
        return newHint;
    }
    private MoveHint CreateWrongFlagHint(Cell affectedCell) {
        var newHint = new WrongFlagHint(affectedCell, grid, this);
        EnqueueIfUnique(newHint);
        return newHint;
    }
    private MoveHint CreateRevealCellHint(Cell affectedCell) {
        var newHint = new RevealCellHint(affectedCell, grid, this);
        EnqueueIfUnique(newHint);
        return newHint;
    }

    public MoveHint GetHint(bool dequeue = true) {
        FlushObsoleteHints();

        if (hintQueue.Count == 0) {
            GenerateHints();
        }

        if (hintQueue.Count != 0) {

            if (dequeue) {
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
        if (hintQueue.Count != 0) return;
        DebugLog.Log("Simple moves exhausted. Applying multiple otherGroup hints");
        ScanForMultipleGroupHints();
    }

    private void ScanForSingleGroupHints() {
        foreach (var group in groups) {
            if (group.IsMineGroup) {
                foreach (var neighbour in group.Cells) {
                    var newHint = CreateFlagCellHint(neighbour);
                    EnqueueIfUnique(newHint);
                    flaggableCells.Add(neighbour);
                }
            }
            else if (group.IsRevealable) {
                var newHint = CreateFlagsSatisfiedHint(group.Owner);
            }
        }
    }

    private void GenerateGroups() {
        groups.Clear();

        foreach (var cell in grid.ActiveCells) {
            if (!cell.IsRevealed) continue;
            List<Cell> unrevealedNeighbours = grid.GetUnrevealedNeighbours(cell, includeFlagged: false);

            CellGroup newCellGroup = new CellGroup(
                unrevealedNeighbours,
                cell,
                cell.NeighbouringMines - cell.NeighbouringFlags,
                this);

            groups.Add(newCellGroup);
        }
    }

    private void ScanForMultipleGroupHints() {

        foreach (var currentGroup in groups) {
            if (!grid.ActiveCells.Contains(currentGroup.Owner)) continue;
            if (currentGroup.IsMineGroup || currentGroup.IsRevealable) continue;
            foreach (CellGroup otherGroup in groups) {

                if (otherGroup.IsMineGroup || otherGroup.IsRevealable) continue;
                if (!currentGroup.HasOverlapWith(otherGroup) || otherGroup == currentGroup) continue;
                
                CellGroup minesGroup, revealGroup;


                if (currentGroup.MinesInGroup > otherGroup.MinesInGroup) {
                    minesGroup = currentGroup.RemoveCommonCellsWith(otherGroup);
                    revealGroup = otherGroup.RemoveCommonCellsWith(currentGroup);
                }
                else if (otherGroup.MinesInGroup > currentGroup.MinesInGroup) {
                    minesGroup = otherGroup.RemoveCommonCellsWith(currentGroup);
                    revealGroup = currentGroup.RemoveCommonCellsWith(otherGroup);
                }
                else if (currentGroup.IsContainedWithin(otherGroup.Cells)) {
                    minesGroup = currentGroup.RemoveCommonCellsWith(otherGroup);
                    revealGroup = otherGroup.RemoveCommonCellsWith(currentGroup);
                }
                else if (otherGroup.IsContainedWithin(currentGroup.Cells)) {
                    minesGroup = otherGroup.RemoveCommonCellsWith(currentGroup);
                    revealGroup = currentGroup.RemoveCommonCellsWith(otherGroup);
                }
                else {
                    DebugLog.Log("Groups are unable to reveal mines or cells");
                    continue;
                }

                DebugLog.Log($"Exploring otherGroup {currentGroup}\nand {otherGroup}\nreveal group: {revealGroup}\nmine group: {minesGroup}");
                if (revealGroup.IsRevealable) {
                    foreach (var revealableCell in revealGroup.Cells) {
                        var newHint = CreateRevealCellHint(revealableCell);
                    }
                }

                if (minesGroup.IsMineGroup) {
                    foreach (var mineCell in minesGroup.Cells) {
                        var newHint = CreateFlagCellHint(mineCell);
                    }
                }
            }
        }


    }

    private void FlushObsoleteHints() {
        int count = 0;
        while (hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Peek();

            if (!hint.IsObsolete()) break;
            count++;
            allHintsSet.Remove(hint);
            hintQueue.Dequeue();
        }
    }

    public void OnUserToggledFlag(Cell cell) {
        GenerateHints();
        if (cell.IsFlagged) {
            if (flaggableCells.Contains(cell)) return;
            var newHint = CreateWrongFlagHint(cell);
        }
    }

    private bool EnqueueIfUnique(MoveHint hint) {
        if (allHintsSet.Contains(hint)) { return false; }
        allHintsSet.Add(hint);
        hintQueue.Enqueue(hint);
        return true;
    }

    [Obsolete("ScanForSolvableNumberCells is deprecated, use GenerateGroups instead")]
    private void ScanForSolvableNumberCells() {
        foreach (Cell cell in grid.ActiveCells) {
            if (cell.HasAllMinesFlagged && grid.GetUnrevealedNeighbours(cell).Count != 0) {
                MoveHint newHint = CreateFlagsSatisfiedHint(cell);
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
            }
        }
    }
}

