﻿using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using ws.winx.editor;
using System.IO;
using ws.winx.editor.utilities;
using ws.winx.editor.extensions;
using ws.winx.unity.utilities;
using UnityEditor.Animations;
using ws.winx.unity;
using ws.winx.unity.sequence;

namespace ws.winx.editor.windows
{





		/// <summary>
		/// Sequence editor window.
	/// !!! IMPORTANT
	/// Can't mix vidoe sound and audio=> crazyness happening
	/// Can't stop all the channels with sounds
	/// 
		/// </summary>
		public class SequenceEditorWindow : EditorWindow
		{

				
				private static Sequence __sequence;
				private static bool __isPlayMode = false;
				private static bool __needSave = false;
				private Sequence.SequenceWrap wrap = Sequence.SequenceWrap.ClampForever;
				private static GameObject __sequenceGameObject;
				private Vector2 settingsScroll;
				private bool dragNode;
				private float timeClickOffset;
				private bool resizeNodeStart;
				private bool resizeNodeEnd;
				private bool stop;
				//private bool playForward;
				//private float time;
				private static TimeAreaW __timeAreaW;
				private static SequenceNode __testNode;
				private static ReorderableList __sequenceChannelsReordableList;
				//private static bool __isPlaying;
				private static bool __isRecording;
				private static int __frameRate = 30;
				private const float NODE_RECT_HEIGHT = 40f;
				//20f for timer ruller + 20f for Events Pad
				private const float TIME_LABEL_HEIGHT = 20f;
				private const float EVENT_PAD_HEIGHT = 20f;
				private static GUIContent __frameRateGUIContent = new GUIContent ("fps:");
				private static GUIContent __nodeLabelGUIContent = new GUIContent ();
		
				private static SequenceNode __nodeSelected {
						get {
								if (__sequence != null) {
										return __sequence.selectedNode;
								}
								return null;
						}
						set {
								__sequence.selectedNode = value;
						}
				}




				[MenuItem("Window/Sequence", false)]
				public static void ShowWindow ()
				{
						SequenceEditorWindow window = EditorWindow.GetWindow<SequenceEditorWindow> (false, "Sequence");

						

						if (__sequence != null) {
								__sequence = Selection.activeGameObject.GetComponent<Sequence> ();
								__sequenceGameObject = Selection.activeGameObject;

								
						
								if(__timeAreaW!=null)
								__timeAreaW.hTicks.SetTickModulosForFrameRate (__sequence.frameRate);
							
						} else {
								selectionChangeEventHandler ();

						}

						

						window.wantsMouseMove = true;
						UnityEngine.Object.DontDestroyOnLoad (window);


	
				}

				private void OnEnable ()
				{




						if (__timeAreaW == null) {
								__timeAreaW = new TimeAreaW (false);

								
								__timeAreaW.hSlider = true;
								__timeAreaW.vSlider = false;
								__timeAreaW.vRangeLocked = true;
								__timeAreaW.hRangeMin = 0f;
								//__timeAreaW.hRangeMax=3f;

								__timeAreaW.margin = 0f;
								
								__timeAreaW.scaleWithWindow = true;
							
								//__timeAreaW.ignoreScrollWheelUntilClicked = false;
								if(__sequence!=null)
									__timeAreaW.hTicks.SetTickModulosForFrameRate (__sequence.frameRate);
								else
									__timeAreaW.hTicks.SetTickModulosForFrameRate (30);

								this.Repaint ();
			
								
			
						}

						if (__sequenceChannelsReordableList == null) {

								__sequenceChannelsReordableList = new ReorderableList (new List<SequenceNode> (), typeof(SequenceNode), true, false, false, false);
								__sequenceChannelsReordableList.elementHeight = NODE_RECT_HEIGHT;
								__sequenceChannelsReordableList.headerHeight = 2f;
							
								__sequenceChannelsReordableList.drawElementCallback = onDrawSequenceChannelElement;
							
								__sequenceChannelsReordableList.onSelectCallback = onSelectSequenceChannelCallback;
								__sequenceChannelsReordableList.onReorderCallback = onReorderSequenceChannelCallback;
					
					
						}
				
						
			
						EditorApplication.playmodeStateChanged -= OnPlayModeStateChange;
						EditorApplication.playmodeStateChanged += OnPlayModeStateChange;

						SceneView.onSceneGUIDelegate -= OnSceneGUI;
						SceneView.onSceneGUIDelegate += OnSceneGUI;
				}

				/// <summary>
				/// Handles the scene GUI event.
				/// </summary>
				/// <param name="sceneView">Scene view.</param>
				private static void OnSceneGUI (SceneView sceneView)
				{

						if (__sequence != null && __sequence.timeCurrent > 0f && Selection.activeGameObject != null && __sequence.channels.Exists (itm => itm.type == SequenceChannel.SequenceChannelType.Animation && itm.target == Selection.activeGameObject)) {//TODO should be target of some channel too
								if (!__sequence.isPlaying && !__sequence.isRecording && (Tools.current == Tool.Move || Tools.current == Tool.Rotate)
				   
										&& currentlyDragging
				    ) {
					
										__sequence.timeCurrent = 0f;
										ResetChannelsTarget ();
								}
						}


		


//			if (clipBindingsSerialized.value == null)
//				return;
//			
//			EditorClipBinding[] clipBindings = clipBindingsSerialized.value as EditorClipBinding[];
//			int bindingsLength = clipBindings.Length;
//			Color clr = Color.red;
//			Vector3[] positionsInKeyframe = null;
//			AnimationClip clip = null;
//			
//			EditorClipBinding clipBindingCurrent = null;
//			
//			long indexListAndindexFramePacked = 0;
//			
//			Transform transformRoot = null;
//			
//			
//			Transform transformFirstChild = null;
//			
//			bool gameObjectCharacterSelected = false;
//			
//			if (Selection.activeGameObject != null) {
//				if (Tools.current == Tool.Move) {
//					
//					
//					if (Selection.activeGameObject == __spaceGameObject) {
//						gameObjectCharacterSelected = true;
//						
//					} else {
//						
//						clipBindingCurrent = clipBindings.FirstOrDefault ((itm) => itm.gameObject == Selection.activeGameObject);
//						// && clipBindingCurrent.gameObject.transform.position!=Tools.handlePosition
//						if (clipBindingCurrent != null) {
//							
//							//clipBindingCurrent.gameObject.position has changed so recalcualte position offset
//							clipBindingCurrent.positionOffset = Quaternion.Inverse (__spaceGameObject.transform.rotation) * (clipBindingCurrent.gameObject.transform.position - __spaceGameObject.transform.position);
//							
//							
//							
//						}
//					}
//					
//				} else if (Tools.current == Tool.Rotate) {
//					
//					if (Selection.activeGameObject == __spaceGameObject) {
//						gameObjectCharacterSelected = true;
//						
//					} else {
//						
//						clipBindingCurrent = clipBindings.FirstOrDefault ((itm) => itm.gameObject == Selection.activeGameObject);
//						// && clipBindingCurrent.gameObject.transform.position!=Tools.handleRotation
//						if (clipBindingCurrent != null) {
//							
//							
//							
//							//clipBindingCurrent.gameObject.rotation has changed so recalcualte rotation offset
//							clipBindingCurrent.rotationOffset = Quaternion.Inverse (__spaceGameObject.transform.rotation) * clipBindingCurrent.gameObject.transform.rotation;
//							
//						}
//					}
//					
//				}
//			}
//			
//			
//			for (int i=0; i<bindingsLength; i++) {
//				
//				clipBindingCurrent = clipBindings [i];
//				
//				if (clipBindingCurrent.visible) {
//					
//					
//					clip = clipBindingCurrent.clip;
//					
//					if (clip != null && clipBindingCurrent.gameObject != null && clipBindingCurrent.gameObject.transform.childCount>0 && (transformFirstChild = clipBindingCurrent.gameObject.transform.GetChild (0)) != null) {
//						
//						
//						if (gameObjectCharacterSelected) {//=> move and/or rotate clip binding's gameobjects
//							GameObject gameObjectBinded = clipBindingCurrent.gameObject;
//							gameObjectBinded.transform.rotation = __spaceGameObject.transform.rotation * clipBindingCurrent.rotationOffset;
//							gameObjectBinded.transform.position = __spaceGameObject.transform.position + __spaceGameObject.transform.rotation * clipBindingCurrent.positionOffset;
//							
//							
//						}
//						
//						
//						
//						transformRoot = clipBindingCurrent.gameObject.transform;
//						
//						
//						
//						positionsInKeyframe = AnimationUtilityEx.GetPositions (clip, AnimationUtility.CalculateTransformPath (transformFirstChild, transformRoot), transformRoot);
//						
//						
//						
//						
//						if (positionsInKeyframe != null) {			
//							for (int j=0; j<positionsInKeyframe.Length; j++) {
//								
//								indexListAndindexFramePacked = i;//put index of clipBinding in list/array
//								indexListAndindexFramePacked = indexListAndindexFramePacked << 32 | (uint)j;//pack together with keyframe index
//								
//								//actualy this is static handle
//								HandlesEx.DragHandle (positionsInKeyframe [j], 0.1f, Handles.SphereCap, Color.red, "(" + j + ")" + " " + Decimal.Round (Convert.ToDecimal (AnimationUtilityEx.GetTimeAt (clip, j, AnimationUtilityEx.EditorCurveBinding_PosX)), 2).ToString (), indexListAndindexFramePacked, onDragHandleEvent, new GUIContent[]{new GUIContent ("Delete")}, new long[]{j}, null);
//								
//								
//								
//							}
//							//save color
//							clr = Handles.color;
//							
//							
//							clipBindingCurrent.color.a = 1;
//							Handles.color = clipBindingCurrent.color;
//							
//							
//							Handles.DrawPolyLine (positionsInKeyframe);
//							//restore color
//							Handles.color = clr;
//						}
//						
//						
//					}
//				}
//			}
//			
//			
//			//Debug.Log ("hot"+GUIUtility.hotControl);
//			
//			SceneView.RepaintAll ();
				}

		
				//				private static void DoTransition (SequenceNode node, Rect rect, int channelOrd)
				//				{
				//					
				//						Color color = GUI.color;
				//						GUI.color = Color.red;
				//						float transitionPosStart = __timeAreaW.TimeToPixel (node.startTime + node.duration - node.transition, rect);
				//						float transitionPosEnd = __timeAreaW.TimeToPixel (node.startTime + node.duration, rect);
//						Rect boxRect = new Rect (transitionPosStart, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT, transitionPosEnd - transitionPosStart, NODE_RECT_HEIGHT);
//				
//						GUI.Box (boxRect, "", "TL LogicBar 0");
//						GUI.color = color;
//					
//				}



