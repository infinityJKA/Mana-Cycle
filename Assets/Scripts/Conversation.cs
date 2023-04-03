using System;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

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
    [TextAreaAttribute]
    public string text;
    public Battler leftSpeaker;
    public Battler rightSpeaker;
    public bool rightFocused;
    public ConvoAnim leftAnim;
    public ConvoAnim rightAnim;
    [Tooltip("Toggles the typing effect.")]
    public bool instant;
}

[Serializable]
public enum ConvoAnim {
    [InspectorName("No Anim")]
    None,

    // fade in from direction or stationary
    In,
    InLeft,
    InRight,
    InUp,
    InDown,

    // fade out from direction or stationary
    Out,
    OutLeft,
    OutRight,
    OutUp,
    OutDown
}

#if (UNITY_EDITOR)
[CustomPropertyDrawer(typeof(ConversationLine))]
public class ConversationLineDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 100;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var text = property.FindPropertyRelative("text");
        var leftSpeaker = property.FindPropertyRelative("leftSpeaker");
        var rightSpeaker = property.FindPropertyRelative("rightSpeaker");
        var rightFocused = property.FindPropertyRelative("rightFocused");
        var leftAnim = property.FindPropertyRelative("leftAnim");
        var rightAnim = property.FindPropertyRelative("rightAnim");
        var instant = property.FindPropertyRelative("instant");

        Rect drawRect = new Rect(position.x, position.y, position.width, 64);

        EditorGUI.PropertyField(drawRect, text, GUIContent.none);
        // dialogue.stringValue = EditorGUI.TextField(position, dialogue.stringValue);

        drawRect.y += 68;
        drawRect.height /= 4;
        drawRect.width /= 3f;
        var divideWidth = drawRect.width;
        drawRect.width -= 4;
        // positionProperty.vector3Value = EditorGUI.Vector3Field(rect,"Left", positionProperty.vector3Value);
        EditorGUI.PropertyField(drawRect, leftSpeaker, GUIContent.none);
        drawRect.x += divideWidth;
        EditorGUI.PropertyField(drawRect, rightSpeaker, GUIContent.none);
        drawRect.x += divideWidth;
        rightFocused.boolValue = EditorGUI.ToggleLeft(drawRect, rightFocused.boolValue ? "Right Focus" : "Left Focus", rightFocused.boolValue);

        drawRect.x -= divideWidth*2;
        drawRect.y += 20;
        // positionProperty.vector3Value = EditorGUI.Vector3Field(rect,"Left", positionProperty.vector3Value);
        EditorGUI.PropertyField(drawRect, leftAnim, GUIContent.none);
        drawRect.x += divideWidth;
        EditorGUI.PropertyField(drawRect, rightAnim, GUIContent.none);
        drawRect.x += divideWidth;
        instant.boolValue = EditorGUI.ToggleLeft(drawRect, "Instant Text", instant.boolValue);

        EditorGUI.EndProperty();
    }
}
#endif