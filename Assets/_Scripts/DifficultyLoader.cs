using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public interface IDifficultyLoader {
    public Dictionary<Difficulty, DifficultySettings> Load();
}

public class UnityDifficultyJsonLoader : IDifficultyLoader {

    private string path;
    public UnityDifficultyJsonLoader(string _path) {
        path = _path;
    }
    public Dictionary<Difficulty, DifficultySettings> Load() {
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null) {
            DebugLog.LogError("Failed to load JSON file.");
            return null;
        }
        var difficultyOptions = JsonUtility.FromJson<DefaultDifficultySettingsJson>(jsonFile.text);

        if (difficultyOptions == null || difficultyOptions.Items == null) {
            DebugLog.LogError("difficultyOptions is NULL or empty.");
            return null;
        }

        return difficultyOptions.Items.ToDictionary(d => Enum.Parse<Difficulty>(d.Name), d => d);
    }
}