				private static bool currentlyDragging {
						get {
								return GUIUtility.hotControl != 0;
						}
				}
		
		
				/// <summary>
				/// Dos the node.
				/// </summary>
				/// <param name="node">Node.</param>
				/// <param name="rect">Rect.</param>
				/// <param name="channelOrd">Channel ord.</param>
				private static void DoNode (SequenceNode node, Rect rect, int channelOrd)
				{
						

						EditorGUIUtility.AddCursorRect (new Rect (__timeAreaW.TimeToPixel (node.startTime, rect) - 5, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT, 10, NODE_RECT_HEIGHT), MouseCursor.ResizeHorizontal);			
						EditorGUIUtility.AddCursorRect (new Rect (__timeAreaW.TimeToPixel (node.startTime + node.duration, rect) - 5, TIME_LABEL_HEIGHT + channelOrd * NODE_RECT_HEIGHT, 10, NODE_RECT_HEIGHT), MouseCursor.ResizeHorizontal);

			
			
						float startTimePos = __timeAreaW.TimeToPixel (node.startTime + node.transition, rect);

						float endTimePos = __timeAreaW.TimeToPixel (node.startTime + node.duration, rect);

						float startTransitionTimePos = 0f;
						SequenceNode nodePrev = null;

						//if there is next node with transition
//						if (node.index + 1< node.channel.nodes.Count && (nodePrev = node.channel.nodes [node.index - 1]).transition > 0) {
//								
//								startTimePos = __timeAreaW.TimeToPixel (node.startTime + nodePrev.transition, rect);
//								
//						}


						//Draw Transtion rect
						if (node.transition > 0) {
								

								Color colorSave = GUI.color;
								GUI.color = Color.red;

								startTransitionTimePos = __timeAreaW.TimeToPixel (node.startTime, rect);
								
								Rect transitionRect = new Rect (startTransitionTimePos, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT, startTimePos - startTransitionTimePos, NODE_RECT_HEIGHT);
				
								GUI.Box (transitionRect, "", "TL LogicBar 0");

								GUI.color = colorSave;

								

						}

						Rect nodeRect = new Rect (startTimePos, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT, endTimePos - startTimePos, NODE_RECT_HEIGHT);

						GUI.Box (nodeRect, "", "TL LogicBar 0");

						nodeRect.xMin += 5;
						nodeRect.xMax -= 5;
						EditorGUIUtility.AddCursorRect (nodeRect, MouseCursor.Pan);

			
						GUIStyle style = new GUIStyle ("Label");

						style.fontSize = (__nodeSelected == node ? 12 : style.fontSize);
						style.fontStyle = (__nodeSelected == node ? FontStyle.Bold : FontStyle.Normal);
						Color color = style.normal.textColor;
						color.a = (__nodeSelected == node ? 1.0f : 0.7f);
						style.normal.textColor = color;
						
						//calc draw name of the node
						__nodeLabelGUIContent.text = node.name;

						Vector3 size = style.CalcSize (__nodeLabelGUIContent);
						
							
						int labelLength = node.name.Length;

						//if labelWidth is greater then node box rect => remove chars
						while (nodeRect.width < size.x && labelLength>0) {
									
								__nodeLabelGUIContent.text = node.name.Substring (0, labelLength);
								size = style.CalcSize (__nodeLabelGUIContent);
								labelLength--;
						}

						if (labelLength > 0) {
								Rect rect1 = new Rect (nodeRect.x + nodeRect.width * 0.5f - size.x * 0.5f, nodeRect.y + nodeRect.height * 0.5f - size.y * 0.5f, size.x, size.y);
								GUI.Label (rect1, __nodeLabelGUIContent, style);
						}
			

			
				}



				///////////////////// HANDLERS OF CHANNEL LIST ////////////////////


				/// <summary>
				/// Ons the reorder sequence channel callback.
				/// </summary>
				/// <param name="list">List.</param>
				static void onReorderSequenceChannelCallback (ReorderableList list)
				{
						//	Debug.Log (list.index);

//						SequenceChannel channel = null;
//						int channelsNumber = __sequence.channels.Count;
//
//						for (int i=0; i<channelsNumber; i++) {
//								channel = __sequence.channels [i];
//								channel.nodes.ForEach (itm => itm.channelOrd = i);
//						}

						
						
				}
		
			
		
		
			
		
		
				/// <summary>
				/// Handles draw list element.
				/// </summary>
				/// <param name="rect">Rect.</param>
				/// <param name="index">Index.</param>
				/// <param name="isActive">If set to <c>true</c> is active.</param>
				/// <param name="isFocused">If set to <c>true</c> is focused.</param>
				private static void onDrawSequenceChannelElement (Rect rect, int index, bool isActive, bool isFocused)
				{
						SequenceChannel channel = __sequenceChannelsReordableList.list [index] as SequenceChannel;
						Rect temp = rect;
						

		
						

						if (channel.target != null) {
								temp.yMin = rect.yMax - 16f;
								temp.yMax = rect.yMax;

								EditorGUI.LabelField (temp, channel.target.name, EditorStyles.miniLabel);

						}
						int fontSize = 16;
						temp.yMin = rect.yMin + (rect.height - fontSize) * 0.5f;
						temp.yMax = rect.yMax;// temp.yMin + fontSize;

						GUIStyle styleGUI = new GUIStyle (EditorStyles.label);
						styleGUI.fontSize = fontSize;
						
						channel.name = EditorGUI.TextField (temp, channel.name, styleGUI);



			
				}

				
		
			
		
