using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Scriptable Objects/Difficulty Settings")]
public class DifficultySettings : ScriptableObject{
    public int Rows;
    public int Cols;
    public int Mines;
}
