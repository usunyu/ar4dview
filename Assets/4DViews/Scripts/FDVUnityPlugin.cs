
//******* DEVELOPER DOCUMENTATION *******//
/*
This script implements the FDVUnityPlugin class. It has to
be attached to the gameobject contained in the 4DViews prefab.

Here is a short description about the available functionalities provided by 
this class for controlling the 4D sequence playback :

- string _sequenceName
Name of the directory containing the 4D sequence data. For example, if the data is stored
in the /SD_CARD/4DV_Sequences/My_Sequence1/DXT1/ then _sequenceName="My_Sequence1"

- bool _dataInStreamingAssets
Set true when the data is stored in the streaming assets folder of the Unity project.
Set false when the data is stored externally to the project.

- string _mainDataPath
    * In the case of data stored in the streamingAssets folder, 
    _mainDataPath is the path from the streamingAssets folder to 
    the sequence directory. For example, if a sequence is stored in 
    Assets/StreamingAssets/My_4DV_Sequences/My_Sequence1/DXT1 then
    _sequenceName="My_Sequence1" _dataInStreamingAssets=true and 
    _mainDataPath="My_4DV_Sequences"
    This value can be null.

    * In the case of data stored in an external folder, 
    _mainDataPath is absolute or relative path to 
    the sequence directory. For example, if a sequence is stored in 
    /SDCARD/My_4DV_Sequences/My_Sequence1/DXT1 then
    _sequenceName="My_Sequence1" _dataInStreamingAssets=false and 
    _mainDataPath="/SDCARD/My_4DV_Sequences"

- List<string> _alternativeDataPaths
    * Alternative paths the script will look in to try to find the sequence data if 
    it can not be found in the _mainDataPath. Useful for specific plateform behaviors.
 

- bool _autoPlay 
    * if true, the sequence will automatically start playback when the scene is loaded.
    * This property is visible from the Editor interface

- bool _computeNormals 
    * If true, the mesh normals per vertex will be computed at each frame. 
    * As it adds some computation, it can have an impact on performances, 
    but result is better than what Unity does by default.
    * This property is visible from the Editor interface
 
  
- void Initialize()
    * Initializes the 4D sequence player. This function has to be called once and the _sequenceName, 
    _mainDataPath, _alternativeDataPaths and _dataInStreamingAssets have to be defined.

- void Uninitialize()
    * Destroies The 4D sequence player. This function is automatically called during the destruction 
    of the gameObject. It can be useful to call this function before the gameObject destruction 
    when memory needs to be freed.

- void Play(bool on)
    * Play/Pause function for controlling the playback of the 4D sequence.

- void GoToFrame(int frameId)
    * Pauses the playback and displays the model of the frame 'frameId'.
    * Playback will resume at this frame.
    * The frame id argument must be in range [0 nbFrames-1]

- int GetCurrentFrame()
    * Returns the current played frame id.

- int GetSequenceNbFrames()
    * Returns the number of frames of the 4D sequence
    
- int GetActiveNbFrames()
    * Returns the number of frames of the active sub-range

- float GetFrameRate()
    * Returns the 4D sequence frame rate. 
    * Note that this value is the frame rate of the sequence. Depending on the device performances, the playback
    frame rate may be lower.

- void SetOutRangeMode (OUT_RANGE_MODE mode)
    * Specifies the behavior of the player when the playbacks ends.
    * This behavior can be modified during the playback.
    * Default is Loop.

- int GetFirstActiveFrameId()
    * Returns the id of the first frame of the active range

- int _bufferMode & int _bufferSize  
    * Specifies the player buffering mode and buffer size. This buffer allows the player to start to read and decode
    the next frames during the playback. For example, with a buffer size of 10, if the current frame is 25, 
    then the player starts to read and decode the frames contained in the range [26 35]. It allows to have a 
    smoother playback by using several threads.
        * mode 0 = buffer_none. The buffer mechanism is disabled.
        * mode 1 = buffer_raw. It reads the next frames without decoding them.
        * mode 2 = buffer_decoded. The next frames are read and decoded.
    * These different modes impact the CPU and memory usage and have to be tuned depending on your application.
    * By default _bufferMode=2 and _bufferSize=10.
    * Theses options can be only modified before Initialize()


- int _cachingMode
    * Specifies if the played frames are stored in memory. It allows to replay a sequence without using too much CPU
    and disk transfert. 
        * mode 0 = cache_none. The cache mechanism is disabled.
        * mode 1 = cache_raw. It stores the raw frames (less memory than mode 2)
        * mode 2 = cache_decoded. It stores the decoded frames (less CPU than mode 1)
    * These different modes impact the CPU and memory usage and have to be tuned depending on your application.
    * By default _cacheMode=0.
    * This option can be only modified before Initialize()



*/

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WSA
#define USE_NATIVE_LIB
#endif


using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif


public enum OUT_RANGE_MODE
{
    Loop = 0,
    Reverse = 1,
    Stop = 2,
    Hide = 3
}


public enum SOURCE_TYPE
{
	Files = 0,
	Network = 1
}

public class FDVUnityPlugin : MonoBehaviour
{
    //-----------------------------//
    //- Class members declaration -//
    //-----------------------------//


    //Events
    public delegate void EventFDV();
    public event EventFDV onNewModel;
    public event EventFDV onModelNotFound;
    public event EventFDV onOutOfRange;


	//Data source
	public SOURCE_TYPE _sourceType = SOURCE_TYPE.Files;

    //Path containing the 4DR data (edited in the unity editor panel)
    public string _sequenceName;
    public string _mainDataPath;
    public List<string> _alternativeDataPaths = new List<string>();
    public bool _dataInStreamingAssets = false;

	//Network configuration
	public string _serverAddress = "127.0.0.1";
	public int    _serverPort = 4444;

    //Playback
    public bool _autoPlay = true;
    public OUT_RANGE_MODE _outRangeMode=OUT_RANGE_MODE.Loop;

    //Normals computing
    public bool _computeNormals = false;

    //Buffer & Cache
    public int _bufferMode = 2;
    public int _bufferSize = 10;
    public int _cachingMode = 0;

    //Active Range
    public float _activeRangeMin = 0;
    public float _activeRangeMax = 0;

    //Infos
    public bool _debugInfo = false;
    private float _decodingFPS = 0f;
    private int _lastDecodingId = 0;
    private System.DateTime _lastDecodingTime;
    private float _updatingFPS = 0f;
    private int _lastUpdatingId = 0;
    private System.DateTime _lastUpdatingTime;
    private int _playCurrentFrame = 0;
    private System.DateTime _playDate;

