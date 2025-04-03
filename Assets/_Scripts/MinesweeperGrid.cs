using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;


public class MinesweeperGrid {
    public Cell[,] Fields;
    public HashSet<Cell> ActiveCells = new HashSet<Cell>();
    public int TotalMines => Generator.TotalMines;
    public bool AllEmptyCellsRevealed => Rows * Columns - TotalMines - RevealedCells == 0;
    public int MinesLeft => TotalMines - FlaggedCells;

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

    #region Getting Neighbours
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
    #endregion

    #region Flags
    private bool IsValidPosition(int row, int col) {
        return (row >= 0 && row < Rows && col >= 0 && col < Columns);
    }

    public void SetFlag(Cell cell, bool isFlagged) {
        if (isFlagged == cell.IsFlagged) return;
        cell.IsFlagged = isFlagged;

        FlaggedCells += isFlagged ? 1 : -1;

        GameEvents.FlagCounterUpdate(MinesLeft);
        GameEvents.FlagSet(cell.Row, cell.Col, isFlagged);
        
        foreach (Cell neighbour in GetCellNeighbours(cell)) {
            neighbour.NeighbouringFlags += isFlagged ? 1 : -1;
        }
        UpdateCellActiveStatus(cell);
    }

    public void ToggleFlag(Cell cell) {
        SetFlag(cell, !cell.IsFlagged);
    }

    #endregion

    #region Revealing Cells

    private void RevealSingleCell(Cell cell) {
        if (cell.IsRevealed) return;
        RevealedCells++;
        cell.IsRevealed = true;

        if (cell.IsMine) {
            GameEvents.MineCellRevealed(cell.Row, cell.Col);
        }
        else {
            GameEvents.EmptyCellRevealed(cell.Row, cell.Col, cell.NeighbouringMines);
            if (AllEmptyCellsRevealed) {
                GameEvents.GameWon();
            }
        }

    }

    public void RevealCell(Cell cell) {
        Queue<Cell> queue = new Queue<Cell>();
        HashSet<Cell> cellsToUpdate = new HashSet<Cell>();

        queue.Enqueue(cell);

        while (queue.Count > 0) {
            Cell currentCell = queue.Dequeue();
            if (currentCell.IsRevealed || currentCell.IsFlagged) continue;

            cellsToUpdate.Add(currentCell);
            RevealSingleCell(currentCell);
            if (currentCell.IsMine) break;

            var neighbours = GetCellNeighbours(currentCell);

            foreach (Cell neighbour in neighbours) {
                if (currentCell.NeighbouringMines == 0) {
                    queue.Enqueue(neighbour);
                }
                cellsToUpdate.Add(neighbour);
            }
        }

        foreach(var updatedCell in cellsToUpdate) {
            UpdateCellActiveStatus(updatedCell);
        }
    }

    public void RevealNeighbours(Cell cell) {
        foreach (Cell neighbour in GetCellNeighbours(cell)) {
            RevealCell(neighbour);
        }
        UpdateCellActiveStatus(cell);
    }
    #endregion

    private void UpdateCellActiveStatus(Cell cell) {
        if(cell.IsRevealed) {
            var unrevealedNeighbours = GetUnrevealedNeighbours(cell);
            if(unrevealedNeighbours.Count == 0) {
                ActiveCells.Remove(cell);
            }
            else {
                ActiveCells.Add(cell);
                foreach (var neighbour in unrevealedNeighbours) {
                    ActiveCells.Add(neighbour);

                }
            }
        }
        else if(cell.IsFlagged) {
            ActiveCells.Remove(cell);
            var revealedNeighbours = GetCellNeighbours(cell).Where(c => c.IsRevealed).ToList();
            foreach (var neighbour in revealedNeighbours) {
                if(GetUnrevealedNeighbours(neighbour).Count == 0) {
                    ActiveCells.Remove(neighbour);
                }
            }
        }
        else {
            var revealedNeighbours = GetCellNeighbours(cell).Where(c => c.IsRevealed).ToList();
            if (revealedNeighbours.Count == 0) ActiveCells.Remove(cell);
            else {
                ActiveCells.Add(cell);
                foreach (var neighbour in revealedNeighbours) {
                    ActiveCells.Add(neighbour);
                }
            }
        }
        GameEvents.UpdateActiveCells(ActiveCells.Select(c => (c.Row, c.Col)).ToList());
    }
}
