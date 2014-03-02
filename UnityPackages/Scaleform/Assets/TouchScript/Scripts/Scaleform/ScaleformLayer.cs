using System;
using System.Runtime.InteropServices;
using Scaleform;
using TouchScript;
using TouchScript.Hit;
using UnityEngine;
using System.Collections;
using TouchScript.Layers;

public class ScaleformLayer : TouchLayer
{
    public enum RenderTime
    {
        EndOfFrame = 0,
        PreCamera,
        PostCamera
    };

    public string FlashMovieFile = "main.swf";
    public ScaleModeType ScaleMode = ScaleModeType.SM_NoScale;
    public ScaleformKey Key = new ScaleformKey();

    // When to trigger Scaleform rendering during the frame.
    public RenderTime WhenToRender = RenderTime.EndOfFrame;

    // true if the SFCamera has been initialized; false otherwise.
    public bool Initialized = false;
    public RenderTexture RenderableTexture = null;
    public bool UseBackgroundColor = false;
    public Color32 BackgroundColor = new Color32(0, 0, 0, 255);
    public SFInitParams InitParams;

    public ScaleformMovie FlashInterface
    {
        get { return Movie; }
    }

    public SFManager SFManager
    {
        get { return SFMgr; }
    }

    // Reference to the SFManager that manages all SFMovies.
    protected static SFManager SFMgr;
    protected static bool InitOnce = false;
    protected ScaleformMovie Movie;

    #region Unity

    protected virtual void Start()
    {
        if (!Application.isPlaying) return;

        SF_SetKey(Key.Key);

        //For GL based platforms - Sets a number to use for Unity specific texture management.  Adjust this number if
        //you start to experience black and/or missing textures. 
        SF_SetTextureCount(500);

        DontDestroyOnLoad(this.gameObject);
        SFMgr = new SFManager(InitParams);
        if (SFMgr.IsSFInitialized())
        {
            InitParams.Print();
            GL.IssuePluginEvent(0);
            GL.InvalidateState();
        }

        Movie = createMovie();

        if (WhenToRender == RenderTime.EndOfFrame) StartCoroutine("CallPluginAtEndOfFrame");
    }

    protected virtual void Update()
    {
        if (!Application.isPlaying) return;

        if (SFMgr != null)
        {
            SFMgr.ProcessCommands();
            SFMgr.Update();
            SFMgr.Advance(Time.deltaTime);
        }
    }

    // Used with PreCamera render layers, pumps Scaleform once prior to the Camera instance rendering its contents
    protected virtual void OnPreRender()
    {
        if (!Application.isPlaying) return;
        if (WhenToRender != RenderTime.PreCamera) return;

        PumpPluginRender();
    }