    //4D source
    private FDVDataSource _dataSource = null;
    private int _lastModelId = -1;

    //Mesh and texture objects
    private Mesh[] _meshes;
    private Texture2D[] _textures;
    private MeshFilter _meshComponent;
    private Renderer _rendererComponent;

    //Receiving geometry and texture buffers
    private Vector3[] _newVertices;
    private Vector2[] _newUVs;
    private int[] _newTriangles;
    private byte[] _newTextureData;
    private Vector3[] _newNormals = null;
    private GCHandle _newVerticesHandle;
    private GCHandle _newUVsHandle;
    private GCHandle _newTrianglesHandle;
    private GCHandle _newTextureDataHandle;
    private GCHandle _newNormalsHandle;

    //Mesh and texture multi-buffering (optimization)
    private int _nbGeometryBuffers = 2;
    private int _currentGeometryBuffer;
    private const int _nbTextureBuffers = 2;
    private int _currentTextureBuffer;

    //time a latest update
    //private float           _prevUpdateTime=0.0f;
    private bool _newMeshAvailable = false;
    private bool _isSequenceTriggerON = false;
    private float _triggerRate = 0.3f;

    //pointer to the mesh Collider, if present (=> will update it at each frames for collisions)
    private MeshCollider _meshCollider;

    //Has the plugin been initialized
    public bool _isInitialized = false;
    private bool _isPlaying = false;
#if UNITY_EDITOR
    private bool _lastEditorMode = false;
#endif
    public int _previewFrame = 0;
    public System.DateTime last_preview_time = System.DateTime.Now;
    private int _pausedFrame;
    public int _nbFrames = 0;

    //Submesh (for models exceeding 65535 vertices)
    private GameObject _submeshGameObject = null;
    private MeshFilter _meshComponentSubMesh;
    private Renderer _rendererComponentSubMesh;
    //private MeshCollider _meshColliderSubMesh;
    private Vector3[] _newVerticesSubMesh = null;
    private Vector2[] _newUVsSubMesh = null;
    private int[] _newTrianglesSubMesh = null;
    private Vector3[] _newNormalsSubMesh = null;
    private GCHandle _newVerticesHandleSubMesh;
    private GCHandle _newUVsHandleSubMesh;
    private GCHandle _newTrianglesHandleSubMesh;
    private GCHandle _newNormalsHandleSubMesh;

    private int _nbVertices;
    private int _nbTriangles;

    private const int MAX_SHORT = 65535;

    //-----------------------------//
    //- Class methods implement.  -//
    //-----------------------------//


    void Awake()
    {
        if ((_sourceType == SOURCE_TYPE.Files && _sequenceName != "") ||
			(_sourceType == SOURCE_TYPE.Network && _serverAddress != ""))
            Initialize();
		//Hide preview mesh
		if(_meshComponent != null) 
			_meshComponent.mesh = null;
		if (_meshComponentSubMesh != null)
			_meshComponentSubMesh.mesh = null;
#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
#endif
    }
    

    public void Initialize()
    {
        //Intialize already called successfully
        if (_isInitialized == true)
            return;

		if (_dataSource == null) {
			if (_sourceType == SOURCE_TYPE.Network) {
				//Creates data source from server ip
				_dataSource = FDVDataSource.CreateNetworkSource (_serverAddress, _serverPort);
			} else {
				//Creates data source from the given path (directory or sequence.xml)
				_dataSource = FDVDataSource.CreateDataSource (_sequenceName, _dataInStreamingAssets, _mainDataPath, _alternativeDataPaths, (int)_activeRangeMin, (int)_activeRangeMax, _outRangeMode);
			}
			if (_dataSource == null) {
				if (onModelNotFound != null)
					onModelNotFound ();
				return;
			}
		}

        _lastModelId = -1;

        _meshComponent = GetComponent<MeshFilter>();
        _rendererComponent = GetComponent<Renderer>();
        _meshCollider = GetComponent<MeshCollider>();
        _meshComponentSubMesh = null;
        _rendererComponentSubMesh = null;
        //_meshColliderSubMesh = null;


        //Allocates geometry buffers
        AllocateGeometryBuffers(ref _newVertices, ref _newUVs, ref _newNormals, ref _newTriangles,
                                ref _newVerticesSubMesh, ref _newUVsSubMesh, ref _newNormalsSubMesh, ref _newTrianglesSubMesh,
                                _dataSource.MaxVertices, _dataSource.MaxTriangles);
        if (_dataSource.MaxVertices > MAX_SHORT)
            InstantiateSubMesh();




        //Allocates texture pixel buffer
        int pixelBufferSize = _dataSource.TextureSize * _dataSource.TextureSize / 2;
        if (_dataSource.TextureFormat == TextureFormat.PVRTC_RGB2 || _dataSource.TextureFormat == TextureFormat.ASTC_RGB_8x8)  //TODO: gérer les futures cas de format
            pixelBufferSize /= 2;
        _newTextureData = new byte[pixelBufferSize];

        //Gets pinned memory handle
#if USE_NATIVE_LIB
        _newVerticesHandle = GCHandle.Alloc(_newVertices, GCHandleType.Pinned);
        _newUVsHandle = GCHandle.Alloc(_newUVs, GCHandleType.Pinned);
        _newTrianglesHandle = GCHandle.Alloc(_newTriangles, GCHandleType.Pinned);
        _newTextureDataHandle = GCHandle.Alloc(_newTextureData, GCHandleType.Pinned);
        System.IntPtr normalAddr = System.IntPtr.Zero;
        if (_computeNormals)
        {
            _newNormalsHandle = GCHandle.Alloc(_newNormals, GCHandleType.Pinned);
            normalAddr = _newNormalsHandle.AddrOfPinnedObject();
        }

        //Gives buffers addresses to the plugin
        FDVUnityBridge.SetReceivingBuffers(_dataSource.FDVUUID,
                                            _newVerticesHandle.AddrOfPinnedObject(),
                                            _newUVsHandle.AddrOfPinnedObject(),
                                            _newTrianglesHandle.AddrOfPinnedObject(),
                                            _newTextureDataHandle.AddrOfPinnedObject(),
                                            normalAddr);



        //Set submesh buffers to the plugin if needed
        if (_submeshGameObject != null)
        {
            _newVerticesHandleSubMesh = GCHandle.Alloc(_newVerticesSubMesh, GCHandleType.Pinned);
            _newUVsHandleSubMesh = GCHandle.Alloc(_newUVsSubMesh, GCHandleType.Pinned);
            _newTrianglesHandleSubMesh = GCHandle.Alloc(_newTrianglesSubMesh, GCHandleType.Pinned);
            System.IntPtr normalAddr2 = System.IntPtr.Zero;
            if (_computeNormals)
            {
                _newNormalsHandleSubMesh = GCHandle.Alloc(_newNormalsSubMesh, GCHandleType.Pinned);
                normalAddr2 = _newNormalsHandleSubMesh.AddrOfPinnedObject();
            }

            //Gives buffers addresses to the plugin
            FDVUnityBridge.SetReceivingBuffersSubMesh(_dataSource.FDVUUID,
                                                _newVerticesHandleSubMesh.AddrOfPinnedObject(),
                                                _newUVsHandleSubMesh.AddrOfPinnedObject(),
                                                _newTrianglesHandleSubMesh.AddrOfPinnedObject(),
                                                normalAddr2);
            _nbGeometryBuffers *= 2;
        }
#else

		FDVUnityBridge.SetReceivingBuffers(_dataSource.FDVUUID, ref _newVertices, ref _newUVs, ref _newTriangles, ref _newNormals, ref _newTextureData);
        if (_dataSource.MaxVertices > MAX_SHORT)
        {
            FDVUnityBridge.SetReceivingBuffersSubMesh(_dataSource.FDVUUID, ref _newVerticesSubMesh, ref _newUVsSubMesh, ref _newTrianglesSubMesh, ref _newNormalsSubMesh);
            _nbGeometryBuffers *= 2;
        }
#endif

        //Allocates objects buffers for double buffering
        _meshes = new Mesh[_nbGeometryBuffers];
        _textures = new Texture2D[_nbTextureBuffers];

        for (int i = 0; i < _nbGeometryBuffers; i++)
        {
            //Mesh
            Mesh mesh = new Mesh();
            mesh.MarkDynamic(); //Optimize mesh for frequent updates. Call this before assigning vertices. 
            if (_submeshGameObject && i % 2 == 1)
            {
                mesh.vertices = _newVerticesSubMesh;
                mesh.uv = _newUVsSubMesh;
                mesh.triangles = _newTrianglesSubMesh;
                if (_computeNormals)
                    mesh.normals = _newNormalsSubMesh;
            }
            else
            {
                mesh.vertices = _newVertices;
                mesh.uv = _newUVs;
                mesh.triangles = _newTriangles;
                if (_computeNormals)
                    mesh.normals = _newNormals;
            }
            Bounds newBounds = mesh.bounds;
            newBounds.extents = new Vector3(10, 10, 10);
            mesh.bounds = newBounds;
            _meshes[i] = mesh;
        }


        for (int i = 0; i < _nbTextureBuffers; i++)
        {
            //Texture
            Texture2D texture = new Texture2D(_dataSource.TextureSize, _dataSource.TextureSize, _dataSource.TextureFormat, false);
            texture.Apply(); //upload to GPU
            _textures[i] = texture;
        }
        
        FDVUnityBridge.SetBufferingMode(_dataSource.FDVUUID, _bufferMode, _bufferSize);

        FDVUnityBridge.SetCachingMode(_dataSource.FDVUUID, _cachingMode);

        _currentGeometryBuffer = _currentTextureBuffer = 0;

        if (_autoPlay)
            Play(true);

        _isInitialized = true;
    }


