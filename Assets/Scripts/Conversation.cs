using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

[CreateAssetMenu(fileName = "Conversations", menuName = "ManaCycle/Conversation")]

public class Conversation : ScriptableObject {
    [SerializeField] public string[] dialougeList;
    [SerializeField] public Battler[] lSpeakerOrder;
    [SerializeField] public Battler[] rSpeakerOrder;
    [SerializeField] public bool[] leftFocused;
    [SerializeField] public string endScene;


    [SerializeField] public ConversationLine[] lines;

    [SerializeField] public ConversationLine epicLine;
}

[Serializable]
public class ConversationLine {
    public string dialogue;
    public Battler leftSpeaker;
    public Battler rightSpeaker;
    public bool leftFocused;
}

[CustomPropertyDrawer(typeof(ConversationLine))]
public class ConversationLineDrawer : PropertyDrawer
{
    // public override VisualElement CreatePropertyGUI(SerializedProperty property)
    // {
    //     // create property container element
    //     var container = new VisualElement();

    //     // get properties
    //     var dialogue = property.FindPropertyRelative("dialogue");
    //     var leftSpeaker = property.FindPropertyRelative("leftSpeaker");
    //     var rightSpeaker = property.FindPropertyRelative("rightSpeaker");
    //     var leftFocused = property.FindPropertyRelative("leftFocused");

    //     // create property fields
    //     container.Add(new PropertyField(dialogue));
    //     container.Add(new PropertyField(leftSpeaker));
    //     container.Add(new PropertyField(rightSpeaker));
    //     container.Add(new PropertyField(leftFocused));

    //     return container;
    // }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 16f * 2;
    }

    // public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    // {
    //     var dialogue = property.FindPropertyRelative("dialogue");
    //     var leftSpeaker = property.FindPropertyRelative("leftSpeaker");
    //     var rightSpeaker = property.FindPropertyRelative("rightSpeaker");
    //     var leftFocused = property.FindPropertyRelative("leftFocused");
        
    //     EditorGUIUtility.wideMode = true;
    //     EditorGUIUtility.labelWidth = 30;
    //     EditorGUI.indentLevel = 0;

    //     dialogue.stringValue = EditorGUI.TextField(rect, dialogue.stringValue);
    //     rect.y += rect.height;
    //     rect.width /= 3;
    //     // positionProperty.vector3Value = EditorGUI.Vector3Field(rect,"Left", positionProperty.vector3Value);
    //     EditorGUI.ObjectField(rect, leftSpeaker);
    //     rect.x += rect.width;
    //     EditorGUI.ObjectField(rect, rightSpeaker);
    //     rect.x += rect.width;
    //     EditorGUI.Toggle(rect, "leftFocus", leftFocused.boolValue);
    //     // normalProperty.vector3Value = EditorGUI.Vector3Field(rect, "Right", normalProperty.vector3Value);
    // }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        // position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        // var indent = EditorGUI.indentLevel;
        // EditorGUI.indentLevel = 0;

        // Calculate rects
        // var amountRect = new Rect(position.x, position.y, 30, position.height);
        // var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
        // var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        var dialogue = property.FindPropertyRelative("dialogue");
        var leftSpeaker = property.FindPropertyRelative("leftSpeaker");
        var rightSpeaker = property.FindPropertyRelative("rightSpeaker");
        var leftFocused = property.FindPropertyRelative("leftFocused");

        position.height /= 2;
        EditorGUI.PropertyField(position, dialogue, GUIContent.none);
        // dialogue.stringValue = EditorGUI.TextField(position, dialogue.stringValue);
        position.y += position.height;
        position.width /= 3;
        // positionProperty.vector3Value = EditorGUI.Vector3Field(rect,"Left", positionProperty.vector3Value);
        EditorGUI.PropertyField(position, leftSpeaker, GUIContent.none);
        position.x += position.width;
        EditorGUI.PropertyField(position, rightSpeaker, GUIContent.none);
        position.x += position.width;
        EditorGUI.PropertyField(position, leftFocused, GUIContent.none);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        // EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"), GUIContent.none);
        // EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
        // EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

        // Set indent back to what it was
        // EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
