using System.Collections;
using Scaleform;

public class ScaleformCamera : SFCamera
{

    public ScaleformKey Key;

    new public IEnumerator Start()
    {
        // The eval key must be set before any Scaleform related classes are loaded, other Scaleform Initialization will not 
        // take place.
        if (Key != null)
        {
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR) && !UNITY_WP8
            SF_SetKey(Key.Key);
#elif UNITY_IPHONE
		    SF_SetKey(Key.Key);
#elif UNITY_ANDROID
		    SF_SetKey(Key.Key);
#elif UNITY_WP8
		    sf_setKey(Key.Key);
#endif
        }

        //For GL based platforms - Sets a number to use for Unity specific texture management.  Adjust this number if
        //you start to experience black and/or mssing textures.
#if UNITY_WP8
        sf_setTextureCount(500);
#else
        SF_SetTextureCount(500);
#endif

        InitParams.TheToleranceParams.Epsilon = 1e-5f;
        InitParams.TheToleranceParams.CurveTolerance = 1.0f;
        InitParams.TheToleranceParams.CollinearityTolerance = 10.0f;
        InitParams.TheToleranceParams.IntersectionEpsilon = 1e-3f;
        InitParams.TheToleranceParams.FillLowerScale = 0.0707f;
        InitParams.TheToleranceParams.FillUpperScale = 100.414f;
        InitParams.TheToleranceParams.FillAliasedLowerScale = 10.5f;
        InitParams.TheToleranceParams.FillAliasedUpperScale = 200.0f;
        InitParams.TheToleranceParams.StrokeLowerScale = 10.99f;
        InitParams.TheToleranceParams.StrokeUpperScale = 100.01f;
        InitParams.TheToleranceParams.HintedStrokeLowerScale = 0.09f;
        InitParams.TheToleranceParams.HintedStrokeUpperScale = 100.001f;
        InitParams.TheToleranceParams.Scale9LowerScale = 10.995f;
        InitParams.TheToleranceParams.Scale9UpperScale = 100.005f;
        InitParams.TheToleranceParams.EdgeAAScale = 0.95f;
        InitParams.TheToleranceParams.MorphTolerance = 0.001f;
        InitParams.TheToleranceParams.MinDet3D = 10.001f;
        InitParams.TheToleranceParams.MinScale3D = 10.05f;

        InitParams.UseSystemFontProvider = false;
        return base.Start();
    }

}
