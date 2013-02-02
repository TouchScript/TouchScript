using System;
using System.Runtime.InteropServices;
using Scaleform;
using Scaleform.GFx;
using TouchScript;
using UnityEngine;
using System.Collections;
using TouchScript.Layers;

/// <summary>
/// Assembled from Scaleform SFCamera and UI_Scene_Demo1 classes
/// </summary>
public class ScaleformLayer : TouchLayer
{
    public String FlashMovieFile = "main.swf";
    public SFInitParams InitParams;

    protected SFManager SFMgr;
    protected ScaleformMovie Movie;

    [DllImport("PC//libgfxunity3d")]
    private static extern void SF_Uninit();

    public void GetViewport(ref int ox, ref int oy, ref int width, ref int height)
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

    #region Unity

    protected void Start()
    {
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
            SFMgr = new SFManager(InitParams);
            SFMgr.Init();
            // SFMgr.InstallDelegates();
            GL.IssuePluginEvent(0);
            GL.InvalidateState();

            Movie = new ScaleformMovie(SFMgr, createMovieCreationParams(FlashMovieFile));

            StartCoroutine(CallPluginAtEndOfFrames());
        }
    }

    protected void Update()
    {
        if (Application.isPlaying)
        {
            SFMgr.ProcessCommands();
            SFMgr.Update();
            SFMgr.Advance(Time.deltaTime);
            SFMgr.ReleaseMoviesMarkedForRelease();
            SFMgr.ReleaseValuesMarkedForRelease();
        }
    }

    protected void OnApplicationQuit()
    {
        if (Application.isPlaying)
        {
            // This is used to initiate RenderHALShutdown, which must take place on the render thread. 
            GL.IssuePluginEvent(2);
            SF_Uninit();
        }
    }

    #endregion

    #region TouchLayer overrides

    protected override void setName()
    {
        Name = "Scaleform Layer";
    }

    protected override HitResult  beginTouch(TouchPoint touch)
    {
        return HitResult.Error;
    }

    protected override void moveTouch(TouchPoint touch)
    {
    }

    protected override void endTouch(TouchPoint touch)
    {
    }

    protected override void cancelTouch(TouchPoint touch)
    {
    }

    #endregion

    // Issues calls to Scaleform Rendering. Rendering is multithreaded on windows and single threaded on iOS/Android
    protected IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(1);
        }
    }

    protected SFMovieCreationParams createMovieCreationParams(string swfName)
    {
        int ox = 0, oy = 0, width = 0, height = 0;
        Int64 start = 0, length = 0;
        Int32 fd = 0;
        IntPtr pDataUnManaged = new IntPtr();
        String SwfPath = GetScaleformContentPath() + swfName;
        GetViewport(ref ox, ref oy, ref width, ref height);
        return new SFMovieCreationParams(SwfPath, ox, oy, width, height, start, length, pDataUnManaged, fd, false);
    }

    protected String GetScaleformContentPath()
    {
        return Application.dataPath + "/StreamingAssets/";
    }
}