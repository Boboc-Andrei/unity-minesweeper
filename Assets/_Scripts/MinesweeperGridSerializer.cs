using System;
using System.Security.Cryptography;
using System.Text;

public static class MinesweeperGridSerializer {

    public static MinesweeperGrid Deserialize() {
        return default;
    }

    public static MinesweeperGridSerialized Serialize(MinesweeperGrid grid) {
        var sb = new StringBuilder();

        for (int r = 0; r < grid.Rows; r++) {
            for (int c = 0; c < grid.Columns; c++) {
                sb.Append(grid.Fields[r, c].IsMine ? "1" : "0");
            }
        }

        var serialized = new MinesweeperGridSerialized() {
            Rows = grid.Rows,
            Cols = grid.Columns,
            MineCount = grid.TotalMines,
            Layout = sb.ToString(),
            Difficulty = grid.Generator.settings.Name
        };

        serialized.Id = ComputeGridHash(serialized);

        return serialized;
    }

    public static string ComputeGridHash(MinesweeperGridSerialized grid) {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{grid.Rows},{grid.Cols},{grid.MineCount},{grid.Layout}");
        var hashBytes = sha256.ComputeHash(inputBytes);
        return ToHexString(hashBytes);
    }

    private static string ToHexString(byte[] bytes) {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        foreach(byte b in bytes) {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}


[Serializable]
public class MinesweeperGridSerialized {
    public string Id { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public int MineCount { get; set; }
    public string Layout { get; set; }
    public string Difficulty { get; set; }
}

[Serializable]
public class MinesweeperGameRecord {
    public string GridId { get; set; }
    public TimeSpan Time { get; set; }
    public int HintsUsed { get; set; }
    public bool IsGameWon { get; set; }
}