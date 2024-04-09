using UnityEngine;
using Battle.Board;

namespace Cosmetics
{
    [System.Serializable]
    public class PaletteColor : CosmeticItem
    {
        public Color32 mainColor = Color.white;
        public Color32 darkColor = Color.black;
    

        public override GameObject MakeIcon(Transform parent)
        {
            TileVisual tileVisual = Object.Instantiate(Storage.tileVisualPrefab.gameObject, parent, false).GetComponent<TileVisual>();

            // use the diamond mana as a preview to show what mana will look like with the palette color
            // maybe, in future this could show one of the player's equipped icons such as the first one. that may be kinda weird though
            tileVisual.SetVisualStandalone(this, Resources.Load<Sprite>("ManaIcons/diamond-mana"));
 
            return tileVisual.gameObject;
        }
    }
}