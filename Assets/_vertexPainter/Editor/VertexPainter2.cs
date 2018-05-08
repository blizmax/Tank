/*

 //////////////  Vertex Painter by Andrew Grant = reissgrant on Unity forums //////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////// support: argrant1977@gmail.com /////////////////////////////////
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VertexPainter2 : EditorWindow {

	static VertexPainter2 myWindow;

	private const String path = "assets/_VertexPainter/_vertexPaintedMeshes/";

	private MeshFilter lockedMeshFilter;
	private Mesh lockedMesh;
	private Mesh myNewMesh;	

	Shader vertexShader;
	Shader blendShader;

	Vector2 scrollPos;
			  
	int menu2int;
	int squareSize = 50; 
	int selectedTextureIndex = 0;
	int textureLimit;

	GameObject lockedObject;
						
	bool useBumpMaps;
	bool useSpecular;
	bool useMobile;
	bool useLighting;
	bool useBlendMap;
	bool useLightMaps;	  
	bool showAdvanced = false; 
	bool useCustom = false;
	bool shaderAlreadySet = false;
	bool increaseBrushSize = false;
	bool decreaseBrushSize = false;
	bool increaseStrength = false;
	bool decreaseStrength = false; 

	List<Texture> materialTextures;
	List<Texture> materialBumpMaps;
	List<bool> materialRandomize;

	enum mode { 
	
		mobile,
		desktop
	};

	static mode shaderMode = mode.desktop;
	
	GameObject pasteTo;
	Mesh copyFrom;
	
	private Vector3[] newVert;

	[SerializeField]		float strengthMin = .01f;
	[SerializeField]		float strengthMax = .25f;
	[SerializeField]		float radiusMin = .1f;

	[SerializeField]		float radiusMax = 50f;
	[SerializeField]		bool lockObject = false;
	[SerializeField]		bool paintVertexColors = false;
	[SerializeField]		bool applyRandom = false;
	[SerializeField]		bool applySolidColor = false;
	[SerializeField]		bool blendColors = false; 
	
	[SerializeField]		bool randomRed = true; 
	[SerializeField]		bool randomBlue = true;
	[SerializeField]		bool randomGreen = true;
	[SerializeField]		bool randomAlpha = true;
	
	[SerializeField]		float absoluteRed = 0f;
	[SerializeField]		float absoluteGreen = 0f;
	[SerializeField]		float absoluteBlue = 0f;
	[SerializeField]		float absoluteAlpha = 0f;
	 
	[SerializeField]		float radius = .1f;
	[SerializeField]		float strength = .1f;
	[SerializeField]		Color vertexColor = new Color(0f,1f,0f,0f);	  

	[MenuItem ("Vertex Painter/Vertex Painter v.2")]
	
	static void Init () {
	
		myWindow = (VertexPainter2)EditorWindow.GetWindow (typeof (VertexPainter2)); 
		myWindow.Show();
		myWindow.autoRepaintOnSceneChange  = true;  
		myWindow.resetLockedObject();

	}

	void OnGUI () {
	
		if(!myWindow)
			Init();
	
		checkMouse();
		
		if(myWindow)
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width (myWindow.position.width), GUILayout.Height (myWindow.position.height));
		

	paintVertexColors = true;

	string lockText = "";

	EditorGUILayout.BeginHorizontal();

	if (lockObject) {

		GUI.backgroundColor = Color.green; 

		lockText = "UNLOCK Object";

	} else {

		GUI.backgroundColor = Color.red; 
		lockText = "LOCK Object For Painting";

	}
													  
	if (GUILayout.Button(lockText,GUILayout.Height(25))) {

		if (lockObject) {

			lockObject = false;
			resetLockedObject();
			lockedObject = null;
			lockedObject = null;
			return;
					
		}

		if (Selection.activeGameObject) {

			if (Selection.activeGameObject.GetComponent<Renderer>()) {

				if (!Selection.activeGameObject.GetComponent("SkinnedMeshRenderer")) {

					shaderAlreadySet = false;

					lockedObject = Selection.activeGameObject;

					lockObject = true;

					if (lockedObject.GetComponent("MeshFilter")) { // is this a regular mesh or skinned mesh?
															
						lockedMesh = null;

						lockedMeshFilter = (MeshFilter)lockedObject.GetComponent("MeshFilter");
						lockedMesh = lockedMeshFilter.sharedMesh;
						checkForCollider();

						//check mode so we don't overwrite legacy meshes with old vertex blend shaders

					} 

					if (!useCustom) {

						shaderAlreadySet = false;
						setupShader();
						getShader();
						setPaintColor();

					}

					checkMesh();

				} else {

					Debug.Log("Skinned Mesh Vertex Color Painting is not currently supported.");

				}

			} else {

				Debug.Log("No Renderer Found, Are you selecting a parent object?");

			}

		} else {

			Debug.Log("No Object Selected!");

		}

	}	  /////IF LOCK BUTTON

	EditorGUILayout.EndHorizontal();

	squareSize = (int)((myWindow.position.width / 4) - 5);
	squareSize = Mathf.Clamp(squareSize, 60, 85);

	GUI.skin.label.alignment = TextAnchor.UpperCenter;
									  
	GUILayout.Space(5);
	EditorGUILayout.Separator();

	if (lockObject) {

		string hideText = "Hide";
		bool hidden = false;
		GUI.backgroundColor = Color.red;
		int ID = Selection.activeInstanceID;

		if (!(ID == lockedObject.GetInstanceID())) { 

			hideText = "Show";
			GUI.backgroundColor = Color.green;
			hidden = true;
		
		}

		if (GUILayout.Button(hideText + " Object Wireframe", GUILayout.Height(25))){
											
			if (lockedObject) {
																								 
				if (!hidden) {

					if (Camera.main) {

						Selection.activeGameObject = Camera.main.gameObject;

					} else if (GameObject.Find("_vpDummyObject")) {

						Selection.activeGameObject = GameObject.Find("_vpDummyObject");

					} else {

						GameObject go = new GameObject();
						go.name = "_vpDummyObject";
						Selection.activeGameObject = go;

					}

				} else {

					Selection.activeGameObject = lockedObject;

					if (GameObject.Find("_vpDummyObject")) {

						DestroyImmediate(GameObject.Find("_vpDummyObject"));

					}
				
				}

			}

		}

	}

	GUI.backgroundColor = Color.white;

	GUILayout.BeginVertical("box");

	GUILayout.Label("Brush Radius (SHIFT + Q,W)");

	radius = EditorGUILayout.Slider(radius, radiusMin, radiusMax); // expanded radius

	GUILayout.EndVertical();

	GUILayout.BeginVertical("box");

	GUILayout.Label("Brush Strength (SHIFT + A,S)");

	strength = EditorGUILayout.Slider(strength, strengthMin, strengthMax);

	GUILayout.EndVertical();

	GUILayout.Label("Press CTRL / CMD to Paint");

	GUILayout.Space(5);
	EditorGUILayout.Separator();

		if (materialTextures == null) {

			//add at least two for blending
			materialTextures = new List<Texture>();	 
			materialBumpMaps = new List<Texture>();	
			materialRandomize = new List<bool>();

			for (int i = 0; i < 2; ++i) {
							 
				materialTextures.Add(new Texture());
				materialBumpMaps.Add(new Texture()); 
				materialRandomize.Add(true);


			}

		}

		GUILayout.BeginVertical("box");

		useCustom = GUILayout.Toggle(useCustom, "Use Custom Shader ");

		GUILayout.EndVertical();

		if (!useCustom) {	  

		GUILayout.BeginVertical("box");

			#if UNITY_ANDROID || UNITY_IPHONE
				useMobile = true; 
				shaderMode = mode.mobile;
			#else
				useMobile = false;
				shaderMode = mode.desktop;
			#endif

			textureLimit = 4;  

			if (!useMobile) {	 

				useBlendMap = GUILayout.Toggle(useBlendMap, "Blend Map");

				if (useBlendMap) {

					useLighting = true;

				} else {

					useLighting = GUILayout.Toggle(useLighting, "Use Lights");
				
				}

				useBumpMaps = GUILayout.Toggle(useBumpMaps, "Bump Maps");

			} else {

				useBlendMap = false;

				useBumpMaps = GUILayout.Toggle(useBumpMaps, "Bump Maps (limit 2 on mobile)");

				if (useBumpMaps) { textureLimit = 2; }

			}

			if (useLighting) {  // using lighting


			} else {

				useBumpMaps = false;
				useSpecular = false;
				
			}
			   
			if (useBumpMaps && !useMobile) {

				useSpecular = GUILayout.Toggle(useSpecular, "Specular Maps");

			} else if(!useMobile){

				GUI.color = Color.gray;
				useSpecular = false;
				GUILayout.Toggle(useSpecular, "Specular Maps");

				GUI.color = Color.white;

			}

			GUILayout.EndVertical();

			GUILayout.BeginVertical("Box");
			GUILayout.Label("Choose Paint Mode");

			EditorGUILayout.BeginHorizontal();
			GUIContent[] paintModeOptions = new GUIContent[2];
			paintModeOptions[0] = new GUIContent("Paint Single");
			paintModeOptions[1] = new GUIContent("Paint Random");
			menu2int = GUILayout.Toolbar(menu2int, paintModeOptions, GUILayout.Height(25));
			EditorGUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUILayout.BeginVertical("Box");

			if (menu2int == 1) {

			} else {
					
				string range = "";

				for (int i = 0; i < materialTextures.Count; ++i) {

					range += (i+1).ToString();

					if (i < materialTextures.Count - 1) {

						range += ", ";

					}
			
				}

			}


			if (materialTextures.Count < textureLimit) {

				GUILayout.BeginHorizontal();

				if (GUILayout.Button("ADD TEXTURE", GUILayout.Height(squareSize / 2))) {	//, GUILayout.Height(squareSize)


					materialTextures.Add(new Texture());
					materialBumpMaps.Add(new Texture());
					materialRandomize.Add(true);

					shaderAlreadySet = false;
					getShader();

				}

				GUILayout.EndHorizontal();

			}


		for (int i = 0; i < materialTextures.Count; ++i ) {

			if (menu2int == 1) {  // in single mode

				if (materialRandomize[i]) {

					GUI.backgroundColor = Color.green;

				} else {

					GUI.backgroundColor = Color.grey;

				}

				GUILayout.Space(5);

			} else {	// in random mode

				if (i == selectedTextureIndex) {

					GUI.backgroundColor = Color.green;

				} else {

					GUI.backgroundColor = Color.grey;

				}

			}

			if (GUI.changed && lockedObject && !useCustom) {

				shaderAlreadySet = false;
				getShader();
				setPaintColor();
				if(lockedObject.GetComponent<MeshCollider>())lockedObject.GetComponent<MeshCollider>().enabled = false;
				if(lockedObject.GetComponent<MeshCollider>())lockedObject.GetComponent<MeshCollider>().enabled = true;

			}
											  
			GUILayout.BeginHorizontal("Box");

			EditorGUILayout.BeginVertical();


			GUILayout.Label("", GUILayout.Height(15), GUILayout.Width(75));

			if (menu2int == 0) {

				if (GUILayout.Button("Paint", GUILayout.Width(squareSize), GUILayout.Height(squareSize))) {

					selectedTextureIndex = i;
					setPaintColor();

				}

			} else {

				if (GUILayout.Button("Include", GUILayout.Width(squareSize), GUILayout.Height(squareSize))) {

					materialRandomize[i] = !materialRandomize[i];

				}

			}

			GUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			//texture label
			GUILayout.Label("Texture", GUILayout.Height(15), GUILayout.Width(squareSize));

			materialTextures[i] = (Texture)EditorGUILayout.ObjectField(materialTextures[i], typeof(Texture), true , GUILayout.Width(squareSize), GUILayout.Height(squareSize));
											   
			if (GUI.changed && lockedObject) {

				setShaderTexture();
						
			}

			GUILayout.EndVertical();

			EditorGUILayout.BeginVertical();

			if (useBumpMaps && materialBumpMaps != null) {

				//bumpmap label

				GUILayout.Label("Bump", GUILayout.Height(15), GUILayout.Width(squareSize));

				materialBumpMaps[i] = (Texture)EditorGUILayout.ObjectField(materialBumpMaps[i], typeof(Texture), true, GUILayout.Width(squareSize), GUILayout.Height(squareSize));

				if (GUI.changed) {

					setShaderTexture();

				}

			}

			GUILayout.EndVertical();

			EditorGUILayout.BeginVertical();

			GUI.backgroundColor = Color.red;

			if (materialTextures.Count > 2) {

				GUILayout.Label("", GUILayout.Height(15), GUILayout.Width(squareSize));

				if (GUILayout.Button("Remove", GUILayout.Height(squareSize), GUILayout.Width(squareSize))) {	//, GUILayout.Height(squareSize)

					materialTextures.RemoveAt(i);
					materialBumpMaps.RemoveAt(i);
					materialRandomize.RemoveAt(i);

					if (selectedTextureIndex > materialTextures.Count) {

						selectedTextureIndex = materialTextures.Count;

					}

					setShaderTexture();
					shaderAlreadySet = false;
					getShader();

				}

			}

			GUI.backgroundColor = Color.white;

			GUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

		}	// for material textures

		GUI.backgroundColor = Color.green;

		if (materialTextures.Count > textureLimit) {

			while (materialTextures.Count > textureLimit) {

				int num = materialTextures.Count - 1;

				materialTextures.RemoveAt(num);
				materialBumpMaps.RemoveAt(num);
				materialRandomize.RemoveAt(num);

			}
		
		}
		  
		GUI.backgroundColor = Color.white;

		if (menu2int == 1) {

			GUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.green;

			if (GUILayout.Button("Randomize!", GUILayout.Height(squareSize / 2))) {

				GUI.backgroundColor = Color.white;

				if (useMobile) {

					//generate using random mobile values

					if (lockedObject) { doRandom("Simple"); } else { Debug.Log("Try locking an object first!"); }

				} else {

					//generate using random desktop values

					if (lockedObject) { doRandom("Simple"); } else { Debug.Log("Try locking an object first!"); }

				}

			}

			GUILayout.EndHorizontal();

		}

		GUI.backgroundColor = Color.white;

		GUILayout.EndVertical();


		}	//if useCustom

		//////////////////Vertex Paint//////////////////

		showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Show Advanced Options");

		if (showAdvanced) {

			EditorGUI.indentLevel = 1;

			GUI.backgroundColor = Color.white;

			EditorGUILayout.Space();

			if (lockedObject) {

				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("  See Vertex Colors ",GUILayout.Height(25))) {

					if (lockedObject.GetComponent<Renderer>()) {

						if (lockedObject.GetComponent<Renderer>().sharedMaterial.shader.name != "vertexPainter/VertexColorsOnly") {

							lockedObject.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("vertexPainter/VertexColorsOnly");

						} else {

							shaderAlreadySet = false;
							setupShader();
							getShader();
							setPaintColor();
									
						}

					} 

				}	 // set vertex color shader	
	
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.Space();

			GUILayout.BeginVertical("box");

			vertexColor = EditorGUILayout.ColorField("Paint Color", vertexColor);

			EditorGUILayout.Space();

			GUILayout.EndVertical();

			GUILayout.BeginVertical("box");

			GUI.backgroundColor = Color.white;

			GUI.backgroundColor = Color.red;
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PrefixLabel("Red");
			GUI.backgroundColor = Color.white;
			randomRed = EditorGUILayout.Toggle(randomRed);
			if (!randomRed) { absoluteRed = EditorGUILayout.Slider(absoluteRed, 0f, 1f); }
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = Color.green;
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PrefixLabel("Green");
			GUI.backgroundColor = Color.white;
			randomGreen = EditorGUILayout.Toggle(randomGreen);
			if (!randomGreen) { absoluteGreen = EditorGUILayout.Slider(absoluteGreen, 0f, 1f); }
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = Color.blue;
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PrefixLabel("Blue");
			GUI.backgroundColor = Color.white;
			randomBlue = EditorGUILayout.Toggle(randomBlue);
			if (!randomBlue) { absoluteBlue = EditorGUILayout.Slider(absoluteBlue, 0f, 1f); }
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = Color.grey;
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.PrefixLabel("Alpha");
			GUI.backgroundColor = Color.white;
			randomAlpha = EditorGUILayout.Toggle(randomAlpha);
			if (!randomAlpha) { absoluteAlpha = EditorGUILayout.Slider(absoluteAlpha, 0f, 1f); }
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = Color.white;
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Blend Colors?");
			blendColors = EditorGUILayout.Toggle(blendColors);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			if (GUILayout.Button(" Apply Random Colors ", GUILayout.Height(25))) {

				applyRandom = true;

			}

			GUILayout.EndVertical();

			GUILayout.BeginVertical("box");

			copyFrom = EditorGUILayout.ObjectField("Copy Colors from:", copyFrom, typeof(Mesh), true) as Mesh;
			pasteTo = EditorGUILayout.ObjectField("Paste Colors to:", pasteTo, typeof(GameObject), true) as GameObject;
			if (GUILayout.Button("Copy Colors", GUILayout.Height(25))) {

				if (copyFrom && pasteTo) {

					copyColors();

				} else {

					Debug.Log("Please select two meshes to transfer colors between!");

				}

			}

			GUILayout.EndVertical();

		}

		if(myWindow){
		
			EditorGUILayout.EndScrollView(); 
			
		}
		
		this.Repaint();


		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		
	}

	[DrawGizmo (GizmoType.Selected)]
	
	static void RenderVisibleVertices (Transform obj, GizmoType gizmoType){
	
		if(myWindow){ // grab window instance

			myWindow.checkMouse(); 
				
			if(myWindow.lockObject){
			
				if(myWindow.paintVertexColors){

					Matrix4x4 lockedObjectMatrix = Matrix4x4.TRS(myWindow.lockedObject.transform.position, myWindow.lockedObject.transform.rotation, myWindow.lockedObject.transform.lossyScale);

					if (!myWindow.lockedObject) {
						
						Debug.Log("Cannot find a valid mesh, are you selecting a parent object?");
						myWindow.paintVertexColors = false;
						return;	
							
					}

					myWindow.checkForCollider();

					Vector3[] vertices = myWindow.lockedMesh.vertices;  ///////////////////////////////////////<---------------------
					Color[] colors = new Color[vertices.Length];
						
					Event e = Event.current;
					RaycastHit hit;
					Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

					if (Physics.Raycast(ray, out hit)) {

						Vector3 relativePoint = hit.point;

						if (myWindow.paintVertexColors) {  /////////////////////////Paint vertex colors here::

							/////////////////////////brush size
							if (myWindow.increaseBrushSize) {

								myWindow.Focus();

								myWindow.radius += .05f;

								myWindow.increaseBrushSize = false;

							}

							if (myWindow.decreaseBrushSize) {

								myWindow.Focus();

								myWindow.radius -= .05f;

								myWindow.decreaseBrushSize = false;

							}

							/////////////////////////brush strength
							if (myWindow.increaseStrength) {

								myWindow.Focus();

								myWindow.strength += .05f;

								myWindow.increaseStrength = false;

							}

							if (myWindow.decreaseStrength) {

								myWindow.Focus();

								myWindow.strength -= .05f;

								myWindow.decreaseStrength = false;

							}

							Color newColor2 = myWindow.vertexColor;
							newColor2.a = 1f;
							Handles.color = newColor2;

							Handles.DrawWireDisc(relativePoint, hit.normal, myWindow.radius);

							float strength = myWindow.strengthMin;
							float radius = myWindow.radiusMin;	
							int steps = 20;
							float strengthStep = (myWindow.strengthMax - myWindow.strengthMin) / steps;
							float radiusStep = .01f;

							for (int i = 0; i < steps; ++i) {

								if (myWindow.strength > strength) { Handles.DrawWireDisc(relativePoint, hit.normal, myWindow.radius + radius); }
								strength += strengthStep;
								radius += radiusStep;

							}

							if (e.control | e.command) {

								if (myWindow.paintVertexColors && myWindow.lockedMesh.colors.Length < 2) { myWindow.lockedMesh.colors = colors; }

								colors = myWindow.lockedMesh.colors;

								float sqrRadius = myWindow.radius * myWindow.radius;

								for (int i = 0; i < vertices.Length; i++) {

									vertices[i] = lockedObjectMatrix.MultiplyPoint(vertices[i]);
									float sqrMagnitude = (vertices[i] - relativePoint).sqrMagnitude;

									if (sqrMagnitude < sqrRadius) {

										float distance = Mathf.Sqrt(sqrMagnitude);

										float falloff = Mathf.Clamp01(Mathf.Pow(360.0f, -Mathf.Pow(distance / myWindow.radius, 2.5f) - 0.1f));

										float colorAdd = falloff * myWindow.strength;
																				
										if (myWindow.strength == .25) {

											colors[i] = myWindow.vertexColor;

										} else {

											colors[i] = Color.Lerp(colors[i], myWindow.vertexColor, colorAdd);

										}

									} //sqr mag

								} //for loop

								myWindow.lockedMesh.colors = colors;

							//	myWindow.saveMesh();

							} // control down?	

						} // paintVertexColors

					} // raycast hit...	

					if (myWindow.lockedMesh && (myWindow.applyRandom | myWindow.applySolidColor)) {

						myWindow.doRandom("Advanced");

					}
						
				} // if(myWindow.paintVertexColors)
					
			} //if lockObject...
			
			HandleUtility.Repaint(); 
			
		} // my window 
		
	} // render gizmo

	void doRandom(string mode) {

		Undo.SetSnapshotTarget(this, "Apply Random");
		Undo.CreateSnapshot();
		Undo.RegisterSnapshot();

		if (lockedMesh == null) {

			Debug.Log("No selected mesh!");
			return;
		
		}

		Vector3[] vertices = lockedMesh.vertices;

		Color[] colors = new Color[vertices.Length];
	
		vertices =  lockedMesh.vertices;

		if (mode == "Simple") {

			List<Color> colorPalette = new List<Color>();

			//desktop

			for (int i = 0; i < materialTextures.Count; ++i ) {

				if (materialRandomize[i]) {
					Color tempColor = new Color(0,0,0,0);
					tempColor[i] = 1;
					colorPalette.Add(tempColor);
				}
				
				
			}

			int numColors = colorPalette.Count;

			if (colorPalette.Count < 1) {
				
				Debug.Log("Select some textures first!");
				return;
				
			}

			for (int i = 0; i < vertices.Length; i++) {

				if (colorPalette.Count == 1) {

					colors[i] = colorPalette[0];

				} else {

					int randomColorIndex = Mathf.Abs(UnityEngine.Random.Range(0, numColors));

					colors[i] = colorPalette[randomColorIndex];

				}

			}


			lockedMesh.colors = colors;

		} else {

			for (int i = 0; i < vertices.Length; i++) {

				if (myWindow.applyRandom) {

					colors[i] = returnRandomColor();

				} // apply Random?

				if (myWindow.applySolidColor) {

					colors[i] = myWindow.vertexColor;

				} // apply solid?

			} // for vertices.Length

			if (myWindow.applyRandom || myWindow.applySolidColor) {

				lockedMesh.colors = colors;

				myWindow.applyRandom = false;

				myWindow.applySolidColor = false;

			}   // apply solid or random?

		}


		myWindow.saveMesh();

	}

	void checkForCollider() {

		if (!myWindow.lockedObject.GetComponent("MeshCollider")) { // add a collider if none...

			ShowColliderAlert alertWindow = (ShowColliderAlert)EditorWindow.GetWindow(typeof(ShowColliderAlert), true, "Caution!", true);
			alertWindow.position = new Rect(myWindow.position.x, myWindow.position.y + 200, 500, 150);				  
			MeshCollider newCollider = (MeshCollider)myWindow.lockedObject.AddComponent<MeshCollider>();

			newCollider.sharedMesh = myWindow.lockedMesh;

		}
	
	}

	private Color returnRandomColor(){

		if (shaderMode == mode.desktop) { };

		Color randomColor = new Color(0,0,0,0);

		if (myWindow.blendColors) {

			if (myWindow.randomRed)		{ randomColor.r = UnityEngine.Random.Range(0f, 1f); } else { randomColor.r = myWindow.absoluteRed;		}
			if (myWindow.randomGreen)	{ randomColor.g = UnityEngine.Random.Range(0f, 1f); } else { randomColor.g = myWindow.absoluteGreen;	}
			if (myWindow.randomBlue)	{ randomColor.b = UnityEngine.Random.Range(0f, 1f); } else { randomColor.b = myWindow.absoluteBlue;		}
			if (myWindow.randomAlpha)	{ randomColor.a = UnityEngine.Random.Range(0f, 1f); } else { randomColor.a = myWindow.absoluteAlpha;	}

		} else {

			int numRandoms = 0;
			int index = 0;

			if (myWindow.randomRed)		{ numRandoms++; } else	{ randomColor.r = myWindow.absoluteRed;		}
			if (myWindow.randomGreen)	{ numRandoms++; } else	{ randomColor.g = myWindow.absoluteGreen;	}
			if (myWindow.randomBlue)	{ numRandoms++; } else	{ randomColor.b  = myWindow.absoluteBlue;	}
			if (myWindow.randomAlpha)	{ numRandoms++; } else	{ randomColor.a = myWindow.absoluteAlpha;	}


			int randomColorInt = Mathf.Abs(UnityEngine.Random.Range(0, numRandoms));

			int[] randomArray = new int[] { 0, 0, 0, 0 };

			randomArray[randomColorInt] = 1;

			if (myWindow.randomRed)		{ randomColor.r = randomArray[index]; index++; }
			if (myWindow.randomGreen)	{ randomColor.g = randomArray[index]; index++; }
			if (myWindow.randomBlue)	{ randomColor.b = randomArray[index]; index++; }
			if (myWindow.randomAlpha)	{ randomColor.a = randomArray[index]; index++; }

		}

		return randomColor;

	}
	
	static void OnDestroy(){
	
		if(myWindow){ // grab window instance
		
			myWindow.paintVertexColors = false;
			myWindow.applyRandom = false;
			myWindow.applySolidColor = false; 
			
			myWindow.resetLockedObject();
		
		}
		
	} // OnDestroy()

	private void resetColors() {

		MeshFilter mFilter = (MeshFilter)lockedObject.GetComponent("MeshFilter");
		Mesh sMesh = mFilter.sharedMesh;

		Vector3[] vertices = sMesh.vertices;
		Color[] colors = new Color[vertices.Length];

		vertices = sMesh.vertices;

		for (int i = 0; i < vertices.Length; i++) {

			colors[i] = new Color(1,0,0,1);


		}

		sMesh.colors = colors;
	
	}
	
	private void colorsTo(bool toMobile) {
											  
		//foreach mesh in scene

		Debug.Log("Setting Colors");

		Material selectedMaterial = lockedObject.GetComponent<Renderer>().sharedMaterial;

		List<GameObject> gameObjects = new List<GameObject>();

		//gameObjects.Add(lockedObject);

		object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));

		  foreach (GameObject o in obj)
		  {

			  if (o.GetComponent<Renderer>() == null) continue;

			  Material[] mats = o.GetComponent<Renderer>().sharedMaterials;

			  foreach(Material mat in mats){
			  
				  if(mat == selectedMaterial) {
				  
					gameObjects.Add(o);
				  
				  }
			  
			  }

		  }
		

		foreach(GameObject go in gameObjects){

			MeshFilter mFilter = (MeshFilter)go.GetComponent("MeshFilter");
			Mesh sMesh = mFilter.sharedMesh;

			Vector3[] vertices = sMesh.vertices;  
			Color[] colors = new Color[vertices.Length];

			vertices = sMesh.vertices;

			int texCount = materialTextures.Count;

			for (int i = 0; i < vertices.Length; i++) {

				Color oldColor = sMesh.colors[i];
				Color newColor = new Color();

				if (toMobile) {

					if (texCount < 2) {

						if (oldColor.r > .5) {

							newColor.a = 1;

						} else if (oldColor.g > .5) {

							newColor.a = 0;
						
						}

					} else {

						if (oldColor.r > .5) {

							newColor.a = 1;

						}

						if (oldColor.g > .5) {

							newColor.a = 0;

						}

						if (oldColor.b > .5) {

							newColor = new Color(1, 1, 1, 1);

						}
					
					}

				} else {

					if (texCount < 2) {

						if (oldColor.a > .5) {

							newColor.r = 1;

						}

						if (oldColor.a < .5) {

							newColor.g = 1;

						}

						newColor.a = 1;

					} else {

						if (oldColor.a > .5) {

							newColor.r = 1;

						}

						if (oldColor.a < .5) {

							newColor.g = 1;

						} 

						if (oldColor == Color.white) {

							newColor = new Color(0, 0, 1, 1);

						}

					}

				
				}

				colors[i] = newColor;

			}


			sMesh.colors = colors;

		}

	}

	private void setPaintColor() {

		if (!lockedObject) {

			return;

		}

		switch (selectedTextureIndex) {

			case 0: vertexColor = new Color(1f, 0f, 0f, 0f); break;

			case 1: vertexColor = new Color(0f, 1f, 0f, 0f); break;

			case 2: vertexColor = new Color(0f, 0f, 1f, 0f); break;

			case 3: vertexColor = new Color(0f, 0f, 0f, 1f); break;

		}

	}

	private void setShaderTexture() {
			
		if (!lockedObject) return;

		for (int i = 0; i < materialTextures.Count; ++i) {

			lockedObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex" + (i + 1), materialTextures[i]);

			if (useBumpMaps) {

				lockedObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_BumpMap" + (i + 1), materialBumpMaps[i]);


			}
		
		
		}

													   	
	}

	private void setupShader() {
	
		//call this when first locking an object  to make sure we're not overwriting the shader, and grabbing the right amount of textures.

		//get the shader and find out what the textures are

		string shaderName = "";

		if (lockedObject.GetComponent<Renderer>().sharedMaterial) {

			shaderName = lockedObject.GetComponent<Renderer>().sharedMaterial.shader.name;

		}

		int count = 2;

		if (shaderName.Contains("VertexBlend") || shaderName.Contains("vertexBlend") || shaderName.Contains("vB")) {

			useLightMaps = false;

			if (useLightMaps) { }

			if (shaderName.Contains("Normal")) {

				useBumpMaps = true;

			} else {

				useBumpMaps = false;

			}

			if (shaderName.Contains("unLit")) {

				useLighting = false;

			} else {

				useLighting = true;

			}

			if (shaderName.Contains("Spec")) {

				useSpecular = true;

			} else {

				useSpecular = false;

			}

			if (shaderName.Contains("3tex")) {

				count = 3;

			} else if (shaderName.Contains("4tex")) {

				count = 4;

			} 

		} 

	//	Debug.Log("Count is: " + count);

		materialTextures.Clear();
		materialBumpMaps.Clear();
		materialRandomize.Clear();

		for (int i = 0; i < count; ++i) {

			materialTextures.Add(new Texture());

			materialBumpMaps.Add(new Texture());

			materialRandomize.Add(true);

		}

		doTextureSlots();

	}

	private void getShader() {

		if (shaderAlreadySet) {

			Debug.Log("Shader already set!");
			return;
		
		}

		if (!lockedObject) {

			return;
		
		}

		string shaderName = "";

		if (lockedObject.GetComponent<Renderer>().sharedMaterial) {

			shaderName = lockedObject.GetComponent<Renderer>().sharedMaterial.shader.name;

		} else {

			Material theMaterial = new Material(Shader.Find("Diffuse"));
			lockedObject.GetComponent<Renderer>().sharedMaterial = theMaterial;
			shaderName = "Diffuse";
		
		}

		//get shader and slots and populate editor window with them

		if(shaderName.Contains("vertexPainter")){

			addUpdateShader(shaderName , true);

		} else {

			//add a shader
			addUpdateShader(shaderName , false);
		
		}
	
	}

	private void addUpdateShader(string shaderType, bool isExisting) {

		Material theMaterial = lockedObject.GetComponent<Renderer>().sharedMaterial;
	
		if (!isExisting && !useCustom) {

			//we need to add a custom shader

			if (theMaterial.name == "Default-Diffuse") {
																	  
				lockedObject.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
														 
				//set material to the new materials
				theMaterial = lockedObject.GetComponent<Renderer>().sharedMaterial;
				theMaterial.name = lockedObject.GetInstanceID().ToString();

			}

		}

		string newShader = "vertexPainter/vB_";

		if (useBlendMap) newShader += "map_";
		newShader += "Diffuse";		 
		if (!useLighting) newShader += "_unLit";
		if (useBumpMaps) newShader += "Normal";
		if (useSpecular) newShader += "Spec";
		newShader += "_" + materialTextures.Count.ToString();
		newShader += "tex";

		lockedObject.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find(newShader);
													   
		doTextureSlots();

		shaderAlreadySet = true;

	}
															  
	private void doTextureSlots() {
											   
		//got shader, now populate textures slots  

		//we will always have at least two main textures.

		if (!lockedObject.GetComponent<Renderer>().sharedMaterial) {

			return;

		}

		if(!lockedObject.GetComponent<Renderer>().sharedMaterial.HasProperty("_MainTex1") || !lockedObject.GetComponent<Renderer>().sharedMaterial.HasProperty("_MainTex2")){

			return;
		
		}

		for (int i = 0; i < materialTextures.Count; ++i) {

			materialTextures[i] = lockedObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex" + (i+1));

			if (useBumpMaps) {

				materialBumpMaps[i] = lockedObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap" + (i + 1));

			}
		
		
		}
	
	}

	private void checkMouse() {

		Event e = Event.current;

		bool changed = false;

		if (e.keyCode == KeyCode.Alpha1) {

			selectedTextureIndex = 0;
			changed = true;

		}


		if (e.keyCode == KeyCode.Alpha2) {

			selectedTextureIndex = 1;
			changed = true;

		}

		if (e.keyCode == KeyCode.Alpha3) {
 
			if (materialTextures.Count > 2) {

				selectedTextureIndex = 2;
				changed = true;

			}

		}

		if (e.keyCode == KeyCode.Alpha4) {

			if (materialTextures.Count > 3) {

				selectedTextureIndex = 3;
				changed = true;

			}

		}

		if (changed) {

			shaderAlreadySet = false;
			getShader();
			setPaintColor();
			lockedObject.GetComponent<MeshCollider>().enabled = false;
			lockedObject.GetComponent<MeshCollider>().enabled = true;

		}
		
		if (e.button == 0 && e.isMouse){
			
			Undo.SetSnapshotTarget(this, "Changed Value");
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
			
		} //  if (e.button == 0 && e.isMouse) 
		
		if(myWindow.paintVertexColors){
		
			if (e.shift && lockObject){  

				if (e.keyCode == KeyCode.Q){
				 
					Undo.SetSnapshotTarget(this, "Decrease Brush Size");
					Undo.CreateSnapshot();
					Undo.RegisterSnapshot();
					
					decreaseBrushSize = true;	
					
					//////////////////////////////////////////

				}

				if (e.keyCode == KeyCode.W){	
				
					Undo.SetSnapshotTarget(this, "Increased Brush Size");
					Undo.CreateSnapshot();
					Undo.RegisterSnapshot();

					increaseBrushSize  = true;	

				}	
				
				if (e.keyCode == KeyCode.A){
				
					Undo.SetSnapshotTarget(this, "Decreased Brush Strength");
					Undo.CreateSnapshot();
					Undo.RegisterSnapshot();

					decreaseStrength = true;	

				}

				if (e.keyCode == KeyCode.S){	
					
					Undo.SetSnapshotTarget(this, "Increased Brush Strength");
					Undo.CreateSnapshot();
					Undo.RegisterSnapshot();			

					increaseStrength = true;		

				}	
			
				myWindow.Focus();

			} // shift held down?
		
		} //paint vertex colors or weights?
		
	} // checkMouse()

	private void saveMesh() { 
							
		if (	!Directory.Exists(path) )	{	Directory.CreateDirectory(path);		}
							
		if (	Directory.Exists(path) )	{
							
			string[] words = path.Split('/');
								
			Mesh clonedMesh = new Mesh();

			clonedMesh.vertices = myWindow.lockedMesh.vertices;
			clonedMesh.colors = myWindow.lockedMesh.colors;
			clonedMesh.uv = myWindow.lockedMesh.uv;
			clonedMesh.uv2 = myWindow.lockedMesh.uv2;
			clonedMesh.normals = myWindow.lockedMesh.normals;
			clonedMesh.tangents = myWindow.lockedMesh.tangents;
			clonedMesh.triangles = myWindow.lockedMesh.triangles;

			if (myWindow.lockedMesh.boneWeights.Length > 1) {

				clonedMesh.boneWeights = myWindow.lockedMesh.boneWeights;
									
			}

			if (myWindow.lockedMesh.bindposes.Length > 1) {

				clonedMesh.bindposes = myWindow.lockedMesh.bindposes;
									
			}
															 								
			String meshFileName = "";
								
			for(int i = 0 ; i < words.Length - 1 ; i++){
								
				meshFileName += words[ i ] + "/";
								
			} // for loop

			meshFileName += myWindow.lockedObject.GetInstanceID() + "_vertexPainted.asset";
								
			AssetDatabase.DeleteAsset (meshFileName);
								
			AssetDatabase.CreateAsset(	clonedMesh, meshFileName	);

			if (myWindow.lockedMeshFilter) {

				myWindow.lockedMeshFilter.sharedMesh = clonedMesh;
				myWindow.lockedMesh = clonedMesh;
				if(myWindow.lockedMeshFilter.GetComponent<MeshCollider>())myWindow.lockedMeshFilter.GetComponent<MeshCollider>().sharedMesh = clonedMesh;	 
									
			}
							
		} else {
							
			Debug.Log("Can't save mesh file, please ensure there is a folder named \"VertexPainter\\_vertexPaintedMeshes\" in your assets foler.");

		} // directory exists?

	}

	private void checkMesh() {

		if (!lockedMesh)
			return;
													
		if(lockedMeshFilter){

			if (lockedMesh.name == lockedObject.name) {

				if (lockedObject.GetComponent<Collider>() != null) {

					resetColors();
					saveMesh();

				}

			} 
					
		} 
	
	}

	private void resetLockedObject() {
	
		lockObject = false;
		lockedObject = null;
	
	}

	private void copyColors() {
		
		MeshFilter tempMF = (MeshFilter)pasteTo.GetComponent("MeshFilter") as MeshFilter;
		Color[] tempColors = tempMF.sharedMesh.colors;
		
		for(int i = 0 ; i < tempColors.Length ; i++){
			
			tempColors[i] = copyFrom.colors[i];
			
		}
		
		tempMF.sharedMesh.colors = tempColors;
		
		pasteTo = null;
		copyFrom = null;
		
	}
	
} // class vertex Painter definition