				/// <summary>
				/// Handles the select list item event.
				/// </summary>
				/// <param name="list">List.</param>
				static void onSelectSequenceChannelCallback (ReorderableList list)
				{

						//Debug.Log ("Select " + list.index);
//			GameObject rootGameObject = (list.list as EditorClipBinding[]) [list.index].gameObject;
//			GameObject rootFirstChildGameObject;
//			
//			if (rootGameObject != null && rootGameObject.transform.childCount>0 && (rootFirstChildGameObject = rootGameObject.transform.GetChild (0).gameObject) != null) {
//				Selection.activeGameObject = rootFirstChildGameObject;
//				
//				
//				EditorWindow.GetWindow<SearchableEditorWindow> ().Focus ();
//			}
				}
		
				/// /////////////////////////////////////////////////////////////////////


				private void OnAddEvent ()
				{
						EditorUtility.DisplayDialog ("Not implemented!", "Events are not availible in this version.", 
			                            "Close");			
				}


				/// <summary>
				/// Handles the editor to  play mode state change event.
				/// </summary>
				private void OnPlayModeStateChange ()
				{
						if (!__isPlayMode && EditorApplication.isPlaying) {

								///TODO check this if not blocking playmode
				 

				
								__isPlayMode = true;
				 
						
						} else {
								__isPlayMode = false;
					
						}


						Debug.Log ("Play mode:" + __isPlayMode);
				}


				/// <summary>
				/// Postprocesses the animation recording and property modifications.
				/// </summary>
				/// <returns>The animation recording modifications.</returns>
				/// <param name="modifications">Modifications.</param>
				private static UndoPropertyModification[] PostprocessAnimationRecordingModifications (UndoPropertyModification[] modifications)
				{

						SequenceNode node = __sequence.selectedNode;

						if (node != null && node.channel.target != null && node.channel.type == SequenceChannel.SequenceChannelType.Animation) {

								modifications.Concat (AnimationModeUtility.Process (node.channel.target, node.source as AnimationClip, modifications, (float)(__sequence.timeCurrent - node.startTime)));


						}
					
					
						return modifications;
				}



				/// <summary>
				/// Update this instance.
				/// </summary>
				private void Update ()
				{



						if (__sequenceGameObject != null) {
								__sequence = __sequenceGameObject.GetComponent<Sequence> ();
						}

		
								if (!EditorApplication.isPlaying && __sequence != null && __sequence.isPlaying && (float)__sequence.timeCurrent<__sequence.duration) {//??? enters here even isPlaying is false
								
								__sequence.UpdateSequence (EditorApplication.timeSinceStartup);


								Debug.Log((float)__sequence.timeCurrent+"__"+__sequence.duration+" "+__sequence.isPlaying+" "+__sequence.name);

								SampleClipNodesAt (__sequence.timeCurrent);

								this.Repaint ();

								//this ensure update of MovieTexture (it its bottle neck do some reflection and force render call)
								//GetWindow<SceneView>().Repaint();
								SceneView.RepaintAll ();

								//AudioUtilW.UpdateAudio();
						}

				}

				private static void ResetChannelsTarget ()
				{
						PrefabType prefabType = PrefabType.None;
					
						foreach (SequenceChannel channel in __sequence.channels) {
								if (channel.type == SequenceChannel.SequenceChannelType.Animation && channel.target != null) {
										prefabType = PrefabUtility.GetPrefabType (channel.target);
							
										if (prefabType == PrefabType.ModelPrefabInstance || prefabType == PrefabType.PrefabInstance)
												channel.target.ResetPropertyModification<Transform> ();
										//PrefabUtility.RevertPrefabInstance (channel.target);
							
							
							
							
										//rewind to start position and rotation (before Animation sampling)
										channel.target.transform.position = channel.positionOriginalRoot;
										channel.target.transform.rotation = channel.rotationOriginalRoot;

								}
						}
				}

				private static void Stop ()
				{
						if (__sequence != null) {
								__sequence.Stop (__sequence.playForward);
						
								__sequence.StopRecording ();
								StopAllVideo ();
						}

						Undo.postprocessModifications -= PostprocessAnimationRecordingModifications;
						AnimationMode.StopAnimationMode ();

						AudioUtilW.StopAllClips ();
					
				
					

				}

				private static void StopAllVideo ()
				{
						double timeCurrent = __sequence.timeCurrent;
						foreach (SequenceChannel ch in __sequence.channels) 
								if (ch.type == SequenceChannel.SequenceChannelType.Video) {
										ch.nodes.ForEach (itm => {

												if (timeCurrent > itm.startTime && timeCurrent < itm.startTime + itm.duration) 
														(itm.source as MovieTexture).Pause ();
												else
														(itm.source as MovieTexture).Stop ();
						
										});
								}
				}

				private static void OnRecord ()
				{
						if (__sequence.isRecording) {//STOP RECORD
								Stop ();

								ResetChannelsTarget ();

						} else {//START RECORD

								__sequence.Record ();

								
						
						
								if (!AnimationMode.InAnimationMode ()) {
							
										AnimationMode.StartAnimationMode ();
										Undo.postprocessModifications += PostprocessAnimationRecordingModifications;

										SaveAnimationData ();
								} else if (__needSave) {
										SaveAnimationData ();

								}
				         
								AudioUtilW.StopAllClips ();

								SampleClipNodesAt (__sequence.timeCurrent);

								
						}
				}

				private void OnPlay ()
				{


			
						if (!__sequence.isPlaying) {//PLAY
								
								
								Undo.postprocessModifications -= PostprocessAnimationRecordingModifications;

								__sequence.SequenceNodeStart -= onSequenceNodeStart;
								__sequence.SequenceNodeStart += onSequenceNodeStart;
//
								__sequence.SequenceNodeStop -= onSequenceNodeStop;
								__sequence.SequenceNodeStop += onSequenceNodeStop;

								__sequence.OnEnd.RemoveListener (onSequenceEnd);
								__sequence.OnEnd.AddListener (onSequenceEnd);

				
				
								if ((float)__sequence.timeCurrent >= __sequence.duration) {//start from begining
										__sequence.timeCurrent = 0f;
										//ResetChannelsTarget ();
								}else
									__sequence.timeCurrent=Mathf.Min((float)__sequence.timeCurrent,__sequence.duration);
				
							
				
								if (!AnimationMode.InAnimationMode ()) {

									
										AnimationMode.StartAnimationMode ();


										SaveAnimationData ();

										
								} else if (__needSave) {

										SaveAnimationData ();

								}
				         
				         
				         
								
								__sequence.Play (EditorApplication.timeSinceStartup);

								
				
				
				
				
						} else {//STOP

								
								__sequence.Stop (__sequence.playForward);


								//Stop ();


//								if (__sequence != null) {
//									__sequence.Stop (__sequence.playForward);
//									
//									__sequence.StopRecording ();
//									
//								}

								StopAllVideo ();
								
								Undo.postprocessModifications -= PostprocessAnimationRecordingModifications;
								//AnimationMode.StopAnimationMode ();
								
								AudioUtilW.StopAllClips ();
				
				
				
				
				__sequence.SequenceNodeStart -= onSequenceNodeStart;
			
				
								__sequence.SequenceNodeStop -= onSequenceNodeStop;
						}

						

				}

			

			

			


				/// <summary>
				/// Handles OnGUI event
				/// </summary>
				private void OnGUI ()
				{
						
						

						Rect rect = this.position;



						rect.x = this.position.width * 0.2f;
						rect.width = this.position.width * 0.8f;//80% for timeArea

						rect.y = 0;

						
						if(__sequence!=null)
							OnTimelineGUI (rect, __sequence.frameRate);
						else
							OnTimelineGUI (rect, 30);

						rect.width = rect.x;//20% for settings
						rect.x = 0;
						rect.y = 0;
			
						DoChannelsGUI (rect);

						
				}



				/// <summary>
				/// Sequences the drop channel target event handler.
				/// </summary>
				/// <param name="rect">Rect.</param>
				/// <param name="channel">Channel.</param>
				static void sequenceDropChannelTargetEventHandler (Rect rect, int channelInx)
				{
						if (__sequence.isPlaying || __sequence.isRecording)
								return;


						Event evt = Event.current;


						
						switch (evt.type) {
						case EventType.DragUpdated:
						case EventType.DragPerform:
								if (!rect.Contains (evt.mousePosition))
										return;
								DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
							
								Type draggedType;
							
								if (evt.type == EventType.DragPerform) {
										DragAndDrop.AcceptDrag ();
										if (EditorApplication.isPlaying) {
												Debug.Log ("Can't add tween object in play mode. Stop the play mode and readd it.");
										} else {
												GameObject target = DragAndDrop.objectReferences [0] as GameObject;

												if (target != null) {
														SequenceChannel channel=null;
														channel=__sequence.channels [channelInx];
														channel.target = target;
														channel.positionOriginalRoot=target.transform.position;
														channel.rotationOriginalRoot=target.transform.rotation;
												}
										}




								}
								break;
						}
				}