    public void Uninitialize()
    {
        if (_dataSource == null)
            return;

        StopCoroutine("SequenceTrigger");

        //Releases sequence
        FDVUnityBridge.DestroySequence(_dataSource.FDVUUID);
        _dataSource = null;

        //Releases memory
        _newVerticesHandle.Free();
        _newUVsHandle.Free();
        _newTrianglesHandle.Free();
        _newTextureDataHandle.Free();
        if (_computeNormals)
            _newNormalsHandle.Free();

        _newVerticesHandleSubMesh.Free();
        _newUVsHandleSubMesh.Free();
        _newTrianglesHandleSubMesh.Free();
        if (_computeNormals)
            _newNormalsHandleSubMesh.Free();


        for (int i = 0; i < _nbGeometryBuffers; i++)
            Destroy(_meshes[i]);
        _meshes = null;
        for (int i = 0; i < _nbTextureBuffers; i++)
            Destroy(_textures[i]);
        _textures = null;

        _newVertices = null;
        _newUVs = null;
        _newTriangles = null;
        _newNormals = null;
        _newVerticesSubMesh = null;
        _newUVsSubMesh = null;
        _newTrianglesSubMesh = null;
        _newNormalsSubMesh = null;
        _newTextureData = null;

        _isSequenceTriggerON = false;
        _isInitialized = false;

#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged;
#endif
    }


    void OnDestroy()
    {
        Uninitialize();
    }



    void Start()
	{
        if (_isInitialized == false && 
			((_sourceType == SOURCE_TYPE.Files && _sequenceName != "") || 
				(_sourceType == SOURCE_TYPE.Network && _serverAddress != "")))	//recall initialize if it was not succsefull yet (webGL)
            Initialize();

        if (_dataSource == null)
            return;

        //launch sequence play
        if (_autoPlay)
        {
            Play(true);
        }

        //init time
        //_prevUpdateTime = Time.realtimeSinceStartup;

        //Start coroutine for givin unity time to the plugin
        //yield return StartCoroutine ("SequenceTrigger");
    }



