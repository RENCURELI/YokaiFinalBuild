using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StateMachine))]
public class StateMachinePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        StateMachine stateMachine = fieldInfo.GetValue(property.serializedObject.targetObject) as StateMachine;
        EditorGUI.indentLevel = 1;
        Rect contentPosition = EditorGUI.PrefixLabel(position, label, EditorStyles.boldLabel);
        if (position.height > 16f)
        {
            position.height = 16f;
            EditorGUI.indentLevel += 1;
            contentPosition = EditorGUI.IndentedRect(position);
            contentPosition.y += 18f;
        }
        if (GUI.Button(contentPosition, "Open Editor"))
        {
            StateMachineEditorWindow.Open(stateMachine);
        }
        //StateMachineEditorWindow.Open(null);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Screen.width < 333 ? (16f + 18f) : 16f;
    }
}