				/// <summary>
				/// Dos the toolbar GUI.
				/// </summary>
				/// <param name="rect">Rect.</param>
				void DoChannelsGUI (Rect rect)
				{
						GUIStyle style = new GUIStyle ("ProgressBarBack");
						style.padding = new RectOffset (0, 0, 0, 0);
						
						GUILayout.BeginArea (rect, GUIContent.none, style);

						if (__sequence != null) {
								GUILayout.BeginHorizontal (EditorStyles.toolbar);

							
								///DRAW PLAY,RECORD AND ADD EVENT BUTTON
								Color colorSave = GUI.backgroundColor;


								GUI.backgroundColor = __sequence.isRecording ? Color.red : Color.white;

								//Record
								if (GUILayout.Button (EditorGUIUtility.FindTexture ("d_Animation.Record"), EditorStyles.toolbarButton)) {
									
										OnRecord ();

								}

								GUI.backgroundColor = colorSave;

								//Play
								if (GUILayout.Button (__sequence.isPlaying ? EditorGUIUtility.FindTexture ("d_PlayButton On") : EditorGUIUtility.FindTexture ("d_PlayButton"), EditorStyles.toolbarButton)) {
									

									
										OnPlay ();

								}
								GUILayout.FlexibleSpace ();
								if (GUILayout.Button (EditorGUIUtility.FindTexture ("d_Animation.AddEvent"), EditorStyles.toolbarButton)) {
										//				if(onAddEvent!= null){
										//					onAddEvent();
										//				}
								}
								GUILayout.EndHorizontal ();
						}
			
						GUILayout.BeginHorizontal ();
						GUILayout.BeginVertical ();




						OnSettingsGUI (rect.width - 1.5f);


						if (__sequence != null) {

								Rect rectLastControl = GUILayoutUtility.GetLastRect ();

								rect.yMin = rectLastControl.yMax;

								//Draw Channels
								__sequenceChannelsReordableList.list = __sequence.channels;
					
								__sequenceChannelsReordableList.DoLayoutList ();


								rectLastControl = GUILayoutUtility.GetLastRect ();
								rect.yMax = rectLastControl.yMax - 16f;//16f for +/- buttons

								if(__sequence.channels.Count>0)
								sequenceDropChannelTargetEventHandler (rect, (int)((Event.current.mousePosition.y - rect.y) / NODE_RECT_HEIGHT));

						}

						GUILayout.EndVertical ();
						GUILayout.Space (1.5f);
						GUILayout.EndHorizontal ();



						


						GUILayout.EndArea ();
			
				}


				/// <summary>
				/// Handles drawing settings OnGUI .
				/// </summary>
				/// <param name="width">Width.</param>
				private void OnSettingsGUI (float width)
				{
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button (__sequence != null ? __sequence.name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width (width * 0.3f))) {
								GenericMenu toolsMenu = new GenericMenu ();
				
								List<Sequence> sequences = GameObjectUtilityEx.FindAllContainComponentOfType<Sequence> ();
								foreach (Sequence sequence in sequences) {
										toolsMenu.AddItem (new GUIContent (sequence.name), false, OnGameObjectSelectionChanged, sequence);
								}
								toolsMenu.AddItem (new GUIContent ("[New Sequence]"), false, CreateNewSequence);
								toolsMenu.AddItem (new GUIContent ("[New Animation Clip]"), false, CreateNewAnimationClip);
								toolsMenu.AddItem (new GUIContent ("[New Animation Controller]"), false, CreateRuntimeAnimatorController);


								

								toolsMenu.DropDown (GUILayoutUtility.GetLastRect ());
								//toolsMenu.DropDown (new Rect (3, 37, 0, 0));
								EditorGUIUtility.ExitGUI ();
						}
			
						if (__sequence != null) {
								wrap = __sequence.wrap;			
						}
						wrap = (Sequence.SequenceWrap)EditorGUILayout.EnumPopup (wrap, EditorStyles.toolbarDropDown, GUILayout.Width (width * 0.4f));
						if (__sequence != null) {
								__sequence.wrap = wrap;			
						}

						if (__sequence != null) {
								//Handle FRAMERATE input and changes		
								EditorGUI.BeginChangeCheck ();

								EditorGUILayout.LabelField (__frameRateGUIContent, GUILayout.Width (width * 0.1f));
								__sequence.frameRate = Mathf.Max (EditorGUILayout.IntField (__sequence.frameRate, GUILayout.Width (width * 0.2f)), 1);
			
								if (EditorGUI.EndChangeCheck ()) {

										//TODO check this
										foreach (SequenceChannel channel in __sequence.channels)
												foreach (SequenceNode n in channel.nodes) {
										
														if (n.source is AnimationClip) {
									
												
																(n.source as AnimationClip).frameRate = __sequence.frameRate;
														} 

														//resnap to new framerate
														//n.duration = TimeAreaW.SnapTimeToWholeFPS (n.duration, __sequence.frameRate);

					
												}

										//now tick drawn on new frame rate
										__timeAreaW.hTicks.SetTickModulosForFrameRate (__sequence.frameRate);

								}
						}
			

						GUILayout.EndHorizontal ();






				}

				private void OnEventGUI (Rect rect)
				{
				
				}


				/// <summary>
				/// Ons the timeline click.
				/// </summary>
				/// <param name="rect">Rect.</param>
				private static void onTimelineClick (Rect rect)
				{

						double time = __timeAreaW.PixelToTime (Event.current.mousePosition.x, rect);

//						if (time > __sequence.duration || time < 0)
//								return;


						__sequence.timeCurrent = time;

					
					
						__sequence.Record ();


						if (!AnimationMode.InAnimationMode ()) {
								AnimationMode.StartAnimationMode ();
								Undo.postprocessModifications += PostprocessAnimationRecordingModifications;
								
								
								SaveAnimationData ();
				
								//SceneView.RepaintAll();

								SceneView.currentDrawingSceneView.Repaint ();
								
								Debug.Log ("Animation mode:" + AnimationMode.InAnimationMode ());
								
								
						} else if (__needSave)
								SaveAnimationData ();
			
						SampleClipNodesAt (__sequence.timeCurrent);

						//stop all movietextures
						
						AudioUtilW.StopAllClips ();

				}


