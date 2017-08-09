
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WSA
#define USE_NATIVE_LIB
#endif

using UnityEngine;
using System.Runtime.InteropServices;

//-----------------FDVUnityBridge-----------------//



//Imports the native plugin functions.

public class FDVUnityBridge
{
//    internal static class NativeMethods
//    {

#if USE_NATIVE_LIB


#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Inits the plugin (sequencemanager, etc.)
        public static extern System.IntPtr CreateSequence([MarshalAs(UnmanagedType.LPStr)] string dataPath , int rangeBegin, int rangeEnd, OUT_RANGE_MODE outRangeMode);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
     [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
	//Inits the plugin (networkmanager, etc.)
	public static extern System.IntPtr CreateConnection(string serverip, int serverport);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
    [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Stops the plugin and releases memory (sequencemanager, etc.)
        public static extern void DestroySequence([MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Gives buffer addresses to the plugin
        public static extern void SetReceivingBuffers([MarshalAs(UnmanagedType.LPStr)] string uuid,
                                                      System.IntPtr ptrVertices,
                                                      System.IntPtr ptrUVs,
                                                      System.IntPtr ptrTriangles,
                                                      System.IntPtr texture,
                                                      System.IntPtr normals);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif

        //Gives buffer addresses to the plugin
        public static extern void SetReceivingBuffersSubMesh([MarshalAs(UnmanagedType.LPStr)] string uuid,
                                                      System.IntPtr ptrVertices,
                                                      System.IntPtr ptrUVs,
                                                      System.IntPtr ptrTriangles,
                                                      System.IntPtr normals);
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Starts or stops the playback
        public static extern void Play([MarshalAs(UnmanagedType.LPStr)] string uuid, bool on);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Gets the new model from plugin
        public static extern int UpdateModel([MarshalAs(UnmanagedType.LPStr)] string uuid,
                                              System.IntPtr ptrVertices,
                                              System.IntPtr ptrUVs,
                                              System.IntPtr ptrTriangles,
                                              System.IntPtr texture,
                                              System.IntPtr normals,
                                                System.IntPtr ptrVertices2,
                                              System.IntPtr ptrUVs2,
                                              System.IntPtr ptrTriangles2,
                                              System.IntPtr normals2,
                                              ref int nbVertices,
                                              ref int nbTriangles);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern bool OutOfRangeEvent( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Gets the 4DR texture image size
        public static extern int GetTextureSize( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        //Gets the 4DR texture encoding
        public static extern System.IntPtr GetTextureEncoding( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern int GetSequenceMaxVertices( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern int GetSequenceMaxTriangles( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern float GetSequenceFramerate( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern int GetSequenceFirstIndex( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern int GetSequenceNbFrames( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern int GetSequenceCurrentFrame( [MarshalAs(UnmanagedType.LPStr)] string uuid);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern void GotoFrame( [MarshalAs(UnmanagedType.LPStr)] string uuid, int frame);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern void ChangeOutRangeMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, OUT_RANGE_MODE mode);

#if UNITY_IPHONE && !UNITY_EDITOR
	    [DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern void SetBufferingMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, int mode, int bufferSize); //0 = None ; 1 = Raw ; 2 = Decoded 

#if UNITY_IPHONE && !UNITY_EDITOR
	    [DllImport ("__Internal")]  
#else //Android & Desktop
        [DllImport("FDVUnityBridge", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
#endif
        public static extern void SetCachingMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, int mode); //0 = None ; 1 = Raw ; 2 = Decoded 

    //    } //NativeMethods

#else   //FULL C# 



    static System.Collections.Generic.Dictionary<string, FDVSequenceManager> _sequenceList = null;


    static System.Collections.Generic.Dictionary<string, FDVSequenceManager> GetSequenceList()
    {
        if (_sequenceList == null)
            _sequenceList = new System.Collections.Generic.Dictionary<string, FDVSequenceManager>();

        return _sequenceList;
    }



    //Inits the plugin (sequencemanager, etc.)
    public static string CreateSequence(string dataPath, int rangeBegin, int rangeEnd, OUT_RANGE_MODE outRangeMode)
    {
        System.Guid uuid = System.Guid.NewGuid();

        FDVSequenceManager manager4DR = new FDVSequenceManager();

#if UNITY_WEBGL
    manager4DR.isWebGL = true;
#endif

        if (manager4DR.initSequence(dataPath) == false)
        {
            return "";
        } 

        manager4DR.setDecompressGeometry(true);
        manager4DR.setOutOfRangeMode((int)outRangeMode);
        if (rangeEnd == -1)
    		manager4DR.setActiveRange(0, manager4DR.getNbFrames()-1);
	    else
            manager4DR.setActiveRange(rangeBegin, rangeEnd);

        GetSequenceList()[uuid.ToString()] = manager4DR;

        return uuid.ToString();
    }

    public static System.IntPtr CreateConnection(string serverip, int serverport)
    {
        // NOT supported yet
        return System.IntPtr.Zero;
    }


    //Stops the plugin and releases memory (sequencemanager, etc.)
    public static void DestroySequence( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        GetSequenceList().Remove(uuid);
    }


    //Gives buffer addresses to the plugin  ,       !! VERSION NON UTILISEE
    public static void SetReceivingBuffers( [MarshalAs(UnmanagedType.LPStr)] string uuid,
                                                  System.IntPtr ptrVertices,
                                                  System.IntPtr ptrUVs,
                                                  System.IntPtr ptrTriangles,
                                                  System.IntPtr texture)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        manager4DR.affectBuffers(ptrVertices, ptrUVs, ptrTriangles, texture);
    }

    public static void SetReceivingBuffers( [MarshalAs(UnmanagedType.LPStr)] string uuid,
                                            ref Vector3[] vertices,
                                            ref Vector2[] uvs,
                                            ref int[] triangles,
                                            ref Vector3[] normals,
                                            ref byte[] texture)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        manager4DR.affectBuffers(ref vertices, ref uvs, ref triangles, ref normals, ref texture);
    }

    public static void SetReceivingBuffersSubMesh( [MarshalAs(UnmanagedType.LPStr)] string uuid,
                                                  ref Vector3[] subVertices,
                                                  ref Vector2[] subUvs,
                                                  ref int[] subTriangles,
                                                  ref Vector3[] subNormals)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        manager4DR.affectBuffersSubMesh(ref subVertices, ref subUvs, ref subTriangles, ref subNormals);
    }


    //Starts or stops the playback
    public static void Play( [MarshalAs(UnmanagedType.LPStr)] string uuid, bool on)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR != null)
        {
            if (on)
            {
                if (!manager4DR.isPlaying())
                {
                    manager4DR.Play();
                }
            }
            else
            {
                if (manager4DR.isPlaying())
                {
                    manager4DR.Pause();
                }
            }
        }
    }

    //Gets the new model from plugin
    public static int UpdateModel( [MarshalAs(UnmanagedType.LPStr)] string uuid,
        //System.IntPtr ptrVertices, 
        //System.IntPtr ptrUVs, 
        //System.IntPtr ptrTriangles, 
        //System.IntPtr texture,
                                          ref int nbVertices,
                                          ref int nbTriangles
                                           /*ref Vector3[] normals*/)
    {
        nbVertices = 0;
        nbTriangles = 0;

        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null)
            return -1;

        FDVDecoder4DR decoder4DR = manager4DR.getCurrentMesh();
        if (decoder4DR == null)
            return -1;
        nbVertices = decoder4DR.m_mesh.m_nbVertices;
        nbTriangles = decoder4DR.m_mesh.m_nbFaces;


        // set zero to unfilled vertices array
        /*        int length = decoder4DR.m_mesh.m_uncompressedVertices.Length - nbVertices;
                decoder4DR.m_mesh.m_uncompressedVertices[nbVertices] = Vector3.zero;
                int count;
                for (count = 1; count <= length / 2; count *= 2)
                    System.Array.Copy(decoder4DR.m_mesh.m_uncompressedVertices, nbVertices, decoder4DR.m_mesh.m_uncompressedVertices, nbVertices+count, count);
                System.Array.Copy(decoder4DR.m_mesh.m_uncompressedVertices, nbVertices, decoder4DR.m_mesh.m_uncompressedVertices, nbVertices+count, length - count);
        */
        // set zero to unfilled faces array
        //int length = (decoder4DR.m_mesh.m_facesBuffer.Length - 3 * nbTriangles);
        //if (length > 0)
        //{
        //    decoder4DR.m_mesh.m_facesBuffer[nbTriangles * 3] = 0;
        //    int count;
        //    for (count = 1; count <= length / 2; count *= 2)
        //        System.Array.Copy(decoder4DR.m_mesh.m_facesBuffer, nbTriangles * 3, decoder4DR.m_mesh.m_facesBuffer, 3 * nbTriangles + count, count);
        //    System.Array.Copy(decoder4DR.m_mesh.m_facesBuffer, nbTriangles * 3, decoder4DR.m_mesh.m_facesBuffer, 3 * nbTriangles + count, length - count);
        //}
        //if (nbVertices > 65535)
        //{
        //    length = (decoder4DR.m_mesh.m_trianglesSubMesh.Length - 3 * nbTriangles);
        //    if (length > 0)
        //    {
        //        decoder4DR.m_mesh.m_facesBuffer[nbTriangles * 3] = 0;
        //        int count;
        //        for (count = 1; count <= length / 2; count *= 2)
        //            System.Array.Copy(decoder4DR.m_mesh.m_facesBuffer, nbTriangles * 3, decoder4DR.m_mesh.m_facesBuffer, 3 * nbTriangles + count, count);
        //        System.Array.Copy(decoder4DR.m_mesh.m_facesBuffer, nbTriangles * 3, decoder4DR.m_mesh.m_facesBuffer, 3 * nbTriangles + count, length - count);
        //    }
        //}

        //Compute normals
        if (decoder4DR.m_mesh.m_normalsBuffer != null && !decoder4DR.m_mesh.m_containsNormals)
        {
            FDVUnityBridge.ComputeNormals(ref decoder4DR.m_mesh.m_uncompressedVertices, ref decoder4DR.m_mesh.m_facesBuffer, decoder4DR.m_mesh.m_nbVertices, decoder4DR.m_mesh.m_nbFaces, ref decoder4DR.m_mesh.m_normalsBuffer);
        }

        return manager4DR.getCurrentFrame();
    }


    //
    public static bool OutOfRangeEvent( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR != null)
        {
            bool res = manager4DR.mOutOfRangeEvent;
            manager4DR.mOutOfRangeEvent = false;
            if (res && manager4DR.getOutOfRangeMode() == 2)//stop
                manager4DR.Stop();
            return res;
        }
        else
            return false;
    }


    //Gets the 4DR texture image size
    public static int GetTextureSize( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return 0;
        else
            return manager4DR.getTextureSize();
    }

    //Gets the 4DR texture encoding
    public static string GetTextureEncoding( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return "";
        else
        return manager4DR.getTextureEncoding();
    }

    public static int GetSequenceMaxVertices( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return 0;
        else
        return manager4DR.getMaxNbVertices();
    }

    public static int GetSequenceMaxTriangles( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return 0;
        else
        return manager4DR.getMaxNbFaces();
    }

    public static float GetSequenceFramerate( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return 0;
        else
        return manager4DR.getFrameRate();
    }

    public static int GetSequenceFirstIndex( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR != null)
            return manager4DR.getFirstIndex();
        else
            return 0;
    }

    public static int GetSequenceNbFrames( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR != null)
            return manager4DR.getNbFrames();
        else return 0;
    }

    public static int GetSequenceCurrentFrame( [MarshalAs(UnmanagedType.LPStr)] string uuid)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return 0;
        else
        return manager4DR.getCurrentFrame();
    }

    public static void GotoFrame( [MarshalAs(UnmanagedType.LPStr)] string uuid, int frame)
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return ;
        else
        manager4DR.getMeshAtFrame(frame);
    }

    public static void ChangeOutRangeMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, OUT_RANGE_MODE mode) //0 = Loop  ;  1 = Reverse  ;  2 = stop
    {
        FDVSequenceManager manager4DR = GetSequenceList()[uuid];
        if (manager4DR == null) return ;
        else
        manager4DR.setOutOfRangeMode( (int) mode);
    }

