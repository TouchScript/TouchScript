using Scaleform;
using Scaleform.GFx;
using UnityEngine;

public class ScaleformMovie : Movie
{
    
    private Value[] beginValues;
    private Value[] moveValues;
    private Value[] endValues;
    private Value[] cancelValues;

    protected Value SWF { get; private set; }

    public ScaleformMovie(SFManager sfmgr, SFMovieCreationParams cp) :
        base(sfmgr, cp)
    {
        beginValues = new Value[3];
        moveValues = new Value[3];
        endValues = new Value[1];
        cancelValues = new Value[1];

        SetFocus(true);
    }

    public int BeginTouch(int id, float x, float y)
    {
        beginValues[0] = new Value(id, MovieID);
        beginValues[1] = new Value(x / Screen.width, MovieID);
        beginValues[2] = new Value(1 - y / Screen.height, MovieID);
        var result = Invoke("root.Scaleform_beginTouch", beginValues, 3);
        if (result != null && result.IsInt()) return result.GetInt();
        return 0;
    }

    public void MoveTouch(int id, float x, float y)
    {
        moveValues[0] = new Value(id, MovieID);
        moveValues[1] = new Value(x / Screen.width, MovieID);
        moveValues[2] = new Value(1 - y / Screen.height, MovieID);
        Invoke("root.Scaleform_moveTouch", moveValues, 3);
    }

    public void EndTouch(int id)
    {
        endValues[0] = new Value(id, MovieID);
        Invoke("root.Scaleform_endTouch", endValues, 1);
    }

    public void CancelTouch(int id)
    {
        cancelValues[0] = new Value(id, MovieID);
        Invoke("root.Scaleform_cancelTouch", cancelValues, 1);
    }

    #region Scaleform callbacks

    public void RegisterScaleformInput(Value swf)
    {
        Debug.Log("Got swf reference from Scaleform movie.");

        SWF = swf;
    }

    #endregion
}