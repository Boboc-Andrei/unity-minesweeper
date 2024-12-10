using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

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

    public void InitializeFields() {
        bool[,] mines = Generator.GenerateMines(Rows, Columns);

        for (int r = 0; r < Rows; r++) {
            for (int c = 0; c < Columns; c++) {
                Cell newCell = new Cell(r, c, mines[r, c]);
                Fields[r, c] = newCell;
            }
        }

        for(int r= 0; r < Rows; r++) {
            for(int c = 0;c < Columns; c++) {
                Fields[r,c].NeighbouringMines = GetCellNeighbours(Fields[r,c], MinesOnly: true).Count;
            }
        }
    }

    public List<Cell> GetCellNeighbours(Cell cell, bool MinesOnly = false) {
        List<Cell> neighbours = new List<Cell>();
        List<(int, int)> neighbourRelativePositions = new List<(int, int)> {
            (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1)
        };

        foreach(var position in neighbourRelativePositions) {
            var (r,c) = position;
            int neighbourRow = cell.Row + r;
            int neighbourCol = cell.Col + c;
            if(IsValidPosition(neighbourRow, neighbourCol)) {
                if (MinesOnly && !Fields[neighbourRow, neighbourCol].IsMine) continue;
                neighbours.Add(Fields[neighbourRow, neighbourCol]);
            }
        }

        return neighbours;
    }
    
    private bool IsValidPosition (int row, int col) {
        return (row >= 0 && row < Rows && col >= 0 && col < Columns);
    }
}
