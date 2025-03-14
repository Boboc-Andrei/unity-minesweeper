using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Cell {
    public int Row;
    public int Col;

    public bool IsRevealed = false;
    public bool IsMine;
    public bool IsFlagged = false;

    public int NeighbouringMines;
    public int NeighbouringFlags;
    public int NeighbouringRevealedCells;
    public bool HasAllMinesFlagged => IsRevealed && !IsMine && NeighbouringMines != 0 && NeighbouringFlags == NeighbouringMines;

    public Cell(int row, int col, bool isMine = false) {
        Row = row;
        Col = col;
        IsMine = isMine;
    }
}

public class MineGroup {
    public Cell SourceCell;
    public int MineCount;
    public List<Cell> PossibleMineLocations;

    public MineGroup(Cell sourceCell, int mineCount, List<Cell> possibleMineLocations) {
        SourceCell = sourceCell;
        MineCount = mineCount;
        PossibleMineLocations = possibleMineLocations;
    }

    //public override bool Equals(object? obj) {
    //    if (obj is MineGroup other) {
    //        return SourceCell == other.SourceCell && MineCount == other.MineCount;
    //    }
    //    return false;
    //}
    //public override int GetHashCode() {
    //    return HashCode.Combine(SourceCell, MineCount);
    //}
}
