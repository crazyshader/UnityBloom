using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlurPostProcess))]
public class BlurPostProcessEditor : Editor {

    SerializedProperty type, sigma;

    void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        sigma = serializedObject.FindProperty("sigma");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        if (type.enumValueIndex == 0)
            EditorGUILayout.Slider(sigma, 0.01f, 10,"Sigma",null);

        serializedObject.ApplyModifiedProperties();
    }
}