				/// <summary>
				/// Saves the AnimationData from nodes's Animations.
				/// AnimationData contains target.rootBone.position offset before animation applied and and at animation t=0s
				/// also contains data of channel.target position and rotation
				/// </summary>
				public static void SaveAnimationData ()
				{
						Transform rootBoneTransform;

					
						
						__needSave = false;   

					//	AnimationMode.BeginSampling ();
			
						//Save binding(gameobject-animationclip) status 
						foreach (SequenceChannel channel in __sequence.channels) {


				
								if (channel.target != null && channel.type == SequenceChannel.SequenceChannelType.Animation && (rootBoneTransform = channel.target.GetRootBone ()) != null) {
					
										//save bone position
										Vector3 positionPrev = Vector3.zero;

										Animator animator = channel.target.GetComponent<Animator> ();

										//must have controller or sampling doesn't work(weird???)
										animator.runtimeAnimatorController = channel.runtimeAnimatorController;


										float timePointer = 0f;



										//check first node if exist and if no change was done to channel.target => no need to resave
										if (channel.positionOriginalRoot == channel.target.transform.position
					   							 && channel.rotationOriginalRoot == channel.target.transform.rotation)
												continue;

										Debug.Log ("SaveAnimationData> resaving...");

										//save target position and rotation so can be restored after stoping animation mode
										channel.positionOriginalRoot = channel.target.transform.position;
										channel.rotationOriginalRoot = channel.target.transform.rotation;
					
					
										foreach (SequenceNode n in channel.nodes) {

												
							
												if (timePointer > 0) {
							
							
														//get sample of animation clip of previous node at the end
														AnimationMode.SampleAnimationClip (channel.target, channel.nodes [n.index - 1].source as AnimationClip, timePointer);


														positionPrev = rootBoneTransform.position + n.clipBinding.boneRootPositionOffset;
												} else {
														positionPrev = rootBoneTransform.position;
												}
						
					
						
												//get sample of animation clip of current node at start
												AnimationMode.SampleAnimationClip (channel.target, (AnimationClip)n.source, 0f);


						
												Vector3 positionAfter = rootBoneTransform.transform.position;
						
												//calculate difference of bone position orginal - bone postion after clip effect
												n.clipBinding.boneRootPositionOffset = positionPrev - positionAfter;
						
												timePointer = n.duration;


											
						
										}
								}
						}



					//	AnimationMode.EndSampling ();
				}



//		public Texture DoRenderPreview (Rect previewRect, GUIStyle background)
//		{
//			this.m_PreviewUtility.BeginPreview (previewRect, background);
//			Vector3 bodyPosition = this.bodyPosition;
//			Quaternion quaternion;
//			Vector3 vector;
//			Quaternion quaternion2;
//			Vector3 pivotPos;
//			if (this.Animator && this.Animator.isHuman)
//			{
//				quaternion = this.Animator.rootRotation;
//				vector = this.Animator.rootPosition;
//				quaternion2 = this.Animator.bodyRotation;
//				pivotPos = this.Animator.pivotPosition;
//			}
//			else
//			{
//				if (this.Animator && this.Animator.hasRootMotion)
//				{
//					quaternion = this.Animator.rootRotation;
//					vector = this.Animator.rootPosition;
//					quaternion2 = Quaternion.identity;
//					pivotPos = Vector3.zero;
//				}
//				else
//				{
//					quaternion = Quaternion.identity;
//					vector = Vector3.zero;
//					quaternion2 = Quaternion.identity;
//					pivotPos = Vector3.zero;
//				}
//			}
//			bool oldFog = this.SetupPreviewLightingAndFx ();
//			Vector3 forward = quaternion2 * Vector3.forward;
//			forward [1] = 0f;
//			Quaternion directionRot = Quaternion.LookRotation (forward);
//			Vector3 directionPos = vector;
//			Quaternion pivotRot = quaternion;



				/// <summary>
				/// Samples the clip nodes at time.
				/// </summary>
				/// <param name="time">Time.</param>
				private static void SampleClipNodesAt (double time)
				{


						Transform rootBoneTransform;
					
					
						SequenceNode node = null;
						Animator animator;


						Undo.FlushUndoRecordObjects ();

						AnimationMode.BeginSampling ();
			
						//find changels of type Animation and find first node in channel that is in time or nearest node left of time
						foreach (SequenceChannel channel in __sequence.channels) {
								if (channel.target != null) {


										//node = channel.nodes.FirstOrDefault (itm => time - itm.startTime >= 0 && time <= itm.startTime + itm.duration);

										node = null;


										foreach (SequenceNode n in channel.nodes) {
												if (time - n.startTime >= 0) {
														if (time <= n.startTime + n.duration) {
																node = n;
																break;
														} else { 
																if (channel.type == SequenceChannel.SequenceChannelType.Animation && time > n.startTime + n.duration) {
																		if (n.index + 1 < channel.nodes.Count && time - channel.nodes [n.index + 1].startTime >= 0) {
																				continue;
																		} else {
																				node = n;
																				time = n.startTime + n.duration;
																				break;
																		}
																}

														}
												}

										}


										if (node != null) {
												if (channel.type == SequenceChannel.SequenceChannelType.Animation) {
											
											
													

														animator = channel.target.GetComponent<Animator> ();

														if (animator == null) {
																animator = channel.target.AddComponent<Animator> ();
														}

														animator.runtimeAnimatorController = channel.runtimeAnimatorController;
									
													

														AnimationMode.SampleAnimationClip (channel.target, node.source as AnimationClip, (float)(time - node.startTime));
						
							///!!! It happen that Unity change the ID of channel or something happen and channal as key not to be found



														//						Correction is needed for root bones as Animation Mode doesn't respect current GameObject transform position,rotation
														//						=> shifting current boneTransform position as result of clip animation, to offset of orginal position before animation(alerady saved)
														if ((rootBoneTransform = channel.target.GetRootBone ()) != null)
																rootBoneTransform.position = rootBoneTransform.position + node.clipBinding.boneRootPositionOffset;
						
													
											
								
												} else if (channel.type == SequenceChannel.SequenceChannelType.Audio) {
										
														AudioClip audioClip = node.source as AudioClip;

														
														int sampleStart = (int)Math.Ceiling (audioClip.samples * ((__sequence.timeCurrent - node.startTime) / audioClip.length));
												     
														AudioUtilW.SetClipSamplePosition (audioClip, sampleStart);
														
										
												} else if (channel.type == SequenceChannel.SequenceChannelType.Video) {
														///!!!I don't know the way of  Sampling or timeshifting of MovieTexture 
							
							
												}
										}
								}
						}


						
						AnimationMode.EndSampling ();

//						//Debug.Log ("2:"+ani.bodyRotation.eulerAngles.ToString ()+" "+ani.rootRotation.eulerAngles.ToString());
//						AnimationModeUtility.SampleClipBindingAt (targets, clips, times);
//
//
//
//						
//						int targetsNum = targets.Count;
//						Transform rootBoneTransform;
//						for (int i=0; i<targetsNum; i++) {
//
//
//								if ((rootBoneTransform = targets [i].GetRootBone ()) != null)
//										rootBoneTransform.position = rootBoneTransform.position + nodes [i].boneOrginalPositionOffset;
//						}
		
				
						
				


					

				}



	


				/// <summary>
				/// Raises the timeline GU event.
				/// </summary>
				/// <param name="rect">Rect.</param>
				/// <param name="frameRate">Frame rate.</param>
				private void OnTimelineGUI (Rect rect, float frameRate)
				{
						Handles.color = new Color (0.5f, 0.5f, 0.5f, 0.2f);
					
						if (__sequence != null) {
								//Draw horizontal lines (rows)
								int rows = __sequence.channels.Count + 1;
								int y = 0;
								float lineY = 0f;
								for (; y<rows; y++) {
								
										lineY = TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + y * NODE_RECT_HEIGHT;
										Handles.DrawLine (new Vector3 (rect.xMin, lineY, 0), new Vector3 (rect.xMax, lineY, 0));	
								
								
								}

										

								//make all area from end of label and Event pad to botton droppable
								sequenceDropSourceEventHandler (new Rect (rect.x, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT, rect.width, rect.height - TIME_LABEL_HEIGHT - EVENT_PAD_HEIGHT), Math.Min ((int)((Event.current.mousePosition.y - rect.y - TIME_LABEL_HEIGHT - EVENT_PAD_HEIGHT) / NODE_RECT_HEIGHT), __sequence.channels.Count));
			
								Handles.color = Color.white;

						}

						//TimeArea
						if(__sequence!=null)
							__timeAreaW.DoTimeArea (rect, __sequence.frameRate);
						else
							__timeAreaW.DoTimeArea (rect, 3);
					
						if (Event.current.type == EventType.Repaint) 
								EditorGUILayoutEx.ANIMATION_STYLES.eventBackground.Draw (new Rect (rect.x, EVENT_PAD_HEIGHT, rect.width, EVENT_PAD_HEIGHT), GUIContent.none, 0);


						if (__sequence != null) {
								//DRAW NODES 
								int channelNumber = __sequence.channels.Count;
								SequenceChannel channel = null;
								for (int i=0; i<channelNumber; i++) {
										channel = __sequence.channels [i];
										foreach (SequenceNode node in channel.nodes) {
									
										

												sequenceNodeClickEventHandler (node, rect, i);
												DoNode (node, rect, i);

										
										}
								}

						
								timeScrubberEventHandler (rect);
						

								//draw time scrubber
								Color colorSaved = GUI.color;

								//if(__sequence.isPlaying)
						
								float timeScrubberX = __timeAreaW.TimeToPixel ((float)__sequence.timeCurrent, rect);
								Handles.color = Color.red;
								Handles.DrawLine (new Vector3 (timeScrubberX, rect.y), new Vector3 (timeScrubberX, rect.height - 16f));//horizontal scroller
								Handles.color = colorSaved;
							

								sequenceNodeDragEventHandler (rect);
						}
				}