    public static void SetBufferingMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, int mode, int bufferSize) //0 = None ; 1 = Raw ; 2 = Decoded 
    {
        //todo?
    }

    public static void SetCachingMode( [MarshalAs(UnmanagedType.LPStr)] string uuid, int mode) //0 = None ; 1 = Raw ; 2 = Decoded 
    {
        //todo?
    }


#endif


    public static void ComputeNormals(ref Vector3[] VecVertices, ref int[] VecTriangles, int nbVertices, int nbTriangles, ref Vector3[] outNormals)
        {
            if (outNormals != null)
            {
                Vector3 AB, AC, normalFace;

                //for normals, routage table between vertex duplicates
                int[] vertexDuplicates = new int[nbVertices];
                int cptDuplicates = 0;
                for (int i = nbVertices - 1; i > 0; i--)
                {
                    outNormals[i] = Vector3.zero;
                    vertexDuplicates[i] = cptDuplicates;
                    if (VecVertices[i] == VecVertices[i - 1]) cptDuplicates++;
                    else cptDuplicates = 0;
                }
                vertexDuplicates[0] = cptDuplicates;
                outNormals[0] = Vector3.zero;

                for (int i = 0; i < nbTriangles; i++)
                {
                    //compute triangle's normal
                    AB = VecVertices[VecTriangles[i * 3 + 1]] - VecVertices[VecTriangles[i * 3]];
                    AC = VecVertices[VecTriangles[i * 3 + 2]] - VecVertices[VecTriangles[i * 3]];
                    normalFace = Vector3.Cross(AB, AC).normalized;
                    //add the normal to the 3 vertices of the triangle
                    outNormals[VecTriangles[i * 3]] += normalFace;
                    outNormals[VecTriangles[i * 3 + 1]] += normalFace;
                    outNormals[VecTriangles[i * 3 + 2]] += normalFace;
                }

                for (int i = 0; i < nbVertices;)
                {
                    int nbDuplicated = vertexDuplicates[i];
                    if (nbDuplicated == 0)
                    {
                        i++;
                        continue;
                    }

                    Vector3 accumulator = Vector3.zero;
                    for (int v = i; v <= i + nbDuplicated; v++)
                        accumulator += outNormals[v];
                    for (int v = i; v <= i + nbDuplicated; v++)
                        outNormals[v] = accumulator;

                    i += nbDuplicated + 1;
                }
            }
        }
//    }
}