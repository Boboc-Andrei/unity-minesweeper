using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class GridGenerator {

    public int TotalMines;
    public System.Random R = new System.Random();

    public GridGenerator(int totalMines) {
        TotalMines = totalMines;
    }

    public bool[,] GenerateMines(int rows, int cols, Cell guaranteedFree) {

        if (TotalMines >= rows * cols) {
            Debug.LogError("Eror: Total mines exceeded grid size");
        }

        bool[,] mines = new bool[rows, cols];
        List<(int, int)> availableCells = new List<(int, int)>();

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                if (Mathf.Abs(r - guaranteedFree.Row) > 1 || Mathf.Abs(c - guaranteedFree.Col) > 1) {
                    availableCells.Add((r, c));
                }
            }
        }

        for (int _ = 0; _ < TotalMines; _++) {
            int randomIndex;
            randomIndex = R.Next(0, availableCells.Count);
            var (mineRow, mineCol) = availableCells[randomIndex];

            availableCells.RemoveAt(randomIndex);
            mines[mineRow, mineCol] = true;
        }
        return mines;
    }

}
