using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinesweeperGrid {
    public Cell[,] Fields;
    public int TotalMines => Generator.TotalMines;

    public bool AllEmptyCellsRevealed => Rows * Columns - TotalMines - RevealedCells == 0;

    public int RevealedCells = 0;
    public int FlaggedCells = 0;

    public int Rows;
    public int Columns;

    public GridGenerator Generator;

    private static List<(int, int)> neighbourRelativePositions = new List<(int, int)> {
            (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1)
        };

    public MinesweeperGrid(int rows, int columns, GridGenerator generator) {
        Rows = rows;
        Columns = columns;
        Generator = generator;
        Fields = new Cell[Rows, Columns];
    }

    public void InitializeCells() {
        for (int row = 0; row < Rows; row++) {
            for (int col = 0; col < Columns; col++) {
                Cell newCell = new Cell(row, col);
                Fields[row, col] = newCell;
            }
        }
    }

    public void PlaceMines(Cell guaranteedFree) {

        bool[,] minesMatrix = Generator.GenerateMines(Rows, Columns, guaranteedFree);
        for (int r = 0; r < Rows; r++) {
            for (int c = 0; c < Columns; c++) {
                Fields[r, c].IsMine = minesMatrix[r, c];
            }
        }

        for (int r = 0; r < Rows; r++) {
            for (int c = 0; c < Columns; c++) {
                Fields[r, c].NeighbouringMines = GetCellNeighbours(Fields[r, c], MinesOnly: true).Count;
            }
        }

    }

    public List<Cell> GetCellNeighbours(Cell cell, bool MinesOnly = false) {
        List<Cell> neighbours = new List<Cell>();

        foreach (var position in neighbourRelativePositions) {
            var (rowDelta, colDelta) = position;
            int neighbourRow = cell.Row + rowDelta;
            int neighbourCol = cell.Col + colDelta;

            if (!IsValidPosition(neighbourRow, neighbourCol)) continue;
            if (MinesOnly && !Fields[neighbourRow, neighbourCol].IsMine) continue;

            neighbours.Add(Fields[neighbourRow, neighbourCol]);
        }

        return neighbours;
    }

    public List<Cell> GetUnrevealedNeighbours(Cell cell, bool includeFlagged = false) {
        List<Cell> neighbours = new List<Cell>();

        foreach (var position in neighbourRelativePositions) {
            var (rowDelta, colDelta) = position;
            int neighbourRow = cell.Row + rowDelta;
            int neighbourCol = cell.Col + colDelta;

            if (!IsValidPosition(neighbourRow, neighbourCol)) continue;
            if (Fields[neighbourRow, neighbourCol].IsRevealed || (Fields[neighbourRow, neighbourCol].IsFlagged && !includeFlagged)) continue;

            neighbours.Add(Fields[neighbourRow, neighbourCol]);
        }

        return neighbours;
    }

    private bool IsValidPosition(int row, int col) {
        return (row >= 0 && row < Rows && col >= 0 && col < Columns);
    }

    public void SetFlag(Cell cell, bool isFlagged) {
        if (isFlagged == cell.IsFlagged) return;
        cell.IsFlagged = isFlagged;

        FlaggedCells += isFlagged ? 1 : -1;
        foreach (Cell neighbour in GetCellNeighbours(cell)) {
            neighbour.NeighbouringFlags += isFlagged ? 1 : -1;
        }
    }

    public void RevealCell(Cell cell) {
        if (!cell.IsRevealed) RevealedCells++;
        cell.IsRevealed = true;
    }
}