				/// <summary>
				/// Times the scrubber event handler.
				/// </summary>
				/// <param name="rect">Rect.</param>
				private void timeScrubberEventHandler (Rect rect)
				{
						Event e = Event.current;
					
						//check timeline click
						if (!__sequence.isPlaying && e.type != EventType.Used && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && new Rect (rect.x, 0, rect.width, TIME_LABEL_HEIGHT).Contains (Event.current.mousePosition)) {
								e.Use ();	
						
						
								onTimelineClick (rect);
						
						
						}
			
				}
		
		
				/// <summary>
				/// Sequence's node click event handler.
				/// </summary>
				/// <param name="node">Node.</param>
				/// <param name="rect">Rect.</param>
				/// <param name="channelOrd">Channel ord.</param>
				private void sequenceNodeClickEventHandler (SequenceNode node, Rect rect, int channelOrd)
				{

						Event ev = Event.current;
						Rect clickRect;
			
						float startTime;
						switch (ev.rawType) {
						case EventType.MouseDown:
				
			
					
								clickRect = new Rect (__timeAreaW.TimeToPixel (node.startTime, rect) - 5, TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT, 10, NODE_RECT_HEIGHT);
					
								//check start of the node rect width=5
								if (clickRect.Contains (Event.current.mousePosition)) {
										
										__nodeSelected = node;
										resizeNodeStart = true;
										ev.Use ();
								}
					
								clickRect.x = __timeAreaW.TimeToPixel (node.startTime + node.duration, rect) - 5;
					
					
								//check the end of node rect width=5
								if (clickRect.Contains (Event.current.mousePosition)) {
										__nodeSelected = node;
										resizeNodeEnd = true;
										ev.Use ();
								}
					
					
								clickRect.x = __timeAreaW.TimeToPixel (node.startTime, rect) + 5;
								clickRect.width = __timeAreaW.TimeToPixel (node.startTime + node.duration, rect) - clickRect.x - 5;
					
								if (clickRect.Contains (Event.current.mousePosition)) {
										
										if (ev.button == 0) {
												
												timeClickOffset = node.startTime - __timeAreaW.PixelToTime (Event.current.mousePosition.x, rect);
												dragNode = true;
												__nodeSelected = node;
										} 
										if (ev.button == 1) {
												GenericMenu genericMenu = new GenericMenu ();
												genericMenu.AddItem (new GUIContent ("Remove"), false, this.RemoveNode, node);
												genericMenu.ShowAsContext ();
												__nodeSelected = node;
										}
										ev.Use ();
								}

								break;
				
						case EventType.ContextClick:

								//don't allow removing right click when play or record
								if (__sequence.isPlaying || __sequence.isRecording)
										break;

								clickRect = new Rect (0, 0, 0, NODE_RECT_HEIGHT);
				

					
								clickRect.x = __timeAreaW.TimeToPixel (node.startTime, rect);
								clickRect.y = TIME_LABEL_HEIGHT + EVENT_PAD_HEIGHT + channelOrd * NODE_RECT_HEIGHT;
								clickRect.width = __timeAreaW.TimeToPixel (node.startTime + node.duration, rect) - clickRect.x;
					
								if (clickRect.Contains (Event.current.mousePosition)) {
										GenericMenu genericMenu = new GenericMenu ();
										genericMenu.AddItem (new GUIContent ("Remove"), false, this.RemoveNode, node);
										genericMenu.ShowAsContext ();
										
								}

								break;
						}



				}


				/// <summary>
				/// Handles the events, resize, pan.
				/// </summary>
				/// <param name="rect">Rect.</param>
				private void sequenceNodeDragEventHandler (Rect rect)
				{
						Event ev = Event.current;
						//Rect clickRect;

						float startTime;
						float leftOffset = 0f;
						float rightOffset = 0f;

						switch (ev.rawType) {

						case EventType.MouseDrag:

								//TODO don't allow resize bigger then sound.length 
								//TODO don't allow any video resize as MovieTexture doesn't support it
								//TODO sound can't be decreased not increased
								


								if (resizeNodeStart) {
										
										//float prevStartTime = __nodeSelected.startTime;
										startTime = __timeAreaW.PixelToTime (Event.current.mousePosition.x, rect);
										

										if (startTime >= 0) {
												//__nodeSelected.startTime = TimeAreaW.SnapTimeToWholeFPS (startTime, __frameRate);
												//__nodeSelected.duration += TimeAreaW.SnapTimeToWholeFPS (prevStartTime - __nodeSelected.startTime, __frameRate);
												
												
											

												
												ev.Use ();
										}
										

										
								}

								if (resizeNodeEnd) {
										
										//__nodeSelected.duration = TimeAreaW.SnapTimeToWholeFPS (__timeAreaW.PixelToTime (Event.current.mousePosition.x, rect) - __nodeSelected.startTime, __frameRate);
										ev.Use ();
								}

								//PAN of the node
								if (dragNode && !resizeNodeStart && !resizeNodeEnd) {
										
										startTime = __timeAreaW.PixelToTime (Event.current.mousePosition.x, rect) + timeClickOffset;

										startTime = TimeAreaW.SnapTimeToWholeFPS (startTime, __sequence.frameRate);

										if (startTime >= 0) {

												
												SequenceChannel channel = __nodeSelected.channel;

												SequenceNode nodePrev = null;
												SequenceNode nodeNext = null;

												float nodeEnd = 0f;

												if (channel.nodes.Count > 1) {

														//find if prev node from selected node exist
														if (__nodeSelected.index > 0) {
																nodePrev = channel.nodes [__nodeSelected.index - 1];

																if (__nodeSelected.duration < nodePrev.duration)
																		leftOffset = nodePrev.duration - __nodeSelected.duration;
														}

														//find if next node from selected node exist
														if (__nodeSelected.index + 1 < channel.nodes.Count) { //if next node exist
																nodeNext = channel.nodes [__nodeSelected.index + 1];

																if (__nodeSelected.duration < nodeNext.duration)
																		rightOffset = nodeNext.duration - __nodeSelected.duration;
														}

														if (__nodeSelected.source is AnimationClip) { //nodes animation clips are allowed to overlap(transitions)

																__nodeSelected.transition = 0f;
															
																if (nodePrev != null && nodeNext != null) {//if there is prev and next node of current selected node
	

																		//restrict selected node draging 
																		if (nodePrev.startTime + leftOffset < startTime && startTime + __nodeSelected.duration < nodeNext.startTime + nodeNext.duration - rightOffset) {
																				__nodeSelected.startTime = startTime;

																				//////////////////////////////////////
																				//calc transition with prev node with selected
																				__nodeSelected.transition = 0f;
																				nodeEnd = nodePrev.startTime + nodePrev.duration;
																				if (startTime < nodeEnd) {
																						__nodeSelected.transition = TimeAreaW.SnapTimeToWholeFPS (nodeEnd - startTime, __sequence.frameRate);
																						//Debug.Log ("Transition Prev:" + __nodeSelected.transition);
																				}

																				/////////////////////////////
																				//calc transition from selected to next node
																				nodeNext.transition = 0f;//reset
																				nodeEnd = startTime + __nodeSelected.duration;
																				if (nodeEnd > nodeNext.startTime) {
																						nodeNext.transition = TimeAreaW.SnapTimeToWholeFPS (nodeEnd - nodeNext.startTime, __sequence.frameRate);
																						//Debug.Log ("Transition Next:" + nodeNext.transition);
																				}


																		}
																} else if ((nodePrev != null && nodePrev.startTime + leftOffset < startTime) || (nodeNext != null && startTime + __nodeSelected.duration < nodeNext.startTime + nodeNext.duration - rightOffset)) {
																		__nodeSelected.startTime = startTime;


																		if (nodePrev != null) {
																				//////////////////////////////////////
																				//calc transition with prev node with selected
																				__nodeSelected.transition = 0f;
																				nodeEnd = nodePrev.startTime + nodePrev.duration;
																				if (startTime < nodeEnd) {
																						__nodeSelected.transition = TimeAreaW.SnapTimeToWholeFPS (nodeEnd - startTime, __sequence.frameRate);
																						//Debug.Log ("Transition Only Prev:" + __nodeSelected.transition);
																				}
																		}
																		
																		if (nodeNext != null) {
																				/////////////////////////////
																				//calc transition from selected to next node
																				nodeNext.transition = 0f;
																				nodeEnd = startTime + __nodeSelected.duration;
																				if (nodeEnd > nodeNext.startTime) {
																						nodeNext.transition = TimeAreaW.SnapTimeToWholeFPS (nodeEnd - nodeNext.startTime, __sequence.frameRate);
																						//Debug.Log ("Transition Only Next:" + nodeNext.transition);
																				}
																		}





																	    
																}
									
																Repaint ();

																//ANY OTHER CHANNEL TYPE	
														} else {
																if (nodePrev != null && nodeNext != null) {//if there is prev and next node of current
																		if (nodePrev.startTime + nodePrev.duration < startTime && startTime + __nodeSelected.duration < nodeNext.startTime) {
																				__nodeSelected.startTime = startTime;
																		}

																} else if ((nodePrev != null && nodePrev.startTime + nodePrev.duration < startTime) || (nodeNext != null && startTime + __nodeSelected.duration < nodeNext.startTime)) {
																		__nodeSelected.startTime = startTime;
																}
														}
												} else
														__nodeSelected.startTime = startTime;


												
							 


										}// startTime>=0
								}// if PAN
										

										
										//change channel 
//										if (Event.current.mousePosition.y > selectedNode.channel * __nodeRectHeight + 25) {
//												selectedNode.channel += 1;
//										}
//										if (Event.current.mousePosition.y < selectedNode.channel * __nodeRectHeight - 5) {
//												selectedNode.channel -= 1;
//										}
//										selectedNode.channel = Mathf.Clamp (selectedNode.channel, 0, int.MaxValue);
								ev.Use ();
								
								break;
						case EventType.MouseUp:
								dragNode = false;
								resizeNodeStart = false;
								resizeNodeEnd = false;
								break;
						}
				}





