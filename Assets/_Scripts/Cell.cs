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

    public Cell(int row, int col, bool isMine) {
        Row = row;
        Col = col;
        IsMine = isMine;
    }
}
