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
using UnityEditor;
using UnityEngine;
namespace ws.winx.editor.extensions
{
	public class AssetPostProcessorEventDispatcher:AssetPostprocessor
	{


		// A delegate type for hooking up change notifications.
		public delegate void ImportedEventHandler(string[] importedAssetsPath);
		// An event that clients can use to be notified whenever the
		// elements of the list change.
		public static event ImportedEventHandler Imported;


		static void OnPostprocessAllAssets (
			string[] importedAssets ,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths) {

			if (Imported != null)
								Imported (importedAssets);

//			foreach (var str in importedAssets)
//				Debug.Log("Reimported Asset: " + str);
//			foreach (var str in deletedAssets)
//				Debug.Log("Deleted Asset: " + str);
//			for (var i=0;i<movedAssets.Length;i++)
//				Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
		}
	}
}

