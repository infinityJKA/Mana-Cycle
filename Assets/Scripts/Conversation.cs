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

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 16f * 2;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

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

        EditorGUI.EndProperty();
    }
}
