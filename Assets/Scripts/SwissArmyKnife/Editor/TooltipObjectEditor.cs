/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;


using UnityEditor;


///=================================================================================================================
///                                                                                                       <summary>
///  TooltipObjectEditor is a MonoBehaviour that does important stuff. 											 </summary>
///
///=================================================================================================================
[CustomEditor(typeof(TooltipObject))]
public class TooltipObjectEditor : Editor 
{
    SerializedProperty _tooltipProperty;

    void OnEnable()
    {
        _tooltipProperty = serializedObject.FindProperty("_tooltip");
    }

    public override void OnInspectorGUI()
    {
        // Always do this at the beginning
        serializedObject.Update();

        EditorGUILayout.PropertyField(_tooltipProperty, new GUIContent("Tooltip"), GUILayout.MinHeight( 100.0f));

        serializedObject.ApplyModifiedProperties();
    }
}*/
