using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRTK;

 [CustomEditor(typeof(FlightHandlerPhys))]
 [CanEditMultipleObjects]
 public class FlightHandlerPhysEditor : Editor
 {

    SerializedProperty minThrustForceThreshold;
    SerializedProperty flightDirection;
    SerializedProperty engageMode;
    SerializedProperty engageButton;
    SerializedProperty glideToggle;
    SerializedProperty glideSensitivity;
    SerializedProperty dragAcceleration;
    SerializedProperty glideDelay;
    SerializedProperty glideControllerMinDistance;


    void OnEnable()
    {
        // Setup the SerializedProperties.
        flightDirection = serializedObject.FindProperty("flightDirection");
        minThrustForceThreshold = serializedObject.FindProperty("minThrustForceThreshold");
        engageMode = serializedObject.FindProperty("engageMode");
        engageButton = serializedObject.FindProperty("engageButton");
        glideToggle = serializedObject.FindProperty("glideToggle");
        dragAcceleration = serializedObject.FindProperty("dragAcceleration");

        glideSensitivity = serializedObject.FindProperty("glideSensitivity");
        glideDelay = serializedObject.FindProperty("glideDelay");
        glideControllerMinDistance = serializedObject.FindProperty("glideControllerMinDistance");
    }
    public override void OnInspectorGUI()
   {
        DrawDefaultInspector();
         serializedObject.Update();
        var flightHandler = target as FlightHandlerPhys;

        EditorGUILayout.PropertyField( flightDirection, 
        new GUIContent("Flight Direction", "AlwaysUp " +
        "ControllerMovementAveraged = Average movement vector between the two controllers determine direction." +
        "ControllerRotationAveraged = Direction fully based on the rotation of the controllers. Controllers need to be placed in the correct hand for this to work."));

        EditorGUILayout.PropertyField(engageMode, 
            new GUIContent("Engage Mode", "Toggles whether a button is needed to activate flight or not"));
        if (engageMode.enumValueIndex == (int)FlightHandlerPhys.EngageMode.Toggle)
        {
            EditorGUILayout.PropertyField(engageButton, new GUIContent("Engage Button",
                 "Sets what button is needed to active flight."));
        }
                EditorGUILayout.PropertyField(glideControllerMinDistance, new GUIContent("Glide Controller Min Distance",
                    "The distance needed between controllers to trigger glide."));


        serializedObject.ApplyModifiedProperties();
    }
 }
