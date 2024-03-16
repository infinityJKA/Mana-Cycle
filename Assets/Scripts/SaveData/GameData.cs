
using System.Collections.Generic;

[System.Serializable]
public class GameData {
    private static GameData _current;
    public static GameData current {
        get {
            if (_current == null) SerializationManager.Load();
            return _current;
        }
        set {
            _current = value;
        }
    }

    public PlayerProfile profile;

    public HashSet<string> achievementsUnlocked;

    public Dictionary<string, LevelData> levelData;
}

[System.Serializable]
public class LevelData
{
    public bool beaten;
    public int highScore;
}

[System.Serializable]
public class PlayerProfile
{
    public string playerName = "Cycler123";
    public int coins = 0;
    public int level = 1;
    public int xp = 0;
}