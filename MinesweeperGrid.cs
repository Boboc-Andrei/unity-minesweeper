using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MinesweeperGrid {
    public Cell[,] Fields;
    public int TotalMines => Generator.TotalMines;
    public int Rows;
    public int Columns;

    public GridGenerator Generator;

    public MinesweeperGrid(int rows, int columns, GridGenerator generator) {
        Rows = rows;
        Columns = columns;
        Generator = generator;
        Fields = new Cell[Rows, Columns];
    }


    internal void GenerateMines() {
        if (Fields == null) print("fields are null"); 
        Generator.PlaceMines(Fields);
    }
}
