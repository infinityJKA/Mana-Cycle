using Cosmetics;
using UnityEngine;

[CreateAssetMenu(fileName = "Palette Color", menuName = "Cosmetics/Palette Color Scriptable")]
[System.Serializable]
public class PaletteColorScriptable : ScriptableObject
{
    public PaletteColor paletteColor;
    public int cost = 500;
}