using System.Collections;
using System.Collections.Generic;
using Battle.Board;
using UnityEngine;

// just a place to store some temporary values between scenes
public class Storage
{
    // battlers selected by players
    public static Battle.Battler battler1;
    public static Battle.Battler battler2;
    
    // player/cpu state selected by players
    public static bool isPlayerControlled1 = true;
    public static bool isPlayerControlled2 = false;

    // amount of lives players will start with. used for persistence between arcade levels, and also set via versus mode settings
    public static int lives = 1;
    // in arcade mode- persist HP between matches
    public static int hp;

    /// <summary>
    /// current level tab player is playing a level within if level is not null.
    /// set via SoloMode in LevelLister.cs when the tab is selected, via SetTabCursor()
    /// </summary>
    public static SoloMode.SoloMenuTab tab;

    /** level that the player selected */
    public static SoloMode.Level level;

    // if the current battle should use abilities.
    public static bool enableAbilities = true;

    // When opening sidebar, last selecition is saved here to return to when sidebar is closed.
    public static GameObject storedSelection;

    /// <summary>Index of last item the player hovered in the main menu, start there when re-entering menu</summary>
    public static int lastMainMenuItem = 0; // start on solo mode
    public static GameObject lastSidebarItem;

    /** last index selected in level select list. -1 will select next level that is not cleared, starts off as this */
    public static int lastLevelSelectedIndex = -1;
    public static int lastTabSelectedIndex = -1;

    /** current gamemode selected by player */
    public static GameMode gamemode = GameMode.Versus;
    public enum GameMode {
        Solo,
        Versus
    }

    public static bool isTwoPlayer => gamemode == GameMode.Versus || (level && level.aiBattle);

    // true when R is pressed to select single player level, so convo doesnt skip first line
    public static bool levelSelectedThisInput;
    // true when R is pressed to prevent input for convo being used in postgamemenu
    public static bool convoEndedThisInput;
    // true when hitting pause during a midlevelconvo, to prevent pausing menu
    public static bool convoSkippedThisInput;

    // used in arcade endless
    public static List<SoloMode.Level> nextLevelChoices;

    // If in dual keyboard mode. Player vs player matches will be controlled wtih both sides of the keyboard.
    public static bool useDualKeyboardInput;

    // If in an online mode. (applies only to CharSelect & ManaCycle scenes)
    public static bool online = false;

    // used by some cosmetics scenes. contains a object with the tile visual prefab
    public static TileVisual tileVisualPrefab;
}
