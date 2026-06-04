using UnityEngine;

public static class GameData
{
    public static int p1SelectedChar = 0;
    public static int p2SelectedChar = 0;
    public static int currentProgress = 0;
    public static bool justClearedPlanet = false;
    public static bool isFuelAcquired = false;

    static GameData()
    {
        currentProgress = PlayerPrefs.GetInt("SaveProgress", 0);
    }
}