#pragma strict
import TouchScript;
import System.Collections.Generic;

function OnTouchFrameStarted()
{
    print("OnTouchFrameStarted");
}

function OnTouchFrameFinished()
{
    print("OnTouchFrameFinished");
}

function OnTouchesBegan(touchPoints:IList.<TouchPoint>)
{
	var s : String = "";
	for (var i:int; i < touchPoints.Count; i++) s += "id: " + touchPoints[i].Id + " ";
    print("OnTouchesBegan ( " + s + ")");
}

function OnTouchesMoved(touchPoints:IList.<TouchPoint>)
{
	var s : String = "";
	for (var i:int; i < touchPoints.Count; i++) s += "id: " + touchPoints[i].Id + " ";
    print("OnTouchesMoved ( " + s + ")");
}

function OnTouchesEnded(touchPoints:IList.<TouchPoint>)
{
	var s : String = "";
	for (var i:int; i < touchPoints.Count; i++) s += "id: " + touchPoints[i].Id + " ";
    print("OnTouchesEnded ( " + s + ")");
}

function OnTouchesCancelled(touchPoints:IList.<TouchPoint>)
{
	var s : String = "";
	for (var i:int; i < touchPoints.Count; i++) s += "id: " + touchPoints[i].Id + " ";
    print("OnTouchesCancelled ( " + s + ")");
}