				/// <summary>
				/// Removes the node.
				/// </summary>
				/// <param name="data">Data.</param>
				private void RemoveNode (object data)
				{
						SequenceNode node = data as SequenceNode;
						
					

						SequenceChannel sequenceChannel = __sequence.channels.Find (itm => itm.nodes.Exists (nd => nd.GetInstanceID () == node.GetInstanceID ()));

			
						
						if (node.source is AnimationClip) {
								if (node.index + 1 < sequenceChannel.nodes.Count)//if prev node exist reset its transition
										sequenceChannel.nodes [node.index + 1].transition = 0;

								//bypass transition
//								if (node.index - 1 > -1 && node.index + 1 < sequenceChannel.nodes.Count) {
										
//										UnityEditor.Animations.AnimatorController animatorController = (sequenceChannel.runtimeAnimatorController as UnityEditor.Animations.AnimatorController);
//										UnityEditor.Animations.AnimatorState statePrev = animatorController.GetStateBy (sequenceChannel.nodes [node.index - 1].stateNameHash);
//								
//										UnityEditor.Animations.AnimatorStateTransition transition = statePrev.transitions [0];
//										transition.destinationState = animatorController.GetStateBy (sequenceChannel.nodes [node.index + 1].stateNameHash);
//								
//								}
						}



						//remove node
						sequenceChannel.nodes.Remove (node);
						


						if (sequenceChannel.runtimeAnimatorController != null)//remove AnimatorState
								(sequenceChannel.runtimeAnimatorController as UnityEditor.Animations.AnimatorController).RemoveStateWith (node.stateNameHash);
						
						int nodesCount = sequenceChannel.nodes.Count;


						//reindex nodes after remove
						for (int i=node.index; i<nodesCount; i++)
								sequenceChannel.nodes [i].index--;

						
						


						//if this removed node was last node => remove channel and move channels up(reindex)
						if (sequenceChannel.nodes.Count == 0) {

								//remove channel
								__sequence.channels.Remove (sequenceChannel);
								
							
								DestroyImmediate(sequenceChannel);

						}
						
						

						__nodeSelected = null;

						__sequence.timeCurrent = 0f;
					
						ResetChannelsTarget ();

						__needSave = true;
				
				
						Repaint ();
				}



				/// <summary>
				/// Raises the game object selection changed event.
				/// </summary>
				/// <param name="data">Data.</param>
				private void OnGameObjectSelectionChanged (object data)
				{
						Sequence sequence = data as Sequence;
						Selection.activeGameObject = sequence.gameObject;
						__sequenceGameObject = sequence.gameObject;
						__sequence = sequence;
				}


				/// <summary>
				/// Sequences the drop source event handler.
				/// </summary>
				/// <param name="area">Area.</param>
				/// <param name="channel">Channel.</param>
				public void sequenceDropSourceEventHandler (Rect area, int channel)
				{

						if (__sequence.isPlaying || __sequence.isRecording)
								return;
			
						Event evt = Event.current;
			
						switch (evt.type) {
						case EventType.DragUpdated:
						case EventType.DragPerform:
								if (!area.Contains (evt.mousePosition))
										return;
								DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

								Type draggedType;
				
								if (evt.type == EventType.DragPerform) {
										DragAndDrop.AcceptDrag ();

										if (EditorApplication.isPlaying) {
												Debug.Log ("Can't add tween object in play mode. Stop the play mode and readd it.");
										} else {
												foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences) {

														//Debug.Log ("Dropped object of type:" + dragged_object);
														
														//handle sound, video, animation clip
														draggedType = dragged_object.GetType ();



														if (draggedType == typeof(UnityEngine.AnimationClip) || 
																draggedType == typeof(UnityEngine.AudioClip) ||
																draggedType == typeof(UnityEngine.MovieTexture)) {

																
																Stop ();

																//allow only dropping multiply items of same type in same channel
																SequenceChannel sequenceChannel = null;

																
																if (channel < __sequence.channels.Count) {
																		sequenceChannel = __sequence.channels [channel];

																
																} else {
																		sequenceChannel = (SequenceChannel)ScriptableObject.CreateInstance<SequenceChannel> ();

										
																		if (draggedType == typeof(AnimationClip)) {

																				//open Animation controller for the channel
																				string path = EditorUtility.OpenFilePanel (
																					"Select AnimaitonController",
																					"Assets",
																					"controller");
																				
																				if (String.IsNullOrEmpty (path))
																						continue;
																					
																				sequenceChannel.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath (AssetDatabaseUtility.AbsoluteUrlToAssets (path), typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
										                                                                        
																				sequenceChannel.name = "Animation";

																		} else if (draggedType == typeof(AudioClip)) {
																				sequenceChannel.name = "Audio";
																				sequenceChannel.type = SequenceChannel.SequenceChannelType.Audio;
																		} else if (draggedType == typeof(MovieTexture)) {
																				sequenceChannel.name = "Video";
																				sequenceChannel.type = SequenceChannel.SequenceChannelType.Video;
																		}

																		sequenceChannel.sequence = __sequence;
																		__sequence.channels.Add (sequenceChannel);
																}

																if (sequenceChannel.nodes.Count > 0) {
																		if (sequenceChannel.nodes [0].source.GetType () != draggedType) {
																				Debug.LogWarning ("You can have only same type nodes in same channel");	
																				continue;
																		}
																}


																//Vector2
																//TODO current logic prevents overlapping on drop (might be ehnaced to allow transition overlap in Animation's node to some meassure)
																//and if they overlapp move start time to fit
																SequenceNode node = CreateNewSequenceNode (Event.current.mousePosition, dragged_object, channel);
														
																
																if (node != null) {
																		sequenceChannel.nodes.Insert (node.index, node);
																		
																		int nodesCount = sequenceChannel.nodes.Count;

																		//reindex nodes after insert
																		for (int i=node.index+1; i<nodesCount; i++)
																				sequenceChannel.nodes [i].index++;

																		__sequence.timeCurrent = 0f;
																		ResetChannelsTarget ();

																		__needSave = true;
										
																}
																
																	
														

																
																__nodeSelected = node;
														}
												}
												EditorUtility.SetDirty (__sequence);
										}
								}
								break;
						}
			
				}

							

