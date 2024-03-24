using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This file contains non-platform specific settings to sync between clients
/// This includes: default CPU levels, equipped cosmetics, etc
/// </summary>
[System.Serializable]
public class Settings {
    public static Settings current {get; private set;}

    // Saving & loading
    private static readonly string filePath;
    static Settings() {
        filePath = Path.Join(Application.persistentDataPath, "settings.sav");
        Load();
    }
    public static void Save() {
        FileStorageManager.Save(current, filePath, encrypt: false);
    }
    public static void Load() {
        current = FileStorageManager.Load<Settings>(filePath, decrypt: false);
    }
    public static void ClearSettings() {
        current = new Settings();
    }

    // ===== DATA

    /// <summary>
    /// If ghost piece is drawn for player 1. On by default
    /// </summary>
    public bool drawGhostPiece = true;
    public bool drawGhostPieceP2 = true;

    /// <summary>
    /// If abilities are enabled in versus mode/
    /// </summary>
    public bool enableAbilities = true;

    /// <summary>
    /// Amount of lives each player has in versus mode
    /// </summary>
    public int versusLives = 1;  

    // CPU levles for:
    // player vs cpu
    // cpu vs. cpu (p1)
    // cpu vs. cpu (p2)  
    public int cpuLevel = 3, cvcP1Level = 3, cvcP2Level = 3;
}