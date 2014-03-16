using Scaleform;
using TouchScript;
using TouchScript.Hit;
using TouchScript.Layers;
using UnityEngine;

public class ScaleformLayer : TouchLayer
{

    public string FlashMovieFile = "main.swf";
    public int MovieDepth = 1;
    public Color32 BackgroundColor = new Color32(0, 0, 0, 0);
    public bool OverrideBackgroundColor = true;
    public bool InitFirstFrame = false;
    public ScaleModeType ScaleMode = ScaleModeType.SM_ExactFit;

    public ScaleformMovie FlashInterface
    {
        get { return movie; }
    }

    protected ScaleformMovie movie;
    protected SFCamera scaleformCamera;

    #region Unity

    protected override void Awake()
    {
        base.Awake();
        scaleformCamera = FindObjectOfType(typeof(SFCamera)) as SFCamera;
        if (scaleformCamera == null) Debug.LogError("Scaleform Camera wasn't found! Scaleform layers will not work.");
    }

    private void Update()
    {
        if (movie == null)
        {
            if (scaleformCamera == null) return;
            var manager = scaleformCamera.GetSFManager();
            if (manager == null || !manager.IsSFInitialized()) return;

            SFMovieCreationParams creationParams = SFCamera.CreateMovieCreationParams(FlashMovieFile, MovieDepth, BackgroundColor, OverrideBackgroundColor);
            creationParams.IsInitFirstFrame = InitFirstFrame;
            creationParams.TheScaleModeType = ScaleMode;

            movie = createMovie(creationParams);
        }
    }

    #endregion

    #region TouchLayer overrides

    protected override void setName()
    {
        Name = "Scaleform Layer";
    }

    protected override LayerHitResult beginTouch(ITouch touch, out ITouchHit hit)
    {
        hit = null;
        if (enabled == false || gameObject.activeInHierarchy == false) return LayerHitResult.Miss;

        var result = movie.BeginTouch(touch.Id, touch.Position.x, touch.Position.y);
        return (LayerHitResult)result;
    }

    protected override void moveTouch(ITouch touch)
    {
        movie.MoveTouch(touch.Id, touch.Position.x, touch.Position.y);
    }

    protected override void endTouch(ITouch touch)
    {
        movie.EndTouch(touch.Id);
    }

    protected override void cancelTouch(ITouch touch)
    {
        movie.CancelTouch(touch.Id);
    }

    #endregion

    protected virtual ScaleformMovie createMovie(SFMovieCreationParams creationParams)
    {
        return new ScaleformMovie(this, scaleformCamera, creationParams);
    }

}