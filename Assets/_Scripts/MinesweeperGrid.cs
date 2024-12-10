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
        InitializeFields();
    }

    private void InitializeFields() {
        for(int r = 0; r < Rows; r++) {
            for (int c = 0; c < Columns; c++) {
                Fields[r,c] = new Cell(r,c,false);
            }
        }
    }

    public void GenerateMines() {
        Generator.PlaceMines(Fields);
    }
}
