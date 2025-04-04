using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class DifficultySettings {
    public string Name;
    public int Rows;
    public int Cols;
    public int Mines;
}

[Serializable]
public class DefaultDifficultySettingsJson {
    public List<DifficultySettings> Items;
}