using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class GridGenerator {

    public DifficultySettings settings;
    public int TotalMines => settings.Mines;
    public int Rows => settings.Rows;
    public int Cols => settings.Cols;
    public System.Random R = new System.Random();

    public GridGenerator(DifficultySettings _settings) {
        settings = _settings;
    }

    public bool[,] GenerateMines(Cell guaranteedFree) {

        if (TotalMines >= Rows * Cols) {
            Debug.LogError("Eror: Total mines exceeded grid size");
        }

        bool[,] mines = new bool[Rows, Cols];
        List<(int, int)> availableCells = new List<(int, int)>();

        for (int r = 0; r < Rows; r++) {
            for (int c = 0; c < Cols; c++) {
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