				/// <summary>
				/// Creates the sequence node.
				/// </summary>
				/// <returns>The sequence node.</returns>
				/// <param name="pos">Position.</param>
				/// <param name="source">Source.</param>
				/// <param name="channel">Channel.</param>
				private SequenceNode CreateNewSequenceNode (Vector2 pos, UnityEngine.Object source, int channelOrd)
				{


						float duration = 0f;


						float startTime = 0f;


						startTime = TimeAreaW.SnapTimeToWholeFPS (__timeAreaW.PixelToTime (pos.x, __timeAreaW.rect), __sequence.frameRate);
						duration = TimeAreaW.SnapTimeToWholeFPS (duration, __frameRate);

						SequenceChannel sequenceChannel = __sequence.channels [channelOrd];

						//prevent intersection on Drop
						if (sequenceChannel.nodes.Exists (itm => (itm.startTime < startTime && startTime < itm.startTime + itm.duration) || (itm.startTime < startTime + duration && startTime + duration < itm.startTime + itm.duration)))
								return null;

						


						SequenceNode node = ScriptableObject.CreateInstance<SequenceNode> ();			
					
						node.source = source;
			
			
						node.channel = sequenceChannel;

						
					
						node.name = source.name;
						
					

					
					
						node.startTime = startTime;
						

						int startTimeEarliestIndex = -1;

						if (node.source is AnimationClip) {

								
					
								UnityEditor.Animations.AnimatorController animatorController = node.channel.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;


					
								//create and add AnimatorState in controller from node.source
								UnityEditor.Animations.AnimatorState stateCurrent = animatorController.AddMotion ((source as Motion));

								//save AnimatorState ID hase
								node.stateNameHash = stateCurrent.nameHash;

								node.clipBinding=ScriptableObject.CreateInstance<EditorClipBinding>();
				
								//if stateCurrent is first and default create empty state for default
								if (animatorController.layers [0].stateMachine.defaultState == stateCurrent) {
										stateCurrent = animatorController.layers [0].stateMachine.AddState ("DefaultState");
										animatorController.layers [0].stateMachine.defaultState = stateCurrent;
								}


						}
				

								
				
						//UnityEditor.Animations.AnimatorStateTransition transition = null;
				
						float endTimeNode = 0f;

								
						if (sequenceChannel.nodes.Count > -1) {

								//find index of node with earliest startTime
								startTimeEarliestIndex = sequenceChannel.nodes.FindLastIndex (itm => itm.startTime < node.startTime);

						}
						
					
						node.index = startTimeEarliestIndex + 1;
						
			
						return node;
				}




				/// <summary>
				/// Ons the sequence node start.
				/// </summary>
				/// <param name="node">Node.</param>
				public static void onSequenceNodeStart (SequenceNode node)
				{

						Debug.Log ("onSequenceNodeStart " + node.name);
			          
						if (node.source is AudioClip) {
								AudioUtilW.PlayClip (node.source as AudioClip, 0, node.loop);//startSample doesn't work in this function????
						} else if (node.source is MovieTexture) {
								MovieTexture movieTexture = node.source as MovieTexture;
								if (node.channel.target.tag == Tags.MAIN_CAMERA) {//==Camera.main

										Debug.Log (node.name + " RenderSettings.skybox isn't supported in EditMode");
										//RenderSettings.skybox.mainTexture = movieTexture;
										
										
								} else {

										Renderer renderer = node.channel.target.GetComponent<Renderer> ();
										if (renderer != null) {
											renderer.material=new Material(	Shader.Find("Unlit/Texture"));	

											renderer.material.mainTexture = movieTexture;
												//renderer.sharedMaterial.mainTexture=movieTexture;//Instantiating material due to calling renderer.material during edit mode. This will leak materials into the scene. You most likely want to use renderer.sharedMaterial instead.
												//renderer.material.shader=Shader.Find("Unlit/Texture");

												//remapp uv so it is not upsidedown


												//renderer.sharedMaterial.mainTexture = movieTexture;

															
												
										}
								}

				//This line would play movieTexture sound but if you have other sound would stack sound samplerr
								//if (movieTexture.audioClip != null)
								//		AudioUtilW.PlayClip (movieTexture.audioClip, 0);//startSample doesn't work in this function????

				
				
				
//								AudioSource audioSource = null;
//										if (movieTexture.audioClip != null) {
//											
//											audioSource = node.channel.target.GetComponent<AudioSource> ();
//											
//											if (audioSource == null)
//												audioSource = (AudioSource)node.channel.target.AddComponent (typeof(AudioSource));
//											
//											audioSource.clip = movieTexture.audioClip;
//					                        
//											audioSource.PlayOneShot (audioSource.clip);
//											//audioSource.Play();
//											
//											
//										}
				
								movieTexture.Play ();
				
						}
				}


				/// <summary>
				/// Ons the sequence end.
				/// </summary>
				/// <param name="sequence">Sequence.</param>
				public static void onSequenceEnd (Sequence sequence)
				{
						Stop ();
				}


				/// <summary>
				/// Ons the sequence node stop.
				/// </summary>
				/// <param name="node">Node.</param>
				public static void onSequenceNodeStop (SequenceNode node)
				{
						 
			
				}

				private static void CreateRuntimeAnimatorController ()
				{
						string path = EditorUtility.SaveFilePanel (
							"Create AnimaitonController",
							"Assets",
							"",
							"controller");
						
						if (!String.IsNullOrEmpty (path)) {
							
							
								//create Controller
								UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath (AssetDatabaseUtility.AbsoluteUrlToAssets (path));
							
							
						}
						
						
				
				}
		
			

			
				




				/// <summary>
				/// Creates the new animation clip.
				/// </summary>
				private static void CreateNewAnimationClip ()
				{

						string path = EditorUtility.SaveFilePanel (
					"Create New Clip",
					"Assets",
					"",
					"anim");

						if (!String.IsNullOrEmpty (path)) {
				
								AnimationClip clip = new AnimationClip ();//UnityEditor.Animations.AnimatorController.AllocateAnimatorClip ();
								clip.name = Path.GetFileNameWithoutExtension (path);		
								AssetDatabase.CreateAsset (clip, AssetDatabaseUtility.AbsoluteUrlToAssets (path));
								AssetDatabase.SaveAssets ();
								clip.frameRate = __sequence.frameRate;
					
						}
		
				}

	

					
		
		
				////   CREATE NEW SEQUENCE GAME OBJECT WITH SEQUENCE BEHAVIOUR ////
				/// <summary>
				/// Creates the new sequence.
				/// </summary>
				private void CreateNewSequence ()
				{
						
						List<Sequence> sequences = GameObjectUtilityEx.FindAllContainComponentOfType<Sequence> ();
						int count = 0;

						while (sequences.Find(x=>x.name == "Sequence "+count.ToString())!=null) {
								count++;
						}

						__sequenceGameObject = new GameObject ("Sequence " + count.ToString ());
						__sequence = __sequenceGameObject.AddComponent<Sequence> ();


						Selection.activeGameObject = __sequenceGameObject;
				}



				/// <summary>
				/// Gets the audio clip texture.
				/// </summary>
				/// <returns>The audio clip texture.</returns>
				/// <param name="clip">Clip.</param>
				/// <param name="width">Width.</param>
				/// <param name="height">Height.</param>
				public static Texture2D GetAudioClipTexture (AudioClip clip, float width, float height)
				{
						if (clip == null) {
								return null;
						}
						AudioImporter audioImporter = (AudioImporter)AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (clip));

						Texture2D[] array = new Texture2D[clip.channels];
						for (int i = 0; i < clip.channels; i++) {
								array [i] = (Texture2D)AudioUtilW.GetWaveForm (
				                                    
					clip,
					audioImporter,
					i,
					width,
					height / (float)clip.channels
								);
						}


						return AudioClipInspectorW.CombineWaveForms (array);
				}


		


				/// <summary>
				/// Handle the selection(gameobject) change event.
				/// this is EditorWindow function and its autobindend by name
				/// </summary>
				void OnSelectionChange ()
				{
						selectionChangeEventHandler ();
				}

				private static void selectionChangeEventHandler ()
				{


						if (Selection.activeGameObject != null) {


								Stop ();

								Sequence sequence = Selection.activeGameObject.GetComponent<Sequence> ();
								if (sequence != null) {
										__sequenceGameObject = sequence.gameObject;
										__sequence = sequence;
					
										SceneView.currentDrawingSceneView.Repaint ();
									
										EditorWindow.GetWindow<SequenceEditorWindow> ().Repaint ();

									

										__timeAreaW.hTicks.SetTickModulosForFrameRate (__sequence.frameRate);
								}
						}
				}
		
		
		}
}