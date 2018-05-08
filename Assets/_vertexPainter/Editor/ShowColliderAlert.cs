using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ShowColliderAlert : EditorWindow {

    static void Init() {
	
 		ShowColliderAlert alertWindow = (ShowColliderAlert)EditorWindow.GetWindow (typeof (ShowColliderAlert),true,"Caution!",true); 
        alertWindow.position = new Rect(Screen.width/2,Screen.height/2, 500, 100);
        alertWindow.ShowPopup(); 
		
    }
    
    void OnGUI() {
	
	
	//	EditorGUIUtility.LookLikeInspector();	
	
		GUILayout.BeginVertical ("box");

		GUI.skin.label.alignment = TextAnchor.UpperCenter;
		
/* 		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.red;
		
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("-----------------------------> CAUTION! <-----------------------------", EditorStyles.boldLabel);
		EditorGUILayout.EndHorizontal (); */

		EditorGUILayout.Space();

		EditorGUILayout.Space();
		
		GUI.contentColor = Color.white; 
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("This will add a mesh collider to your object.");
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("The collider is needed to detect when the mouse is over your object.");
		EditorGUILayout.EndHorizontal ();


		EditorGUILayout.Space();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal ();

        if(GUILayout.Button("OK!" , GUILayout.Height(50))) {
		
            this.Close();
			
        }

		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
		
	}
    
}