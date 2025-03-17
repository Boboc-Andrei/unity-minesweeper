using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CellGroup
{
    public Cell Owner;
    public List<Cell> Cells;
    public int MinesInGroup;
    private MinesweeperSolver Solver;
    public bool IsEmpty => Cells.Count == 0;
    public bool IsMineGroup => MinesInGroup == Cells.Count;
    public bool IsRevealable => MinesInGroup == 0;

    public CellGroup(List<Cell> cells, Cell owner, int minesInGroup, MinesweeperSolver solver) {
        Cells = cells;
        MinesInGroup = minesInGroup;
        Solver = solver;
        Owner = owner;
    }

    public bool Contains(Cell cell) {
        return Cells.Contains(cell);
    }

    public bool IsContainedWithin(List<Cell> otherGroupCells) {
        foreach (var cell in Cells) {
            if (!otherGroupCells.Contains(cell)) return false;
        }
        return true;
    }

    public bool IsObsolete() {
        foreach(var cell in Cells) {
            if (!cell.IsRevealed || (cell.IsFlagged && Solver.flaggableCells.Contains(cell)))
                return false;
        }
        return true;
    }

    public CellGroup SubstractFrom(CellGroup other) {
        List<Cell> newGroupCells = new();

        foreach(var cell in other.Cells) {
            if (!Cells.Contains(cell)) {
                newGroupCells.Add(cell);
            }
        }

        int newGroupMinesCount = other.MinesInGroup - MinesInGroup;

        return new CellGroup(newGroupCells, other.Owner, newGroupMinesCount, Solver);
    }
}
