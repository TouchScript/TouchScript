using Scaleform;
using Scaleform.GFx;
using UnityEngine;
using System.Collections;

public class ScaleformMovie : Movie {


    public ScaleformMovie(SFManager sfmgr, SFMovieCreationParams cp) :
        base(sfmgr, cp) {
		SetFocus(true);
    }

}