    //Called every frame
    //Get the geometry from the plugin and update the unity gameobject mesh and texture
    void Update()
    {
		if (_isInitialized == false && 
			((_sourceType == SOURCE_TYPE.Files && _sequenceName != "") || 
				(_sourceType == SOURCE_TYPE.Network && _serverAddress != "")))	//recall initialize if it was not succsefull yet (webGL)
			Initialize();

        if (_dataSource == null)
            return;
        //everything is in UpdateMesh(), which called by the SequenceTrigger coroutine

        if (_newMeshAvailable)
        {
            //Get current object buffers (double buffering)
            Mesh mesh = _meshes[_currentGeometryBuffer];
            Mesh submesh = null;
            Texture2D texture = _textures[_currentTextureBuffer];

            //Optimize mesh for frequent updates. Call this before assigning vertices.
            //Seems to be useless :(
            mesh.MarkDynamic();

            //Update geometry
            mesh.vertices = _newVertices;
            mesh.uv = _newUVs;
            mesh.triangles = _newTriangles;
            if (_computeNormals)
                mesh.normals = _newNormals;
            else
                mesh.normals = null;
            mesh.UploadMeshData(false); //Good optimization ! nbGeometryBuffers must be = 1

            //Update submesh
            if (_submeshGameObject != null)
            {
                _currentGeometryBuffer = (_currentGeometryBuffer + 1) % _nbGeometryBuffers;
                if (_nbVertices > MAX_SHORT)
                {
                    submesh = _meshes[_currentGeometryBuffer];
                    submesh.MarkDynamic();
                    submesh.vertices = _newVerticesSubMesh;
                    submesh.uv = _newUVsSubMesh;
                    submesh.triangles = _newTrianglesSubMesh;
                    if (_computeNormals)
                        submesh.normals = _newNormalsSubMesh;
                    submesh.UploadMeshData(false);
                }
            }

            //Update texture
            texture.LoadRawTextureData(_newTextureData);
            texture.Apply();

            //Assign current mesh buffers and texture
            _meshComponent.sharedMesh = mesh;
            _rendererComponent.material.mainTexture = texture;

            if (_submeshGameObject != null)
            {
                _meshComponentSubMesh.sharedMesh = submesh;
                _rendererComponentSubMesh.material = _rendererComponent.material;
            }

            //Switch buffers
            _currentGeometryBuffer = (_currentGeometryBuffer + 1) % _nbGeometryBuffers;
            _currentTextureBuffer = (_currentTextureBuffer + 1) % _nbTextureBuffers;

            //Send event
            if (onNewModel != null)
                onNewModel();

            _newMeshAvailable = false;

            //TODO: mesh collider du submesh
			if (_meshCollider && _meshCollider.enabled)
                _meshCollider.sharedMesh = mesh;
            //_updateCollider = !_updateCollider;

            if (_debugInfo)
            {
                double timeInMSeconds = System.DateTime.Now.Subtract(_lastUpdatingTime).TotalMilliseconds;
                _lastUpdatingId++;
                if (timeInMSeconds > 500f)
                {
                    _updatingFPS = (float)((float)(_lastUpdatingId) / timeInMSeconds * 1000f);
                    _lastUpdatingTime = System.DateTime.Now;
                    _lastUpdatingId = 0;
                }
            }
        }
    }


    private void UpdateMesh()
    {
        if (_dataSource == null)
            return;

        //Get the new model
#if USE_NATIVE_LIB
        System.IntPtr normalAddr = System.IntPtr.Zero;
        if (_computeNormals)
        {
            if (_newNormals == null)
            {
                _newNormals = new Vector3[_dataSource.MaxVertices];
                _newNormalsHandle = GCHandle.Alloc(_newNormals, GCHandleType.Pinned);
            }
            normalAddr = _newNormalsHandle.AddrOfPinnedObject();
        }

        System.IntPtr uvAddrSubMesh = System.IntPtr.Zero;
        System.IntPtr vertAddrSubMesh = System.IntPtr.Zero;
        System.IntPtr triAddrSubMesh = System.IntPtr.Zero;
        System.IntPtr normalAddrSubMesh = System.IntPtr.Zero;
        if (_submeshGameObject != null)
        {
            vertAddrSubMesh = _newVerticesHandleSubMesh.AddrOfPinnedObject();
            uvAddrSubMesh = _newUVsHandleSubMesh.AddrOfPinnedObject();
            triAddrSubMesh = _newTrianglesHandleSubMesh.AddrOfPinnedObject();
            if (_computeNormals)
            {
                if (_newNormalsSubMesh == null)
                {
                    _newNormalsSubMesh = new Vector3[_dataSource.MaxVertices - MAX_SHORT];
                    _newNormalsHandleSubMesh = GCHandle.Alloc(_newNormalsSubMesh, GCHandleType.Pinned);
                }
                normalAddrSubMesh = _newNormalsHandleSubMesh.AddrOfPinnedObject();
            }
        }
        
        int modelId = FDVUnityBridge.UpdateModel(_dataSource.FDVUUID,
                                                  _newVerticesHandle.AddrOfPinnedObject(),
                                                  _newUVsHandle.AddrOfPinnedObject(),
                                                  _newTrianglesHandle.AddrOfPinnedObject(),
                                                  _newTextureDataHandle.AddrOfPinnedObject(),
                                                  normalAddr,
                                                  vertAddrSubMesh,
                                                  uvAddrSubMesh,
                                                  triAddrSubMesh,
                                                  normalAddrSubMesh,
                                                  ref _nbVertices,
                                                  ref _nbTriangles);

#else
		int modelId = FDVUnityBridge.UpdateModel (_dataSource.FDVUUID,
												  ref _nbVertices,
												  ref _nbTriangles);
#endif

        //look for end of range event
        if (FDVUnityBridge.OutOfRangeEvent(_dataSource.FDVUUID))
        {   //Send event
            if (onOutOfRange != null)
                onOutOfRange();

            if (_outRangeMode == OUT_RANGE_MODE.Hide)
            {
                Play(false);
                _meshComponent.mesh = null;
            }
        }

        //Check if there is model
        if (!_newMeshAvailable)
            _newMeshAvailable = (modelId != -1 && modelId != _lastModelId);

        if (modelId == -1) modelId = _lastModelId;

        if (_debugInfo)
        {
            double timeInMSeconds = System.DateTime.Now.Subtract(_lastDecodingTime).TotalMilliseconds;
            if (_lastDecodingId == 0 || timeInMSeconds > 500f)
            {
                _decodingFPS = (float)((double)(Mathf.Abs((float)(modelId - _lastDecodingId))) / timeInMSeconds) * 1000f;
                _lastDecodingTime = System.DateTime.Now;
                _lastDecodingId = modelId;
            }
        }

        _lastModelId = modelId;

        //Todo: do something when mesh is empty
        if (_nbVertices < 0)
            return;
    }


    //manage the UpdateMesh() call to have it triggered by the sequence framerate
    private IEnumerator SequenceTrigger()
    {
        float duration = (_triggerRate / _dataSource.FrameRate);

        //infinite loop to keep executing this coroutine
        while (true)
        {
            //do nothing while the elapsed time is below the frame duration
            //while (Time.realtimeSinceStartup - _prevUpdateTime < duration)
            //yield return null;

            //_prevUpdateTime = Time.realtimeSinceStartup;

            UpdateMesh();
            yield return new WaitForSeconds(duration);
        }
    }

    //set the sequence on pause if the application looses the focus
    void OnApplicationPause(bool pauseStatus)
    {
		PlayOnFocus (!pauseStatus);
    }

	void OnEnable() {
		PlayOnFocus (true);
	}

	void OnDisable() {
		PlayOnFocus (false);
	}

