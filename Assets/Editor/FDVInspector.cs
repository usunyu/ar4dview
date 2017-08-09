using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(FDVSync)), CanEditMultipleObjects]
public class FDVSyncInspector : Editor {
	int currentTab = 0;

	public override void OnInspectorGUI () {
		serializedObject.Update(); 

		GUILayout.Space (7);
		currentTab = GUILayout.Toolbar (currentTab, new string[] {"Audio", "Animation"});
		switch (currentTab) {
		case 0:
			GUILayout.Space (10);
			GUILayout.BeginHorizontal ();
			float firstLabelWidth = EditorGUIUtility.currentViewWidth * 0.7f - 70; //70=buttons+margin 
			GUILayout.Label ("Audio source", GUILayout.Width (firstLabelWidth));
			GUILayout.Label ("Start on frame");
			GUILayout.EndHorizontal ();
			EditorList.Show (serializedObject.FindProperty ("_audioSources"), EditorListOption.Buttons);
			GUILayout.Space (10);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("_audioPrecisionInMsec"), new GUIContent ("Precision (msec)"));
			break;
		case 1:
			GUILayout.Space (10);
			GUILayout.Label ("Animator");
			EditorList.Show (serializedObject.FindProperty ("_animationSources"), EditorListOption.Buttons);
			break;
		}

		GUILayout.Space (15);
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("_debugInfo"));
		serializedObject.ApplyModifiedProperties();
	}
}

[CustomPropertyDrawer(typeof(FDVAudioSource))]
public class FDVAudioSourceDrawer : PropertyDrawer {

	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		// Calculate rects
		float valueW = (float)(position.width)/3f;
		float sourceW = position.width-valueW-5;
		Rect sourceRect = new Rect(position.x, position.y, sourceW, position.height);
		Rect frameRect = new Rect(position.x+sourceW+5, position.y, valueW, position.height);

		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		EditorGUI.PropertyField(sourceRect, property.FindPropertyRelative ("audioSource"), GUIContent.none);
		EditorGUI.PropertyField(frameRect, property.FindPropertyRelative ("startOnFrame"), GUIContent.none);

		EditorGUI.EndProperty();
	}
}

[CustomPropertyDrawer(typeof(FDVAnimationSource))]
public class FDVAnimationSourceDrawer : PropertyDrawer {

	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		EditorGUI.PropertyField(position, property.FindPropertyRelative ("animationSource"), GUIContent.none);

		EditorGUI.EndProperty();
	}
}