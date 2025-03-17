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
    public bool IsMineGroup => MinesInGroup == Cells.Count && !IsEmpty;
    public bool IsRevealable => MinesInGroup == 0 && !IsEmpty;

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

    public bool HasOverlapWith(CellGroup other) {
        foreach(var cell in Cells) {
            if (other.Contains(cell)) return true;
        }
        return false;
    }

    public bool IsObsolete() {
        foreach(var cell in Cells) {
            if (!cell.IsRevealed || (cell.IsFlagged && Solver.flaggableCells.Contains(cell)))
                return false;
        }
        return true;
    }

    public CellGroup RemoveCommonCellsWith(CellGroup other) {
        List<Cell> newGroupCells = new();

        foreach(var cell in Cells) {
            if (!other.Contains(cell)) {
                newGroupCells.Add(cell);
            }
        }

        int newGroupMinesCount = MinesInGroup - other.MinesInGroup;

        return new CellGroup(newGroupCells, other.Owner, newGroupMinesCount, Solver);
    }

    public override string ToString() {
        string output = $"(mines: {MinesInGroup}, cells: [";
        foreach(var cell in Cells) {
            output += $"({cell.Row},{cell.Col}),";
        }
        output += "]";
        return output;
    }
}