	void PlayOnFocus(bool on) {

		if (on && _isPlaying)
		{
			if (_isSequenceTriggerON == false)
			{
				FDVUnityBridge.Play(_dataSource.FDVUUID, on);
				StartCoroutine("SequenceTrigger");
				_isSequenceTriggerON = true;
			}
		}
		else
		{
			if (_isSequenceTriggerON == true)
			{
				FDVUnityBridge.Play(_dataSource.FDVUUID, on);
				StopCoroutine("SequenceTrigger");
				_isSequenceTriggerON = false;
			}
		}
	}

#if UNITY_EDITOR
    void HandleOnPlayModeChanged()
    {
        if (EditorApplication.isPaused && _lastEditorMode)
        {
            _pausedFrame++;
            bool isCurrentlyPlaying = _isPlaying;
            GotoFrame(_pausedFrame % GetSequenceNbFrames()); //GotoFrame pauses automatically the playback
            _isPlaying = isCurrentlyPlaying;                 //so we need to restore the playback mode
            Debug.Log("CURRENT FRAME " + GetCurrentFrame());
        }
        else
        {
            _pausedFrame = GetCurrentFrame();
            OnApplicationPause(EditorApplication.isPaused);
            _lastEditorMode = EditorApplication.isPaused;
        }
    }
#endif


    //Public functions
    public void Play(bool on)
    {
        if (on)
        {
            if (_isSequenceTriggerON == false)
            {
                FDVUnityBridge.Play(_dataSource.FDVUUID, on);
                StartCoroutine("SequenceTrigger");
                _isSequenceTriggerON = true;
                _playCurrentFrame = GetCurrentFrame();
                _playDate = System.DateTime.Now;
            }
        }
        else
        {
            if (_isSequenceTriggerON == true)
            {
                FDVUnityBridge.Play(_dataSource.FDVUUID, on);
                StopCoroutine("SequenceTrigger");
                _isSequenceTriggerON = false;
            }
        }
        _isPlaying = on;
    }

    public bool IsPlaying()
    {
        return _isPlaying;
    }

    public void GotoFrame(int frame)
    {
        FDVUnityBridge.GotoFrame(_dataSource.FDVUUID, frame);
        _isPlaying = false;
        UpdateMesh();
    }

    public int GetFirstIndex()
    {
        return FDVUnityBridge.GetSequenceFirstIndex(_dataSource.FDVUUID);
    }

    public int GetSequenceNbFrames()
    {
        return FDVUnityBridge.GetSequenceNbFrames(_dataSource.FDVUUID);
    }

	public int GetActiveNbFrames()
	{
		return (int)_activeRangeMax - (int)_activeRangeMin + 1;
	}

    public int GetCurrentFrame()
    {
        if (_lastModelId < 0)
            return 0;
        else
            return _lastModelId;//(_lastModelId - GetFirstIndex ());
        //return FDVUnityBridge.GetSequenceCurrentFrame (_dataSource.FDVUUID);
    }

    public float GetFrameRate()
    {
        return (_dataSource == null) ? 0.0f : _dataSource.FrameRate;
    }

    public void SetOutRangeMode(OUT_RANGE_MODE mode)
    {
        FDVUnityBridge.ChangeOutRangeMode(_dataSource.FDVUUID, mode);
    }

	public int GetFirstActiveFrameId() {
		return (int)_activeRangeMin;
	}

    public TextureFormat GetTextureFormat()
    {
        return _dataSource.TextureFormat;
    }


