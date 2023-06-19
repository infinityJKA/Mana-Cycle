using System;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

 [CreateAssetMenu(fileName = "Item", menuName = "ManaCycle/Item")]
public class Item : ScriptableObject
{  
    /// <summary>
    /// name of item shown in shop and player inventory
    /// </summary>
    public string itemName = "Unnamed Item";

    /// <summary>
    /// description of what the item does
    /// </summary>
    public string description = "This item does cool stuff!!!";

    /// <summary>
    /// small image shown in shop and inventory
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// how this item is used when it is interacted with from the inventory 
    /// </summary>
    public UseType useType;

    [Serializable]
    class Effect
    {
        /// <summary>
        /// what this item does when interacted with
        /// </summary>
        public EffectType type;

        /// <summary>
        /// value used by some effects, like hp recovery amount
        /// </summary>
        public float value;

        /// <summary>
        /// key used if the effect needs access to the stats dict
        /// </summary>
        public ArcadeStats.Stat key;
    }

    /// <summary>
    /// all of the effects this item has when used / equiped
    /// </summary>
    [SerializeField] List<Effect> effects;

    /// <summary>
    /// base cost to purchase in shop
    /// </summary>
    public int baseCost;

    /// <summary>
    /// cost after multipliers
    /// </summary>
    [NonSerialized] public int cost;

    public void OnEnable()
    {
        cost = baseCost;
        // Debug.Log(itemName + " " + cost);
    }

    /// <summary>
    /// cost increase after being purchased in the shop as a multiplier of base cost
    /// </summary>
    public float costIncreaseMult = 1f;

    public enum UseType
    {
        Consume, // lost when used, like a health potion
        Equip, // toggled on/off when used, given a free equip slot
        UseOnObtain, // imediate effect when obtained, for permanent uprades in arcade endless
    }

    public enum EffectType
    {
        IncreaseHpPercent, // increase player hp by percent of max hp (yiik reference)
        IncreaseHpFlat, // increase hp by flat amount
        IncreaseMaxHP, // for equipables / perm upgrades
        AddToStat, // adds to a stat on the stats dict with a given key
    }

    public string UseTypeToString()
    {
        switch (useType)
        {
            case UseType.Consume: return "Consumable";
            case UseType.Equip: return "Equipable"; 
            case UseType.UseOnObtain: return "Upgrade"; 
            default: return "-"; 
        }
    }

    public void ActivateEffect()
    {
        Debug.Log("used " + itemName);

        if (useType == UseType.Equip) 
        {
            if (!ArcadeStats.equipedItems.Contains(this))
            {
                // return early if trying to equip over max slots
                if (ArcadeStats.usedEquipSlots >= ArcadeStats.maxEquipSlots) return;
                // equiping the item
                ArcadeStats.equipedItems.Add(this);
                ArcadeStats.usedEquipSlots = ArcadeStats.equipedItems.Count;
            }
            else
            {
                // unequip the item, note only one copy of an item should be in list at a time
                ArcadeStats.equipedItems.Remove(this);
                UnequipEffect();
                ArcadeStats.usedEquipSlots = ArcadeStats.equipedItems.Count;
                return;
            }
            
        }

        foreach (Effect e in effects)
        {
            switch (e.type)
            {
                case EffectType.IncreaseHpPercent: GainHP((int) (ArcadeStats.maxHp * e.value)); break;
                case EffectType.IncreaseHpFlat: GainHP ((int) e.value); break;
                case EffectType.IncreaseMaxHP: ArcadeStats.maxHp += (int) e.value; GainHP(0); break;
                case EffectType.AddToStat: ArcadeStats.playerStats[e.key] += e.value; break;
                default: Debug.Log("Effect Type Not Handled! :("); break;
            }
        }

    }

    public void UnequipEffect()
    {

        foreach (Effect e in effects)
        {
            switch (e.type)
            {
                case EffectType.IncreaseMaxHP: ArcadeStats.maxHp -= (int) e.value; break;
                case EffectType.AddToStat: ArcadeStats.playerStats[e.key] -= e.value; break;
                default: Debug.Log("Unhandled Unequip"); break; // note some effects don't need to be handled here as they don't really make sense to be on an equipable
            }
        }

    }

    public void GainHP(int gain)
    {
        Storage.hp = Math.Min(Storage.hp + gain, ArcadeStats.maxHp);
    }

    #if (UNITY_EDITOR)
    [CustomPropertyDrawer(typeof(Effect))]
    public class EffectDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative("type");

            // make property taller if the stat key box needs to be drawn
            return typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.AddToStat)) ? 40 : 20;
        }

        // draw property in rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // declare rects
            Rect typeRect;
            Rect valRect;

            var typeProperty = property.FindPropertyRelative("type");
            // Debug.Log(typeProperty.enumNames[typeProperty.enumValueIndex]);
            if (typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.AddToStat)))
            {
                // when dict key is needed, show key enum selector in gui
                Rect statRect = new Rect(position.x, position.y + 20, position.width / 2, position.height / 2);
                typeRect = new Rect(position.x, position.y, position.width / 2, position.height / 2);
                valRect = new Rect(position.x * 2f, position.y + 20, 50, position.height / 2);

                // draw fields 
                EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);
                EditorGUI.PropertyField(valRect, property.FindPropertyRelative("value"), GUIContent.none);
                EditorGUI.PropertyField(statRect, property.FindPropertyRelative("key"), GUIContent.none);
            }
            else
            {
                typeRect = new Rect(position.x, position.y, position.width / 2, position.height);
                valRect = new Rect(position.x * 2f, position.y, 50, position.height);

                EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);
                EditorGUI.PropertyField(valRect, property.FindPropertyRelative("value"), GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
    #endif
}
