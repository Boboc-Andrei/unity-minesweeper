using System;
using System.Data;

public class GridGenerator {

    public int TotalMines;
    public Random R = new Random();

    public GridGenerator(int totalMines) {

        TotalMines = totalMines;
    }

    internal void PlaceMines(Cell[,] fields) {
        var (rows, cols) = (fields.GetLength(0), fields.GetLength(1));

        for (int _ = 0; _ < TotalMines; _++) {
            int mineRow, mineCol;
            do {
                mineRow = R.Next(0, rows);
                mineCol = R.Next(0, cols);

            } while (!fields[mineRow, mineCol].IsMine);

            fields[mineRow, mineCol].IsMine = true;
        }
    }

}
