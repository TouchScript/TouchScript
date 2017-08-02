![](https://raw.github.com/wiki/TouchScript/TouchScript/images/dvfu.jpg)

## TouchScript — multi-touch library for Unity

When working on a project for mobile devices or PCs with touch input you will soon require basic gestures like tap, pan, pinch and zoom — they are not hard to implement manually using Unity API or using a package from Asset Store. The hard part is to make these gestures work together, e.g. to have a button with a tap gesture placed on a zoomable window. This is where you will need **TouchScript** — it makes handling complex gesture interactions on any touch surface an effortless job. 

## Why TouchScript?
- TouchScript abstracts touch and gesture logic from input methods and platforms. Your touch-related code will be the same everywhere.
- TouchScript supports many touch input methods starting from smartphones to giant touch surfaces: mouse, Windows 7/8 touch, mobile (iOS, Android, Windows Store/Windows Phone), TUIO.
- TouchScript includes common gesture implementations: press, release, tap, long press, flick, pinch/scale/rotate.
- TouchScript allows you to write your own gestures and custom pointer input logic.
- TouchScript manages gestures in transform hierarchy and makes sure that the most relevant gesture will receive touch input.
- TouchScript comes with many examples and is extensively documented.
- TouchScript makes it easy to test multi-touch gestures without an actual multi-touch device using built-in second touch simulator (activated with Alt + click), [TUIOPad on iOS](https://itunes.apple.com/us/app/tuiopad/id412446962) or [TUIODroid on Android](https://play.google.com/store/apps/details?id=tuioDroid.impl&hl=en"). [Read more](Testing-multitouch-on-a-PC).
- It's free and open-source. Licensed under MIT license.

Developed by Valentin Simonov.

## Getting started
### Downloading the package
To use **TouchScript** in your project you either need to 
* download the [latest release from Github](https://github.com/TouchScript/TouchScript/releases),
* or get it from [Asset Store](https://www.assetstore.unity3d.com/en/#!/content/7394),
* or clone the [repository](https://github.com/TouchScript/TouchScript) and use the source ([more info on how to do it](https://github.com/TouchScript/TouchScript/wiki/How-to-Contribute)).

### Your first TouchScript project
To test how TouchScript works, create an empty scene and drag two prefabs from `TouchScript/Prefabs` folder to the scene: `TouchManager` and `Cursors`. Press Play and click or touch (if your PC supports touch input) the Game View — you will see colored circles, pointer cursors.

> Note: to simulate a second pointer you can hold Alt and click anywhere within the Game View.

You can make any GameObject react to touch input — just attach one of the scripts called Gestures to it. TouchScript ships with a few built-in Gestures which you can find in `Component/TouchScript/Gestures` menu. It is also possible to write your own gestures.

To test how built-in Gestures work, create an empty cube in the scene and attach a `TransformGesture` to it either from `Component` menu or `Add Component` button. Make the cube large enough to be able to touch it with two fingers. Attach another component called `Transformer` to the cube — this component listens to events from `TransformGesture` and applies translation, rotation and scaling to the GameObject.

Press Play. Note how you can drag the object with one touch and scale or rotate it with two touches. Don't forget that you can use Alt + click to simulate a second pointer ([read more more about testing multi-touch gestures](https://github.com/TouchScript/TouchScript/wiki/Testing-multitouch-on-a-PC)).

### Examples
TouchScript comes with many examples in `TouchScript/Examples` folder. Open `Examples.unity` scene and read description for every example to find out what it is about.  

[All examples are explaned here.](https://github.com/TouchScript/TouchScript/wiki/Examples)

### What to read next
- [How to receive a pointer.](https://github.com/TouchScript/TouchScript/wiki/Pointer-Input)
- [What is a Gesture and how to work with it.](https://github.com/TouchScript/TouchScript/wiki/Gestures)
- [What is an Input Source and why it is needed.](https://github.com/TouchScript/TouchScript/wiki/Input-Sources)
- [What is a Layer and why it is needed.](https://github.com/TouchScript/TouchScript/wiki/Layers)
- [Some info on how TouchScript works internally.](https://github.com/TouchScript/TouchScript/wiki/Main-Ideas-Behind-TouchScript)
- [How to affect which objects can be touched.](https://github.com/TouchScript/TouchScript/wiki/Modifying-Hits)
- [How to change touch coordinates from an input device.](https://github.com/TouchScript/TouchScript/wiki/Remapping-Coordinates-From-an-Input-Source)
- [How to write a custom Gesture.](https://github.com/TouchScript/TouchScript/wiki/Tutorial.-Writing-a-Custom-Gesture.)
- [How you can help.](https://github.com/TouchScript/TouchScript/wiki/How-to-Contribute)

## Need help?
> If you have a problem using TouchScript or running examples please check the [FAQ](FAQ) before submitting issues.

 - [FAQ](FAQ)  
_Some of the questions have been already asked multiple times. Check if yours is in the list._
 - [Documentation](http://touchscript.github.io/docs/)  
_Complete up-to-date generated docs with all public API annotated._
 - [Official Forum](http://touchprefab.com/index.php)  
_Want to ask a question about TouchScript? Use the official Forum._
 - [Issues](https://github.com/TouchScript/TouchScript/issues)  
_Found a bug? Got a feature request? Feel free to post it in Issues._  
