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
    private MoveHint CreateFlagsSatisfiedHint(CellGroup cellGroup) {
        var newHint = new FlagsSatisfiedHint(cellGroup, grid, this);
        EnqueueIfUnique(newHint);
        return newHint;
    }
    private MoveHint CreateFlagCellsHint(CellGroup cellGroup) {
        var newHint = new FlagGroupHint(cellGroup, grid, this);
        EnqueueIfUnique(newHint);
        foreach(var cell in cellGroup.Cells) {
            flaggableCells.Add(cell);
        }
        return newHint;
    }
    private MoveHint CreateWrongFlagHint(Cell cell) {
        var wrongFlagGroup = new CellGroup(new List<Cell> { cell }, cell, -1, this);
        var newHint = new WrongFlagHint(wrongFlagGroup, grid, this);
        EnqueueIfUnique(newHint);
        return newHint;
    }
    private MoveHint CreateRevealCellsHint(CellGroup cellGroup) {
        var newHint = new RevealCellsHint(cellGroup, grid, this);
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
        ScanForMultipleGroupHints();
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

    private void ScanForSingleGroupHints() {
        foreach (var group in groups) {
            if (group.IsMineGroup) {
                var newHint = CreateFlagCellsHint(group);
            }
            else if (group.IsRevealable) {
                var newHint = CreateFlagsSatisfiedHint(group);
            }
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
                    continue;
                }

                if (revealGroup.IsRevealable) {
                    CreateRevealCellsHint(revealGroup);
                }

                if (minesGroup.IsMineGroup) {
                    CreateFlagCellsHint(minesGroup);
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
}

