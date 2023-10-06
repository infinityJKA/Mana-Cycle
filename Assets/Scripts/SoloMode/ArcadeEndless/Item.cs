using System;
using System.Collections.Generic;
using UnityEngine;

using Sound;
using Battle.Board;

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

        /// <summary>
        /// when the effect is refered to (if at all)
        /// </summary>
        public DeferType deferType = DeferType.None;
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

    /// <summary>
    /// set in GameBoard's start method. used for mid-game effects
    /// </summary>
    [NonSerialized] public GameBoard board;

    // use / equip sound
    [SerializeField] private AudioClip useSFX;
    [SerializeField] private float soundPitch = 1f;
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
        TempAddToStat, // adds to a stat, then reverts it at the end of the match
        TakeDamageFlat, // decrease hp by flat amount
        DealDamageFlat, // deal damage to oppenent (mid-game only), to be implemented
    }

    public enum DeferType
    {
        None, // used like a regular item
        PostGame, // activated on post game screen
        OnCast, // to be implemented
        OnFullCycle, // u already kno
        OnDamageDealt, // when enemy board takes damage (not when added to damage cycle)
        OnDamageTaken, // when player board takes damage (not when added to damage cycle)
        OnSpecialUsed, // when active ability is used
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

    public static void Proc(List<Item> items, DeferType deferType = DeferType.None)
    {
        Debug.Log("procing with type " + deferType);
        foreach (Item i in items) i.ActivateEffect(deferType);
    }

    public void ActivateEffect(DeferType deferType = DeferType.None)
    {
        Debug.Log("used " + itemName);
        
        // none defer type means used in menu, so run equip / unequip logic
        if (deferType == DeferType.None)
        {
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
            else if (useType == UseType.Consume)
            {
                // play consumable sound serialized in this item. equip sounds are serialized in inventory
                SoundManager.Instance.PlaySound(useSFX, pitch: soundPitch);
            }
        }

        // apply effects
        foreach (Effect e in effects)
        {
            // skip if not correct defer type
            if (e.deferType != deferType)
            {
                Debug.Log("Incorrect defer type. Skipping!");
                continue;
            } 

            switch (e.type)
            {
                case EffectType.IncreaseHpPercent: GainHP((int) (ArcadeStats.maxHp * e.value)); break;
                case EffectType.IncreaseHpFlat: GainHP ((int) e.value); break;
                case EffectType.IncreaseMaxHP: ArcadeStats.maxHp += (int) e.value; GainHP(0); break;
                case EffectType.AddToStat: ArcadeStats.playerStats[e.key] += e.value; Debug.Log("bruh momentos"); break;
                case EffectType.TempAddToStat: ArcadeStats.playerStats[e.key] += e.value; ArcadeStats.deltaPlayerStats[e.key] -= e.value; break;
                case EffectType.TakeDamageFlat: board.TakeDamage((int) e.value); break;
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
        if (board != null) board.Heal(gain);
        else Storage.hp = Math.Min(Storage.hp + gain, ArcadeStats.maxHp);
    }

    #if (UNITY_EDITOR)
    [CustomPropertyDrawer(typeof(Effect))]
    public class EffectDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative("type");

            // make property taller if the stat key box needs to be drawn
            return (typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.AddToStat))) || typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.TempAddToStat)) ? 60 : 40;
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
            Rect deferRect;

            var typeProperty = property.FindPropertyRelative("type");
            // Debug.Log(typeProperty.enumNames[typeProperty.enumValueIndex]);
            if (typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.AddToStat)) || typeProperty.enumNames[typeProperty.enumValueIndex].Equals(nameof(EffectType.TempAddToStat)))
            {
                // when dict key is needed, show key enum selector in gui
                Rect statRect = new Rect(position.x, position.y + 20, position.width / 2, position.height / 3);
                typeRect = new Rect(position.x, position.y, position.width / 2, position.height / 3);
                valRect = new Rect(position.x * 2f, position.y + 20, 50, position.height / 3);
                deferRect = new Rect(position.x, position.y + 40, 150, position.height / 3);

                EditorGUI.PropertyField(statRect, property.FindPropertyRelative("key"), GUIContent.none);

            }
            else
            {
                typeRect = new Rect(position.x, position.y, position.width / 2, position.height / 2);
                valRect = new Rect(position.x * 2f, position.y, 50, position.height / 2);
                deferRect = new Rect(position.x, position.y + 20, 150, position.height / 2);

            }

            // draw fields 
            EditorGUI.PropertyField(valRect, property.FindPropertyRelative("value"), GUIContent.none);
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);
            EditorGUI.PropertyField(deferRect, property.FindPropertyRelative("deferType"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
    #endif
}
