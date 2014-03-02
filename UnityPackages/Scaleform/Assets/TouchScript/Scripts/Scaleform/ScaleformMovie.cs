using Scaleform;
using UnityEngine;

public class ScaleformMovie : Movie
{
    
    public ScaleformLayer Layer { get; private set; }
    public SFManager SFManager { get; private set; }
    public Value SWF { get; private set; }

    public ScaleformMovie(ScaleformLayer layer, SFManager sfmgr, SFMovieCreationParams creationParams) :
        base(sfmgr, creationParams)
    {
        Layer = layer;
        SFManager = sfmgr;

        SetFocus(true);
    }

    public int BeginTouch(int id, float x, float y)
    {
        var result = Invoke("root.Scaleform_beginTouch", id, x / Screen.width, 1 - y / Screen.height);
        if (result != null && result.IsInt()) return result;
        return 0;
    }

    public void MoveTouch(int id, float x, float y)
    {
        Invoke("root.Scaleform_moveTouch", id, x / Screen.width, 1 - y / Screen.height);
    }

    public void EndTouch(int id)
    {
        Invoke("root.Scaleform_endTouch", id);
    }

    public void CancelTouch(int id)
    {
        Invoke("root.Scaleform_cancelTouch", id);
    }

    public override bool AcceptMouseEvents()
    {
        // we route all input data from Unity
        return false;
    }

    public override bool AcceptTouchEvents()
    {
        // we route all input data from Unity
        return false;
    }

    #region Scaleform callbacks

    public void RegisterScaleformInput(Value swf)
    {
        Debug.Log("Got swf reference from Scaleform movie.");

        SWF = swf;
    }

    #endregion
}