    void OnGUI()
    {
        if (_debugInfo)
        {
            double delay = System.DateTime.Now.Subtract(_playDate).TotalMilliseconds - ((float)(GetCurrentFrame() - _playCurrentFrame) * 1000 / GetFrameRate());
            string decoding = _decodingFPS.ToString("00.00") + " fps";
            string updating = _updatingFPS.ToString("00.00") + " fps";
            delay /= 1000;
            if (!_isPlaying)
            {
                delay = 0f;
                decoding = "paused";
                updating = "paused";
            }
            int top = 20;
            GUIStyle title = new GUIStyle();
            title.normal.textColor = Color.white;
            title.fontStyle = FontStyle.Bold;
            GUI.Button(new Rect(Screen.width - 210, top - 10, 200, 330), "");
            GUI.Label(new Rect(Screen.width - 200, top, 190, 20), "Sequence ", title);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Length: " + ((float)GetSequenceNbFrames() / GetFrameRate()).ToString("00.00") + " sec");
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb Frames: " + GetSequenceNbFrames() + " frames");
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Frame rate: " + GetFrameRate().ToString("00.00") + " fps");
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Max submeshes: " + ((_submeshGameObject == null) ? 1 : 2));
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Max vertices: " + _dataSource.MaxVertices);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Max triangles: " + _dataSource.MaxTriangles);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Texture format: " + _dataSource.TextureFormat);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Texture size: " + _dataSource.TextureSize + "x" + _dataSource.TextureSize + "px");
            GUI.Label(new Rect(Screen.width - 200, top += 25, 190, 20), "Current Mesh", title);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb submeshes: " + ((_nbVertices <= MAX_SHORT) ? 1 : 2));
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb vertices: " + _nbVertices);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb triangles: " + _nbTriangles);
            GUI.Label(new Rect(Screen.width - 200, top += 25, 190, 20), "Playback", title);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Time: " + ((float)(GetCurrentFrame()) / GetFrameRate()).ToString("00.00") + " sec");
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Frame Id / File Id: " + GetCurrentFrame() + "/" + (GetCurrentFrame() + GetFirstIndex()));
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Decoding rate: " + decoding);
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Decoding delay: " + delay.ToString("00.00") + " sec");
            GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Updating rate: " + updating);
        }
    }


    public void Preview()
    {
#if UNITY_EDITOR
        _meshComponent = GetComponent<MeshFilter>();
        _rendererComponent = GetComponent<Renderer>();

		if (_sequenceName == "" || _sourceType != SOURCE_TYPE.Files)
            return;

        string rootpath;
        string dataPath = "";

        //---- get path
        if (_dataInStreamingAssets)
            rootpath = Application.streamingAssetsPath + "/" + _mainDataPath + "/" + _sequenceName;
        else
            rootpath = _mainDataPath + "/" + _sequenceName;

        //TODO gerer alternative paths
        DirectoryInfo rootDirectoryInfo = new DirectoryInfo(rootpath);
        FileInfo rootFileInfo = new FileInfo(rootpath);
        TextureFormat textureFormat = TextureFormat.DXT1;
        if (rootDirectoryInfo != null && Directory.Exists(rootpath))
        { //if path to a directory
            if (!Directory.Exists(rootpath + "/" + textureFormat.ToString()) && Directory.Exists(rootpath + "/ETC_RGB4"))
                textureFormat = TextureFormat.ETC_RGB4;
            else if (!Directory.Exists(rootpath + "/" + textureFormat.ToString()) && Directory.Exists(rootpath + "/PVRTC_RGB2"))
                textureFormat = TextureFormat.PVRTC_RGB2;

            dataPath = rootpath + "/" + textureFormat.ToString() + "/sequence.xml";
        }
        else if (rootFileInfo != null && File.Exists(rootpath))
        { //if path to file
            if (rootFileInfo.Name == "sequence.xml")
                dataPath = rootpath;
        }
        else
        {
            Debug.Log("FDV Warning: source path does not exist : " + rootpath);
            return;
        }


        //----- create manager
        FDVSequenceManager manager4DR = new FDVSequenceManager();
        if (manager4DR.initSequence(dataPath) == false)
        {
            Debug.Log("FDV Warning: couldn't initialize 4DR sequence : " + dataPath);
            return;
        }
        manager4DR.setDecompressGeometry(true);
        _nbFrames = manager4DR.getNbFrames();

		if (_activeRangeMax == 0 || _activeRangeMax >= _nbFrames) _activeRangeMax = _nbFrames - 1;
		if(_activeRangeMin >= _nbFrames) _activeRangeMin = 0;

        //----- update mesh
        Vector3[] verts = null, norms = null, subverts = null, subnorms = null;
        Vector2[] uvs = null, subuvs = null;
        int[] tris = null, subtris = null;
        AllocateGeometryBuffers(ref verts, ref uvs, ref norms, ref tris,
                                ref subverts, ref subuvs, ref subnorms, ref subtris,
                                manager4DR.getMaxNbVertices(), manager4DR.getMaxNbFaces());
        int sizeTex = manager4DR.getTextureSize() * manager4DR.getTextureSize() / 2;
        if (textureFormat == TextureFormat.PVRTC_RGB2)
            sizeTex /= 2;
        byte[] tex = new byte[sizeTex];
        manager4DR.affectBuffers(ref verts, ref uvs, ref tris, ref norms, ref tex);
        //submesh
        if (manager4DR.getMaxNbVertices() > MAX_SHORT)
        {
            InstantiateSubMesh();
            manager4DR.affectBuffersSubMesh(ref subverts, ref subuvs, ref subtris, ref subnorms);
        }

        FDVDecoder4DR decoder4DR = manager4DR.getMeshAtFrame((int)(_previewFrame /* * manager4DR.getNbFrames() / 100.0f */));

        Mesh mesh = new Mesh();
        Texture2D texture = null;
        if (textureFormat == TextureFormat.DXT1)
            texture = new Texture2D(manager4DR.getTextureSize(), manager4DR.getTextureSize(), textureFormat, false);

        //Update geometry
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        if (_computeNormals)
        {
            if (!decoder4DR.m_mesh.m_containsNormals)
                FDVUnityBridge.ComputeNormals(ref decoder4DR.m_mesh.m_uncompressedVertices, ref decoder4DR.m_mesh.m_facesBuffer,
                                              decoder4DR.m_mesh.m_nbVertices, decoder4DR.m_mesh.m_nbFaces, ref decoder4DR.m_mesh.m_normalsBuffer);
            mesh.normals = norms;
        }

        if (manager4DR.getMaxNbVertices() > MAX_SHORT)
        {
            Mesh submesh = new Mesh();
            submesh.vertices = subverts;
            submesh.uv = subuvs;
            submesh.triangles = subtris;
            if (_computeNormals && decoder4DR.m_mesh.m_containsNormals)
                submesh.normals = subnorms;
            _meshComponentSubMesh.sharedMesh = submesh;
        }

        //Update texture
        if (texture)
        {
            texture.LoadRawTextureData(tex);
            texture.Apply();
        }

        //Assign current mesh buffers and texture
        _meshComponent.sharedMesh = mesh;
        var tempMaterial = new Material(_rendererComponent.sharedMaterial);
        tempMaterial.mainTexture = texture;
        _rendererComponent.sharedMaterial = tempMaterial;
        if (_rendererComponentSubMesh)
            _rendererComponentSubMesh.sharedMaterial = _rendererComponent.sharedMaterial;
#endif //UNITY_EDITOR
	}


    public void ConvertPreviewTexture()
    {
        System.DateTime current_time = System.DateTime.Now;
		if(_rendererComponent != null && _rendererComponent.sharedMaterial.mainTexture != null) {
	        if (((System.TimeSpan)(current_time - last_preview_time)).TotalMilliseconds < 1000 
	            || ((Texture2D)_rendererComponent.sharedMaterial.mainTexture).format == TextureFormat.RGBA32)
	            return;

	        last_preview_time = current_time;

	        if (_rendererComponent != null)
	        {
	            Texture2D tex = (Texture2D)_rendererComponent.sharedMaterial.mainTexture;
	            if (tex && tex.format != TextureFormat.RGBA32)
	            {
	                Color32[] pix = tex.GetPixels32();
	                Texture2D textureRGBA = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
	                textureRGBA.SetPixels32(pix);
	                textureRGBA.Apply();

	                _rendererComponent.sharedMaterial.mainTexture = textureRGBA;
	                if (_rendererComponentSubMesh)
	                    _rendererComponentSubMesh.sharedMaterial.mainTexture = textureRGBA;
	            }
	        }
    	}
	}


    private void AllocateGeometryBuffers(ref Vector3[] verts, ref Vector2[] uvs, ref Vector3[] norms, ref int[] tris,
                                     ref Vector3[] subverts, ref Vector2[] subuvs, ref Vector3[] subnorms, ref int[] subtris,
                                     int nbMaxVerts, int nbMaxTris)
    {
        int size1 = nbMaxVerts, size2 = 0;
        if (size1 > MAX_SHORT)
        {
            size1 = MAX_SHORT;
            size2 = nbMaxVerts - MAX_SHORT;
        }

        verts = new Vector3[size1];
        uvs = new Vector2[size1];
        tris = new int[nbMaxTris * 3];
        norms = null;
        if (_computeNormals)
            norms = new Vector3[size1];

        //submesh
        if (nbMaxVerts > MAX_SHORT)
        {
            InstantiateSubMesh();
            subverts = new Vector3[size2];
            subuvs = new Vector2[size2];
            subtris = new int[nbMaxTris * 3];
            subnorms = null;
            if (_computeNormals)
                subnorms = new Vector3[size2];
        }
    }


    private void InstantiateSubMesh()
    {
        Transform child = transform.Find("submesh");
        if (child != null)
        {
            _submeshGameObject = child.gameObject;
        }
        else
        {
            _submeshGameObject = new GameObject("submesh");
            _submeshGameObject.AddComponent<MeshFilter>();
            _submeshGameObject.AddComponent<MeshRenderer>();
            _submeshGameObject.transform.parent = this.transform;
            _submeshGameObject.transform.localRotation = Quaternion.identity;
            _submeshGameObject.transform.localPosition = Vector3.zero;
            _submeshGameObject.transform.localScale = Vector3.one;
        }
        _meshComponentSubMesh = _submeshGameObject.GetComponent<MeshFilter>();
        //_meshColliderSubMesh = _submesh.GetComponent<MeshCollider>();
        _rendererComponentSubMesh = _submeshGameObject.GetComponent<MeshRenderer>();
        _rendererComponentSubMesh.sharedMaterial = _rendererComponent.sharedMaterial;
        _rendererComponentSubMesh.receiveShadows = _rendererComponent.receiveShadows;
        _rendererComponentSubMesh.shadowCastingMode = _rendererComponent.shadowCastingMode;
        _rendererComponentSubMesh.lightProbeUsage = _rendererComponent.lightProbeUsage; //if this line creates an error, comment it and uncomment the next one
        //_rendererComponentSubMesh.useLightProbes = _rendererComponent.useLightProbes;
        _rendererComponentSubMesh.reflectionProbeUsage = _rendererComponent.reflectionProbeUsage;
    }



}






