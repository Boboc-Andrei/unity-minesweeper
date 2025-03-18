using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
class DifficultySettingsJson {
    public string Name;
    public int Rows;
    public int Cols;
    public int Mines;
}

[Serializable]
class DefaultDifficultySettingsJson {
    public List<DifficultySettingsJson> Items;
}