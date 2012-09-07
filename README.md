# TouchScript — multi-touch framework for Unity3d
(c) Interactive Lab  
Licensed under MIT license.  
version: 1.0.1  

TouchScript is a multi-touch framework originally created by Interactive Lab to develop interactive installations.

## Documentation
Up to date documentation is available [at our web site](http://touchscript.interactivelab.ru/).  
If you have any questions feel free to ask them here.

## Features
- Supports TUIO, mouse and Windows 7 touch input
- Manages gestures hierarchy and rules distribution of touch points to gestures
- Can group touch points to clusters on big touch surfaces
- Easy to write custom gesture recognizers

## Description
TouchScript consists of a number of components to configure touch and gesture recognition, implementations of common gestures and other tools. Gesture recognition works very similar to iOS implementation: a number of gesture recognizers attached to GameObjects receive touch point events and signal to central manager if they recognize a sequence of touch events as a gesture.

Remember, GameObjects have to have colliders to be touchable.

### To add touch support to your application you must do the following: 

- Import TouchScript unity package or copy Plugins folder to your Assets folder
- *[Optional]* Configure scripts execution order in the IDE so that you had all touch inputs executed before TouchManager and before default scripts execution time. For example, set MouseInput to -1003, TuioInput to -1002 and TouchManager to -1001. This step will increase interface responsiveness.
- Add TouchOptions component to a GameObject in your scene.
- Add needed input sources: MouseInput, TuioInput or WMTouchInput.
- Add *Debug Camera* prefab to your scene to see 2d touches on the screen.

## Known limitations.
- Windows 7 touch recognition doesn't work in Unity IDE.

## Before building
- Copy UnityEngine.dll to Lib folder since the framework links to it. If anyone knows a better way of doing this without hardcoding paths or setting system variables, please let us know.

## Tutorial

In *Gesture and Event Propagation* scene we have a hierarchy of objects which got different gestures attached. 

Check out *Main Camera* for inputs configuration and touch options.

Let's take a look at *Plane* GameObject. It has the following gestures attached: *Pan*, *Scale*, *Rotate* and *Flick*. Also two behaviors which depend on these gestures: *Transformer2D* and *TSFLickDetector*. They all work together because they reference each other in *Will Recognize With* array. **Otherwise only the first gesture to recognize in transforms hierarchy will be able to access these touch points.**

Check out the documentation to see what gestures' parameters mean.

Transformer2D applies transforms from attached gestures to the GameObject. Open *TSFlickDetector* to see how it works. It has only one line of code which uses FlickGesture's *StateChanged* event to get notified when it is recognized.

```
GetComponent<FlickGesture>().StateChanged += delegate(object sender, GestureStateChangeEventArgs args) { 
if (args.State == Gesture.GestureState.Recognized) print("FLICK"); };
```

Other objects in the scene also have *Press*, *Release* and *Tap* gestures attached which are self explanatory. *Bump* and *Colored* behaviors show how to handle events when these gestures are recognized.

```
if (GetComponent<PressGesture>() != null) GetComponent<PressGesture>().StateChanged += onPress;
if (GetComponent<ReleaseGesture>() != null) GetComponent<ReleaseGesture>().StateChanged += onRelease;

private void onRelease(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs) {
  transform.localScale = startScale;
}

private void onPress(object sender, GestureStateChangeEventArgs gestureStateChangeEventArgs) {
  transform.localScale = startScale * .7f;
}
```

Notice that cubes in the example can be tapped and dragged separately or with the parent cube.

```
tap.StateChanged += (sender, args) => { 
  if (args.State == Gesture.GestureState.Recognized) 
    renderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); 
};
```

## Examples
See sample scenes in *Scenes* folder.

## Versions history
**1.0.1** *- 07.09.2012*
- Fixed: bugs with disabled gestures.
- Fixed: MouseInput bug with frozen touch points.
- Fixed: simultaneous gesture recognition.
- Added: Cluster.Camera.
- Added: DLLs to the sample project.
- Removed: PreventTouchFlicker in TuioInput.
- Changed: camera settings for debug camera.

**1.0**
- Initial release.