    // Used with PostCamera render layers, pumps Scaleform once after the Camera instance renders its contents
    protected virtual void OnPostRender()
    {
        if (!Application.isPlaying) return;
        if (WhenToRender != RenderTime.PostCamera) return;

        PumpPluginRender();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!Application.isPlaying) return;
        // This is used to initiate RenderHALShutdown, which must take place on the render thread. 
        GL.IssuePluginEvent(2);
        SF_Uninit();
    }

    protected virtual void OnApplicationQuit()
    {
        /*
                // This is used to initiate RenderHALShutdown, which must take place on the render thread. 
        #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
                GL.IssuePluginEvent(2);
        #endif
                SF_Uninit();
         */
    }

    #endregion

    #region TouchLayer overrides

    protected override void setName()
    {
        Name = "Scaleform Layer";
    }

    protected override LayerHitResult beginTouch(ITouch touch, out ITouchHit hit)
    {
        var result = Movie.BeginTouch(touch.Id, touch.Position.x, touch.Position.y);
        hit = null;
        return (LayerHitResult)result;
    }

    protected override void moveTouch(ITouch touch)
    {
        Movie.MoveTouch(touch.Id, touch.Position.x, touch.Position.y);
    }

    protected override void endTouch(ITouch touch)
    {
        Movie.EndTouch(touch.Id);
    }

    protected override void cancelTouch(ITouch touch)
    {
        Movie.CancelTouch(touch.Id);
    }

    #endregion

    // Issues a call to perform Scaleform rendering. Rendering is multithreaded on windows and single threaded on iOS/Android
    private void PumpPluginRender()
    {
        GL.IssuePluginEvent(1);
    }

    // Used with EndOfFrame render layers, pumps Scaleform once per frame at the end of the frame
    protected IEnumerator CallPluginAtEndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            PumpPluginRender();
        }
    }

    protected virtual ScaleformMovie createMovie()
    {
        SFMovieCreationParams creationParams = CreateMovieCreationParams(FlashMovieFile);
        creationParams.TheScaleModeType = ScaleMode;
        creationParams.IsInitFirstFrame = false;
        return new ScaleformMovie(this, SFMgr, creationParams);
    }

    public static void GetViewport(ref int ox, ref int oy, ref int width, ref int height)
    {
        width = Screen.width;
        height = Screen.height;
        ox = 0;
        oy = 0;
        // Note that while using D3D renderer, the tool bar (that contains "Maximize on Play" text) is part of 
        // the viewport, while using GL renderer, it's not. So there should be a further condition here depending on 
        // what type of renderer is being used, however I couldn't find a macro for achieving that. 
#if UNITY_EDITOR
        oy = 24;
#endif
    }

    public static SFMovieCreationParams CreateRTTMovieCreationParams(string swfName, int depth, int RTToX, int RTToY, RenderTexture texture, Color32 clearColor)
    {
        // Used for Android only
        Int32 length = 0;
        IntPtr pDataUnManaged = IntPtr.Zero;
        String SwfPath = SFManager.GetScaleformContentPath() + swfName;

        return new SFMovieCreationParams(SwfPath, depth, RTToX, RTToY, texture.width, texture.height, pDataUnManaged, length, false, texture, clearColor, true, ScaleModeType.SM_ShowAll, true);
    }

    public static SFMovieCreationParams CreateMovieCreationParams(string swfName, int depth = 1)
    {
        return CreateMovieCreationParams(swfName, depth, new Color32(0, 0, 0, 0), false);
    }

    public static SFMovieCreationParams CreateMovieCreationParams(string swfName, int depth, byte bgAlpha)
    {
        return CreateMovieCreationParams(swfName, depth, new Color32(0, 0, 0, bgAlpha), false);
    }

    public static SFMovieCreationParams CreateMovieCreationParams(string swfName, int depth, Color32 bgColor, bool overrideBackgroundColor)
    {
        Int32 length = 0;

        int ox = 0;
        int oy = 0;
        int width = 0;
        int height = 0;

        IntPtr pDataUnManaged = IntPtr.Zero;
        String swfPath = SFManager.GetScaleformContentPath() + swfName;
        GetViewport(ref ox, ref oy, ref width, ref height);
        return new SFMovieCreationParams(swfPath, depth, ox, oy, width, height, pDataUnManaged, length, false, bgColor, overrideBackgroundColor, ScaleModeType.SM_ShowAll, true);
    }

    public SFMovieCreationParams CreateMovieCreationParams(string swfName, int depth, Byte[] swfBytes, Color32 bgColor, bool overrideBackgroundColor)
    {
        int ox = 0;
        int oy = 0;
        int width = 0;
        int height = 0;

        GetViewport(ref ox, ref oy, ref width, ref height);

        Int32 length = 0;
        IntPtr pDataUnManaged = IntPtr.Zero;

        if (swfBytes != null)
            length = swfBytes.Length;

        if (length > 0)
        {
            pDataUnManaged = new IntPtr();
            pDataUnManaged = Marshal.AllocCoTaskMem((int)length);
            Marshal.Copy(swfBytes, 0, pDataUnManaged, (int)length);
        }

        String swfPath = SFManager.GetScaleformContentPath() + swfName;

        return new SFMovieCreationParams(swfPath, depth, ox, oy, width, height, pDataUnManaged, length, false, bgColor, overrideBackgroundColor, ScaleModeType.SM_ShowAll, true);
    }

    [DllImport("libgfxunity3d")]
    private static extern void SF_Uninit();

    [DllImport("libgfxunity3d")]
    public static extern void SF_SetKey(String key);

    [DllImport("libgfxunity3d")]
    public static extern void SF_SetTextureCount(int textureCount);
}