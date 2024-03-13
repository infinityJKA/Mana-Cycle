using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "InputPromptGlyphTable", menuName = "ManaCycle/Input Prompt Glyph Table", order = 0)]
public class InputPromptGlyphTable : ScriptableObject {
    [SerializeField] private InputGlyph[] glyphs;
    private Dictionary<InputBinding, InputGlyph> kvp;

    public InputGlyph GetGlyph(InputBinding binding) {
        if (kvp.ContainsKey(binding)) 
        {
            return kvp[binding];
        }
        else {
            for (int i = 0; i < glyphs.Length; i++) 
            {
                InputGlyph glyph = glyphs[i];
                if (glyph.binding == binding) {
                    kvp[binding] = glyph;
                    return glyph;
                }
            }
        }

        Debug.LogError("No glyph defined for keycode "+binding);
        return new InputGlyph();
    }
}

[Serializable]
public struct InputGlyph {
    public InputBinding binding;
    public Sprite sprite;
    public Sprite pressedSprite; // optional
}