//-----------------FDVDataSource-----------------//
//Creates a 4D sequence from a path.
//If the path is a directory, FDVDataSource looks for the best supported format.
//If path is a sequence.xml file, FDVDataSource creates directly a sequence
//from this file without checking format compatibility.

public class FDVDataSource
{

    public string FDVUUID;
    public TextureFormat TextureFormat;
    public int TextureSize;
    public int MaxVertices;
    public int MaxTriangles;
    public float FrameRate;

    //Static constructor: creates a data source or returns null when no source can be created
    static public FDVDataSource CreateDataSource(string sequenceName, bool dataInStreamingAssets, string mainPath, List<string> alternativePaths, int activeRangeBegin, int activeRangeLastFrame, OUT_RANGE_MODE outRangeMode)
    {
        bool success = false;
        string rootpath;

        if (dataInStreamingAssets)
        {
            rootpath = Application.streamingAssetsPath + "/" + mainPath + "/" + sequenceName;

            FDVDataSource instance = new FDVDataSource(rootpath, activeRangeBegin, activeRangeLastFrame, outRangeMode, ref success);
            if (success)
                return instance;
            else
            {
                Debug.LogError("FDV Error: cannot find data source");
                return null;
            }

        }
        else
        {
            rootpath = mainPath + "/" + sequenceName;
            FDVDataSource instance = new FDVDataSource(rootpath, activeRangeBegin, activeRangeLastFrame, outRangeMode, ref success);
            if (success)
                return instance;
            else
            {
                int pathIndex = 0;
                while (!success && pathIndex < alternativePaths.Count)
                {
                    rootpath = alternativePaths[pathIndex] + "/" + sequenceName;
                    instance = new FDVDataSource(rootpath, activeRangeBegin, activeRangeLastFrame, outRangeMode, ref success);
                    pathIndex++;
                }
                if (success)
                    return instance;
                else
                {
                    Debug.LogError("FDV Error: cannot find data source");
                    return null;
                }
            }
        }

    }

	//Static constructor: creates a data source or returns null when no source can be created
	static public FDVDataSource CreateNetworkSource(string serverip, int serverport)
	{
		bool success = false;

		FDVDataSource instance = new FDVDataSource(serverip, serverport, ref success);
		if (success)
			return instance;
		else
		{
			Debug.LogError("FDV Error: cannot create network source");
			return null;
		}
	}

    //private constructor
    private FDVDataSource(string rootpath, int activeRangeBegin, int activeRangeEnd, OUT_RANGE_MODE outRangeMode, ref bool success)
    {
        this.FDVUUID = "";
        success = false;
        string dataPath = "";

        //Find data source
#if UNITY_WEBGL
        //can't use directory info in webgl, so let's suppose we have a good directory
        List<TextureFormat> subdirsNames = new List<TextureFormat>() { TextureFormat.DXT1, TextureFormat.ETC_RGB4, TextureFormat.PVRTC_RGB2};
        this.TextureFormat = FindTextureFormat(subdirsNames, ref success);
        success = true;
        if (success) {
            dataPath = rootpath + "/" + this.TextureFormat.ToString() + "/sequence.xml";
            FDVDownloaderManager.getFileText(dataPath);
        }
        else
            dataPath = null;
#else
        DirectoryInfo rootDirectoryInfo = new DirectoryInfo(rootpath);
        FileInfo rootFileInfo = new FileInfo(rootpath);
        //IF path starts with "jar", it normally means that the data is in the streaming assets on Android device
        if (rootDirectoryInfo != null && (Directory.Exists(rootpath) || rootpath.StartsWith("jar")) )
        { //if path to a directory
            this.TextureFormat = FindTextureFormat(GetAvailableFormatsAtPath(rootpath), ref success);
            if (success)
            {
                dataPath = rootpath + "/" + this.TextureFormat.ToString() + "/sequence.xml";
            }
            else
            {
                dataPath = null;
                //Debug.LogError ("FDV Error: cannot find data source");
            }
        }
        else if (rootFileInfo != null && File.Exists(rootpath))
        { //if path to file
            if (rootFileInfo.Name == "sequence.xml")
            {
                success = true;
                dataPath = rootpath;
            }
        }
        else
        {
            Debug.Log("FDV Warning: source path does not exist : " + rootpath);
        }
#endif


        //Create sequence with native plugin
        if (success)
        {
#if USE_NATIVE_LIB
            System.IntPtr uuidPtr = FDVUnityBridge.CreateSequence(dataPath, activeRangeBegin, activeRangeEnd, outRangeMode);
            if (uuidPtr != System.IntPtr.Zero)
                this.FDVUUID = Marshal.PtrToStringAnsi(uuidPtr);
#else
            this.FDVUUID = FDVUnityBridge.CreateSequence(dataPath, activeRangeBegin, activeRangeEnd, outRangeMode); 
#endif
            if (this.FDVUUID == null || this.FDVUUID == "undefined" || this.FDVUUID == "")
                success = false;
        }

        //Get sequence info
        if (success)
        {
            this.TextureSize = FDVUnityBridge.GetTextureSize(this.FDVUUID);
            if (this.TextureSize == 0)
                this.TextureSize = 1024;	//put 1024 by default => will crash if we have 2048 texture and it's not written in xml fi
#if USE_NATIVE_LIB
            string textureEncoding = Marshal.PtrToStringAnsi(FDVUnityBridge.GetTextureEncoding(this.FDVUUID));
#else
			string textureEncoding = FDVUnityBridge.GetTextureEncoding (this.FDVUUID);
#endif
            switch (textureEncoding)
            {
                case "ETC1":
                    this.TextureFormat = TextureFormat.ETC_RGB4;
                    break;
                case "PVRTC4":
                    this.TextureFormat = TextureFormat.PVRTC_RGB4;
                    break;
                case "PVRTC2":
                    this.TextureFormat = TextureFormat.PVRTC_RGB2;
                    break;
                case "DXT1":
                    this.TextureFormat = TextureFormat.DXT1;
                    break;
                case "ASTC":
                    this.TextureFormat = TextureFormat.ASTC_RGB_8x8;
                    break;
                default:
#if UNITY_IPHONE
				this.TextureFormat = TextureFormat.PVRTC_RGB4;
#elif UNITY_ANDROID
                    this.TextureFormat = TextureFormat.ETC_RGB4;
#else 
				this.TextureFormat = TextureFormat.DXT1;
#endif
                    break;
            }
            this.MaxVertices = FDVUnityBridge.GetSequenceMaxVertices(this.FDVUUID);
            if (this.MaxVertices == 0)
                this.MaxVertices = 65535;
            this.MaxTriangles = FDVUnityBridge.GetSequenceMaxTriangles(this.FDVUUID);
            if (this.MaxTriangles == 0)
                this.MaxTriangles = 65535;
            this.FrameRate = (float)FDVUnityBridge.GetSequenceFramerate(this.FDVUUID);
        }
    }


