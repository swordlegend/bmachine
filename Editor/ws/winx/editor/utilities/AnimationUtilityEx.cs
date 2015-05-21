// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace ws.winx.editor.utilities
{
		public static class AnimationUtilityEx
		{
				static Type TransformType = typeof(Transform);
				static EditorCurveBinding _editorCurveBinding_PosX = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalPosition.x");
				static EditorCurveBinding _editorCurveBinding_PosY = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalPosition.y");
				static EditorCurveBinding _editorCurveBinding_PosZ = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalPosition.z");
				static EditorCurveBinding _editorCurveBinding_RotX = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalRotation.x");
				static EditorCurveBinding _editorCurveBinding_RotY = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalRotation.y");
				static EditorCurveBinding _editorCurveBinding_RotZ = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalRotation.z");
				static EditorCurveBinding _editorCurveBinding_SclX = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalScale.x");
				static EditorCurveBinding _editorCurveBinding_SclY = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalScale.y");
				static EditorCurveBinding _editorCurveBinding_SclZ = EditorCurveBinding.FloatCurve ("", TransformType, "m_LocalScale.z");

				public static EditorCurveBinding EditorCurveBinding_PosX {
						get {

								return _editorCurveBinding_PosX;
						}


				}

				public static EditorCurveBinding EditorCurveBinding_PosY {
						get {
								
										
								return _editorCurveBinding_PosY;
						}
			
			
				}

				public static EditorCurveBinding EditorCurveBinding_PosZ {
						get {
								
								return _editorCurveBinding_PosZ;
						}
			
			
				}


				/// <summary>
				/// Removes the keyframe from clip (!!! Inserts hidden keyframes in between)
				/// </summary>
				/// <param name="clip">Clip.</param>
				/// <param name="keyframeInx">Keyframe inx.</param>
				public static void RemoveKeyframeFrom (AnimationClip clip, int keyframeInx)
				{

						EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings (clip);
						AnimationCurve curve;
						int curveBindingsNum = curveBindings.Length;
						EditorCurveBinding curveBindingCurrent;

						for (int i=0; i<curveBindingsNum; i++) {

								curveBindingCurrent = curveBindings [i];
								curve = AnimationUtility.GetEditorCurve (clip, curveBindingCurrent);

								if (curve != null && curve.length > keyframeInx) {
										curve.RemoveKey (keyframeInx);
										AnimationUtility.SetEditorCurve (clip, curveBindingCurrent, curve);
								}
						}
					

						
				}


				/// <summary>
				/// Gets the time at clip and keyFrameInx.
				/// </summary>
				/// <returns>The <see cref="System.Single"/>.</returns>
				/// <param name="clip">Clip.</param>
				/// <param name="keyFrameInx">Key frame inx.</param>
				public static float GetTimeAt (AnimationClip clip, int keyFrameInx)
				{

						return AnimationUtility.GetEditorCurve (clip, EditorCurveBinding_PosX).keys [keyFrameInx].time;


				}



				/// <summary>
				/// Gets the positions x,y,z by keyframes transformed from local to world space
				/// //expect every clip to have x,y,z curves
				/// </summary>
				/// <returns>The positions.</returns>
				/// <param name="clip">Clip.</param>
				/// <param name="transform">Transform.</param>
				public static Vector3[] GetPositions (AnimationClip clip, Transform transform=null)
				{

						Type T = typeof(Transform);	
						AnimationCurve curvePosX = AnimationUtility.GetEditorCurve (clip, EditorCurveBinding_PosX);
						AnimationCurve curvePosY = AnimationUtility.GetEditorCurve (clip, EditorCurveBinding_PosY);
						AnimationCurve curvePosZ = AnimationUtility.GetEditorCurve (clip, EditorCurveBinding_PosZ);
						

						Vector3[] positionVectors = null;

						//expect every keyframe to have x,y,z
						if (curvePosX != null && curvePosY != null && curvePosZ != null) {

								int keysNumber = curvePosX.length;


								positionVectors = new Vector3[keysNumber];
									
								
								for (int keyCurrent=0; keyCurrent<keysNumber; keyCurrent++) {

										if (transform != null)//conver local transform to world
												positionVectors [keyCurrent] = transform.TransformPoint (new Vector3 (curvePosX.keys [keyCurrent].value, curvePosY.keys [keyCurrent].value, curvePosZ.keys [keyCurrent].value));
										else
												positionVectors [keyCurrent] = new Vector3 (curvePosX.keys [keyCurrent].value, curvePosY.keys [keyCurrent].value, curvePosZ.keys [keyCurrent].value);
								}
								

						}


					


						return positionVectors;
				}

		}
}

