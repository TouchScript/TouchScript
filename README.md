# TouchScript — multi-touch framework for Unity3d

*(c) Interactive Lab*
Licensed under MIT license.
version: 1.0

TouchScript is a multi-touch framework originally created by Interactive Lab to develop interactive installations.

## Features
- Supports TUIO, mouse and Windows 7 touch input
- Manages gestures hierarchy and rules distribution of touch points to gestures
- Can group touch points to clusters on big touch surfaces
- Easy to write custom gesture recognizers

## Description
TouchScript consists of a number of components to configure touch and gesture recognition, implementations of common gestures and other tools. Gesture recognition works very similar to iOS implementation: a number of gesture recognizers attached to GameObjects receive touch point events and signal to central manager if they recognize a sequence of touch events as a gesture.

Remember, GameObjects have to have colliders to be touchable.

To add touch support to your application you must do the following: 
- Import TouchScript unity package or copy Plugins folder to your Assets folder
- *[Optional]* Configure scripts execution order in the IDE so that you had all touch inputs executed before TouchManager and before default scripts execution time. For example, set MouseInput to -1003, TuioInput to -1002 and TouchManager to -1001. This step will increase interface responsiveness.
- Add TouchOptions component to a GameObject in your scene.
- Add needed input sources: MouseInput, TuioInput or WMTouchInput.
- Add *Debug Camera* prefab to your scene to see 2d touches on the screen.

## Known limitations.
- Windows 7 touch recognition doesn't work in Unity IDE.

## Examples
See sample scenes in *Scenes* folder.