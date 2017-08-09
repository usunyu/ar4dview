using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(FDVUnityPlugin))]
public class FDVUnityPluginEditor : Editor
{
	bool showPath = false;

    public override void OnInspectorGUI()
    {
        FDVUnityPlugin myTarget = (FDVUnityPlugin)target;

        Undo.RecordObject(myTarget, "Inspector");

		GUILayout.Space (7);
		myTarget._sourceType = (SOURCE_TYPE) GUILayout.Toolbar ((int)myTarget._sourceType, new string[] {"Files", "Network"});
		GUILayout.Space (10);

		switch (myTarget._sourceType) {
		case SOURCE_TYPE.Files:
			BuildFilesInspector (myTarget);
			break;
		case SOURCE_TYPE.Network:
			BuildNetworkInspector (myTarget);
			break;
		}			
        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }



	private void BuildFilesInspector(FDVUnityPlugin myTarget) {

		myTarget._autoPlay = EditorGUILayout.Toggle("Auto Play", myTarget._autoPlay);

		bool val = EditorGUILayout.Toggle("Compute Normals", myTarget._computeNormals);
		if (val != myTarget._computeNormals)
		{
			myTarget._computeNormals = val;
			myTarget.Preview();
		}
			
		Rect rect = EditorGUILayout.BeginVertical();
		myTarget._sequenceName = EditorGUILayout.TextField("Sequence Name", myTarget._sequenceName);
		EditorGUILayout.EndVertical();

		showPath = EditorGUILayout.Foldout(showPath, "Data Path");
		if (showPath)
		{
			myTarget._dataInStreamingAssets = EditorGUILayout.Toggle("In Streaming Assets", myTarget._dataInStreamingAssets);
			if (!myTarget._dataInStreamingAssets)
			{
				myTarget._mainDataPath = EditorGUILayout.TextField("Main Path", myTarget._mainDataPath);
				for (int i = 0; i < myTarget._alternativeDataPaths.Count; i++)
				{
					if (myTarget._alternativeDataPaths[i] == "" && i < myTarget._alternativeDataPaths.Count - 1)
					{
						myTarget._alternativeDataPaths.Remove("");
						i--;
					}
					else
						myTarget._alternativeDataPaths[i] = EditorGUILayout.TextField("Alternative Path", myTarget._alternativeDataPaths[i]);
				}

				if (GUILayout.Button("+", GUILayout.Width(40)))
				{
					myTarget._alternativeDataPaths.Add("");
					//					myTarget._alternativeDataPaths[myTarget._alternativeDataPaths.Count-1] = EditorGUILayout.TextField ("Alternative Path", "");
				}
			}
		}

		GUIContent previewframe = new GUIContent("Preview Frame");
		Color color = GUI.color;
		if ( (myTarget._activeRangeMax != -1) && (myTarget._previewFrame < (int)myTarget._activeRangeMin || myTarget._previewFrame > (int)myTarget._activeRangeMax))
			GUI.color = new Color(1, 0.6f, 0.6f);

		int frameVal = EditorGUILayout.IntSlider(previewframe, myTarget._previewFrame, 0, myTarget._nbFrames - 1);
		if (frameVal != myTarget._previewFrame)
		{
			myTarget._previewFrame = (int)frameVal;
			myTarget.Preview();
			myTarget.last_preview_time = System.DateTime.Now;
		}
		else
			myTarget.ConvertPreviewTexture();
		GUI.color = color;

		GUIContent activerange = new GUIContent("Active Range");
        float rangeMax = myTarget._activeRangeMax == -1 ? myTarget._nbFrames - 1 : myTarget._activeRangeMax;
        if (myTarget._activeRangeMax == -1)
            GUI.color = new Color(0.5f, 0.7f, 2.0f);
        EditorGUILayout.MinMaxSlider(activerange, ref myTarget._activeRangeMin, ref rangeMax, 0.0f, myTarget._nbFrames - 1);
        if (rangeMax == myTarget._nbFrames - 1 && myTarget._activeRangeMin==0)
            myTarget._activeRangeMax = -1;
        else
            myTarget._activeRangeMax = rangeMax;

        EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();

        if ( myTarget._activeRangeMax == -1)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Full Range", GUILayout.Width(80));
            GUI.color = color;
            EditorGUILayout.Space();
            myTarget._activeRangeMax = -1;
        }
        else
        {
            myTarget._activeRangeMin = EditorGUILayout.IntField((int)myTarget._activeRangeMin, GUILayout.Width(50));
            EditorGUILayout.Space();
            myTarget._activeRangeMax = EditorGUILayout.IntField((int)myTarget._activeRangeMax, GUILayout.Width(50));
        }


		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		myTarget._outRangeMode = (OUT_RANGE_MODE)EditorGUILayout.EnumPopup("Out of Range Mode", myTarget._outRangeMode);

		myTarget._debugInfo = EditorGUILayout.Toggle("Debug Info", myTarget._debugInfo);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Help...", GUILayout.Width(150)))
		{
			string helpMessage = "If you want the data to be \"in app\", you should put sequence directory into Assets/StreamingAssets project directory.\n";
			helpMessage += "Then it will be copied verbatim in application resources during the deployment.\n\n";
			helpMessage += "You can drag and drop the sequence directory from the assets in the \"Sequence name\" field.\n\n";
			helpMessage += "Otherwise, if you want to keep the 4DViews data external to the project, you can drag and drop the sequence from an external explorer.\n ";
			helpMessage += "Then you can specify alternative paths the application will look in to try to find the data (depending on the device for instance).\n\n";
			helpMessage += "These paths can be absolute or relatives\n";
			EditorUtility.DisplayDialog("How to specify the 4DViews data path", helpMessage, "Close");
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		Event evt = Event.current;
		switch (evt.type)
		{
		case EventType.DragUpdated:
		case EventType.DragPerform:
			if (rect.Contains(evt.mousePosition))
				DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			else
			{
				//EditorGUILayout.EndVertical();
				return;
			}

			if (evt.type == EventType.DragPerform)
			{
				foreach (string path in DragAndDrop.paths)
				{
					string seqName = path.Substring(path.LastIndexOf("/") + 1);
					string dataPath = path.Substring(0, path.LastIndexOf("/") + 1);

					if (dataPath.Contains("StreamingAssets"))
					{
						myTarget._dataInStreamingAssets = true;
						dataPath = dataPath.Substring(dataPath.LastIndexOf("StreamingAssets") + 16);
						myTarget._mainDataPath = dataPath;
					}
					else
					{
						if (dataPath.Contains("Assets"))
						{
							string message = "The sequence should be in \"Streaming Assets\" for a good application deployment";
							EditorUtility.DisplayDialog("Warning", message, "Close");
						}
						myTarget._dataInStreamingAssets = false;
						myTarget._mainDataPath = dataPath;
					}
					myTarget._sequenceName = seqName;

					EditorUtility.SetDirty(target);

					myTarget.Preview();
				}
			}
			break;
		}
	}



	private void BuildNetworkInspector(FDVUnityPlugin myTarget) {

		myTarget._autoPlay = EditorGUILayout.Toggle("Auto Play", myTarget._autoPlay);
		EditorGUILayout.BeginVertical();
		myTarget._serverAddress = EditorGUILayout.TextField("Server address", myTarget._serverAddress);	
		myTarget._serverPort = EditorGUILayout.IntField("Server port", myTarget._serverPort);

		EditorGUILayout.EndVertical();
		myTarget._debugInfo = EditorGUILayout.Toggle("Debug Info", myTarget._debugInfo);
	}
}

