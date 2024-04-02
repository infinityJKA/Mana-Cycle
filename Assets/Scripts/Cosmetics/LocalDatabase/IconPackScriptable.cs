using Cosmetics;
using UnityEngine;

[CreateAssetMenu(fileName = "Icon Pack", menuName = "Cosmetics/Icon Pack Scriptable")]
[System.Serializable]
public class IconPackScriptable : ScriptableObject
{
    public IconPack iconPack;
    public int cost = 500;
}