	//private constructor
	private FDVDataSource(string serverip, int serverport, ref bool success)
	{
		this.FDVUUID = "";
		success = true;

		//Create network client with native plugin
		System.IntPtr uuidPtr = FDVUnityBridge.CreateConnection(serverip, serverport);
		if (uuidPtr != System.IntPtr.Zero)
			this.FDVUUID = Marshal.PtrToStringAnsi(uuidPtr);
			
		if (this.FDVUUID == "undefined" || this.FDVUUID == "")
			success = false;

		//Get sequence info
		if (success)
		{
			this.TextureSize = FDVUnityBridge.GetTextureSize(this.FDVUUID);
			if (this.TextureSize == 0)
				this.TextureSize = 1024;    //put 1024 by default => will crash if we have 2048 texture and it's not written in xml fi

#if USE_NATIVE_LIB
            string textureEncoding = Marshal.PtrToStringAnsi(FDVUnityBridge.GetTextureEncoding(this.FDVUUID));
#else
			string textureEncoding = FDVUnityBridge.GetTextureEncoding (this.FDVUUID);
#endif
            switch (textureEncoding)
			{
			case "ETC1":
				this.TextureFormat = TextureFormat.ETC_RGB4;
				break;
			case "PVRTC4":
				this.TextureFormat = TextureFormat.PVRTC_RGB4;
				break;
			case "PVRTC2":
				this.TextureFormat = TextureFormat.PVRTC_RGB2;
				break;
			case "DXT1":
				this.TextureFormat = TextureFormat.DXT1;
				break;
			case "ASTC":
				this.TextureFormat = TextureFormat.ASTC_RGB_8x8;
				break;
			default:
				#if UNITY_IPHONE
				this.TextureFormat = TextureFormat.PVRTC_RGB4;
				#elif UNITY_ANDROID
				this.TextureFormat = TextureFormat.ETC_RGB4;
				#else 
				this.TextureFormat = TextureFormat.DXT1;
				#endif
				break;
			}
			this.MaxVertices = FDVUnityBridge.GetSequenceMaxVertices(this.FDVUUID);
			if (this.MaxVertices == 0)
				this.MaxVertices = 65535;
			this.MaxTriangles = FDVUnityBridge.GetSequenceMaxTriangles(this.FDVUUID);
			if (this.MaxTriangles == 0)
				this.MaxTriangles = 65535;
			this.FrameRate = (float)FDVUnityBridge.GetSequenceFramerate(this.FDVUUID);
		}
	}




    //Texture format list, from the best to the worst
    static private List<TextureFormat> _textureFormats = new List<TextureFormat>(7) {
		TextureFormat.ASTC_RGB_8x8,
		TextureFormat.PVRTC_RGB2,
		TextureFormat.PVRTC_RGB4,
		TextureFormat.ETC2_RGB,
		TextureFormat.ETC_RGB4,
		TextureFormat.DXT1,
		TextureFormat.RGB24
	};


    //Finds the best texture format depending on the HW and the data source
    static public TextureFormat FindTextureFormat(List<TextureFormat> availableFormats, ref bool success)
    {
        success = true;

#if UNITY_EDITOR
        return TextureFormat.DXT1;
#else
		
		//Finds formats supported by the HW
		List<TextureFormat> supportedFormats = new List<TextureFormat>();
		foreach (TextureFormat format in _textureFormats) {
			if(SystemInfo.SupportsTextureFormat(format)) {
				supportedFormats.Add (format);
			}
		}

		//Gets the first format of the supported and available lists intersection
		foreach (TextureFormat format in supportedFormats) {
			if(availableFormats.Contains(format)) return format; 
		}
	 
		success = false;
		return TextureFormat.RGB24;		
#endif
    }

    //Finds texture formats which are available in the data source
    static public List<TextureFormat> GetAvailableFormatsAtPath(string path)
    {
        List<TextureFormat> availableFormats = new List<TextureFormat>();

        //Lists all sudirectories contained in the data path
        DirectoryInfo[] subdirs;
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        try
        {
            //IF path starts with "jar", it normally means that the data is in the streaming assets on Android device
            if (path.StartsWith("jar"))
            {
                //for the moment we only check if ETC is present; TODO: find mechanism to check other formats
                WWW www = new WWW(path+"/ETC_RGB4/sequence.xml");
                if (www != null)
                    availableFormats.Add(TextureFormat.ETC_RGB4);
                return availableFormats;
            }
            subdirs = dirInfo.GetDirectories();
        }
        catch
        {
            Debug.LogError("FDV Error: cannot access to data path : " + path);
            return availableFormats;
        }
        List<string> subdirsNames = new List<string>();
        foreach (DirectoryInfo subdir in subdirs)
        {
            subdirsNames.Add(subdir.Name);
        }

        //Compare subdir names to texture format names
        List<string> formatNames = new List<string>();
        foreach (TextureFormat format in _textureFormats)
        {
            formatNames.Add(format.ToString());
        }
        foreach (string subdirName in subdirsNames)
        {
            int formatIndex = formatNames.IndexOf(subdirName);
            if (formatIndex != -1)
            {
                availableFormats.Add(_textureFormats[formatIndex]);
            }
        }
        return availableFormats;
    }
}







