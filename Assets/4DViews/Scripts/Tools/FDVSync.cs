using UnityEngine;
using System.Collections;

[System.Serializable]
public class FDVAudioSource{
	public AudioSource audioSource;
	public int startOnFrame = 0;
}

[System.Serializable]
public class FDVAnimationSource{
	public Animator animationSource;
}

public class FDVSync : MonoBehaviour {

	private FDVUnityPlugin _sequence;
	//Audio
	public int _audioPrecisionInMsec = 100;
	private float _audioSynchroPrecision;
	public FDVAudioSource[] _audioSources = new FDVAudioSource[1];
	//Animation
	public FDVAnimationSource[] _animationSources = new FDVAnimationSource[1];
	private float _animationNormalizer;

	public bool _debugInfo = false;

	void Awake() {
		//Get FDV Plugin
		_sequence = transform.GetComponent<FDVUnityPlugin> ();
	}

	void Start() {
		//Audio
		_audioSynchroPrecision = (float)_audioPrecisionInMsec / 1000f;

		//animation
		foreach (FDVAnimationSource animation in _animationSources) {
			if (animation.animationSource != null) {
				animation.animationSource.speed = 0;
			}
		}
		_animationNormalizer = Mathf.Max (1, _sequence.GetSequenceNbFrames () - 1);
		_sequence.onNewModel += SyncAnimationSources;
	}

	void OnDestroy() {
		_sequence.onNewModel -= SyncAnimationSources;
	}
	 

	// Update is called once per frame
	void Update () {
		SyncAudioSources ();
	}
		
	void SyncAudioSources() {
		int currentFrame = _sequence.GetCurrentFrame ();

		foreach (FDVAudioSource audio in _audioSources) {
			AudioSource source = audio.audioSource;
			int startOnFrame = audio.startOnFrame;

			if (source == null)
				continue;

			//Pause audio if sequence is not playing
			if (!_sequence.IsPlaying ()) {
				source.Pause ();
				continue;
			}

			//Pause audio if sample is out of bounds
			int sample = SeqToClipSample (source, currentFrame - startOnFrame);
			if(sample < 0 || (sample >= source.clip.length * source.clip.frequency)) {
				source.Pause ();
				continue;
			}

			//Update audio if not synchro
			float timeDiff = (float)(Mathf.Abs (sample - source.timeSamples)) / (float)source.clip.frequency;
			if(timeDiff > _audioSynchroPrecision)
				source.timeSamples = sample;

			//Play audio if needed
			if (!source.isPlaying)
				source.Play ();
		}

	}

	int SeqToClipSample(AudioSource source, int frame) {
		return (int) (source.clip.frequency * frame / _sequence.GetFrameRate ());
	}


	void SyncAnimationSources() {
		foreach (FDVAnimationSource animation in _animationSources) {
			Animator source = animation.animationSource;
			if(source != null)
				source.Play ("", -1, (float)(_sequence.GetCurrentFrame() / _animationNormalizer));
		}
	}


	void OnValidate() {
		//Audio
		foreach (FDVAudioSource audio in _audioSources) {
			if (audio.audioSource != null) {
				audio.audioSource.playOnAwake = false;
				audio.audioSource.loop = false;
			}
		}

	}

	void OnGUI() {
		if (_debugInfo) {
			int audioId = 0;
			foreach (FDVAudioSource audio in _audioSources) {
				audioId++;
				AudioSource source = audio.audioSource;
				if (source != null) {
					int startOnFrame = audio.startOnFrame;
					float seqTime = (_sequence.GetCurrentFrame () - startOnFrame) / _sequence.GetFrameRate ();
					float sampleTime = (float)source.timeSamples / (float)source.clip.frequency;
					float diff = seqTime - sampleTime;
					string message = "Audio Sync " + audioId.ToString ("00") + ": " + diff.ToString ("00.00") + "sec\n";
					GUI.Label (new Rect (10, 20 + audioId * 18, 200, 20), message);
				}
			}
		}
	}

}

