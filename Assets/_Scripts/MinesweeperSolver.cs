using System.Diagnostics;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;


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

    public MoveHint GetHint(bool dequeue = true) {
        if(wrongFlagsHints.Count != 0) {
            MoveHint wrongFlagHint = wrongFlagsHints[0];
            if(dequeue) wrongFlagsHints.Remove(wrongFlagHint);
            return wrongFlagHint;
        }

        if(hintQueue.Count == 0) {
            GenerateHints();
        }

        while(hintQueue.Count != 0) {
            MoveHint hint = hintQueue.Peek();

            if (hint.IsObsolete()) {
                hintQueue.Dequeue();
            }
            else {
                if (dequeue) hintQueue.Dequeue();
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
                    MoveHint newHint = CreateFlagsSatisfiedHint(cell);
                    if (hintSet.Contains(newHint)) continue;
                    hintQueue.Enqueue(newHint);
                    hintSet.Add(newHint);
                }
            }
        }
    }

    private MoveHint CreateFlagsSatisfiedHint(Cell affectedCell) {
        return new FlagsSatisfiedHint(affectedCell, grid);
    }
    private MoveHint CreateFlagCellHint(Cell affectedCell) {
        return new FlagCellHint(affectedCell, grid);
    }
    private MoveHint CreateWrongFlagHint(Cell affectedCell) {
        return new WrongFlagHint(affectedCell, grid);
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
                    MoveHint newHint = CreateFlagCellHint(neighbour);
                    if (!flaggableCells.Contains(neighbour)) flaggableCells.Add(neighbour);
                    if (hintSet.Contains(newHint)) continue;
                    hintQueue.Enqueue(newHint);
                    hintSet.Add(newHint);
                }
            }
        }
    }

    public void OnUserToggledFlag(Cell cell) {
        if(cell.IsFlagged) {
            if (flaggableCells.Contains(cell)) return;
            wrongFlagsHints.Add(CreateWrongFlagHint(cell));
        }
        else {
            MoveHint possibleWrongFlag = CreateWrongFlagHint(cell);
            if (wrongFlagsHints.Contains(possibleWrongFlag)) {
                wrongFlagsHints.Remove(possibleWrongFlag);
            }
        }
    }
}