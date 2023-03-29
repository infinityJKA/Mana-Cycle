using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

[CreateAssetMenu(fileName = "Conversations", menuName = "ManaCycle/Conversation")]

public class Conversation : ScriptableObject {
    // [SerializeField] public string[] dialougeList;
    // [SerializeField] public Battler[] lSpeakerOrder;
    // [SerializeField] public Battler[] rSpeakerOrder;
    // [SerializeField] public bool[] leftFocused;

    [SerializeField] public ConversationLine[] dialogueList;
}

[Serializable]
public class ConversationLine {
    public string text;
    public Battler leftSpeaker;
    public Battler rightSpeaker;
    public bool rightFocused;
}

[CustomPropertyDrawer(typeof(ConversationLine))]
public class ConversationLineDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 42;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var text = property.FindPropertyRelative("text");
        var leftSpeaker = property.FindPropertyRelative("leftSpeaker");
        var rightSpeaker = property.FindPropertyRelative("rightSpeaker");
        var rightFocused = property.FindPropertyRelative("rightFocused");

        Rect drawRect = new Rect(position.x, position.y, position.width, 16);

        EditorGUI.PropertyField(drawRect, text, GUIContent.none);
        // dialogue.stringValue = EditorGUI.TextField(position, dialogue.stringValue);
        drawRect.y += 20;
        drawRect.width /= 2.5f;
        var divideWidth = drawRect.width;
        drawRect.width -= 4;

        // positionProperty.vector3Value = EditorGUI.Vector3Field(rect,"Left", positionProperty.vector3Value);
        EditorGUI.PropertyField(drawRect, leftSpeaker, GUIContent.none);
        drawRect.x += divideWidth;
        EditorGUI.PropertyField(drawRect, rightSpeaker, GUIContent.none);
        drawRect.x += divideWidth;
        rightFocused.boolValue = EditorGUI.ToggleLeft(drawRect, rightFocused.boolValue ? "Right Focus" : "Left Focus", rightFocused.boolValue);

        EditorGUI.EndProperty();
    }
}
