using UnityEngine;
using System.Collections;

//------------------------------
// FDV extra tool which adds a slider 
// allowing to go through the sequence timeline
// Also displays the current frame and file index
//------------------------------

public class FDVTimeline : MonoBehaviour {

	FDVUnityPlugin fdv;
	bool fullRange = false;
	int newFrameId;

	void Awake() {
		fdv = this.GetComponent<FDVUnityPlugin> ();
		//When this component is active, the playback is stopped to be controlled
		//by the slider
		if(this.isActiveAndEnabled) fdv._autoPlay = false;
	}

	void Start () {
		fdv.Play (false);
		newFrameId = fdv.GetFirstActiveFrameId ();
	}


	void Update() {
		if (newFrameId != fdv.GetCurrentFrame ()) //Need to be fixed in plugin 
			fdv.GotoFrame (newFrameId);
	}

	void OnGUI() {

		bool newfullRange = GUI.Toggle (new Rect (25, Screen.height - 65, 200, 20), fullRange, "Full sequence range");

		if (!newfullRange && fullRange) {
			int firstFrame = fdv.GetFirstActiveFrameId ();
			int lastFrame = firstFrame + fdv.GetActiveNbFrames () - 1;
			if (newFrameId < firstFrame) 
				newFrameId = firstFrame;
			else if (newFrameId > lastFrame)
				newFrameId = lastFrame;
				
		}

		fullRange = newfullRange;

		{
			int firstFrame, lastFrame, fileFrame;
			int sliderMargin;

			if (fullRange) {
				firstFrame = 0;
				lastFrame = fdv.GetSequenceNbFrames () - 1;
				sliderMargin = 25;
			} else {
				firstFrame = fdv.GetFirstActiveFrameId ();
				lastFrame = firstFrame + fdv.GetActiveNbFrames () - 1;
				sliderMargin = 75;
			}
			fileFrame = fdv.GetFirstIndex () + fdv.GetCurrentFrame ();
			string message = fdv.GetCurrentFrame ().ToString () + " / " + lastFrame.ToString () + "  file-" + fileFrame.ToString ();
			GUI.Label (new Rect (10, 10, 200, 20), message);

			newFrameId = (int)GUI.HorizontalSlider (new Rect (sliderMargin, Screen.height - 35, Screen.width - (sliderMargin * 2), 20), newFrameId, (float)firstFrame, (float)lastFrame);
		}
	}

}

