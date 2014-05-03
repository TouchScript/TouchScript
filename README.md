![](https://raw.github.com/wiki/InteractiveLab/TouchScript/images/dvfu.jpg)

## TouchScript — multitouch library for Unity
> Warning! [Please read before upgrading](Upgrading).

**TouchScript** is much more than a handful of routines to receive touch events from different sources. Inspired by iOS, TouchScript makes handling complex gesture interactions on large touch surfaces much easier. Unity and Flash implementations are developed to work together in Scaleform environment.

**TouchScript** was developed by Valentin Simonov at [Interactive Lab](http://interactivelab.ru) to:
 - Provide a reliable way to code multitouch interfaces on various devices.
 - Unify different input methods.
 - Handle complex interactions between gestures.
 - Take into account differences between a large touch surface and an iPad.

## Features
 - Easy and intuitive API.
 - Works on PC (Windows 7 and Windows 8), Mac, iOS, Android and Windows RT.
 - Doesn't require Unity Pro.
 - Supports UnityScript _(starting from version 5)_.
 - Supports various input sources: TUIO, mouse, mobile (iOS, Android, WinRT) and native Windows touch.
 - **Manages simultaneous gesture recognition within scene hierarchy (inspired by iOS).**
 - Groups touch points into clusters on big touch surfaces.
 - Comes with many commonly used gestures. Easy to write custom ones.
 - Easy to test multitouch gestures without actual multitouch device using built-in second touch simulator (activated with ALT+CLICK), [TUIOPad on iOS](https://itunes.apple.com/us/app/tuiopad/id412446962) or [TUIODroid on Android](https://play.google.com/store/apps/details?id=tuioDroid.impl&hl=en"). [Read more](Testing-multitouch-on-a-PC).
 - **It's free and open-source. Licensed under MIT license.**

## Getting started
The library comes in several packages to use on different devices.  
Every package has an accompanying example package which contains example scenes you might want to check to get a better understanding of how TouchScript works.

 - [Packages](Packages)  
_Find out what all these packages mean._
 - [Getting Started](Getting-Started)  
_See what you need to do to start using TouchScript in your Unity project._
 - [Upgrading Guide](Upgrading)  
_Please read this document if you are upgrading from an earlier version of TouchScript._
 - [Tutorials](Tutorials)  
_Learn how to use TouchScript and its various features._
 - [Building from Sources](How-to-Contribute)  
_If you prefer to build everything from sources or you want to modify the library, you can read here about it._

## Need help?
> If you got a problem using TouchScript or running examples please check the [FAQ](FAQ) before submitting issues.

 - [FAQ](FAQ)  
_Some of the questions have been already asked multiple times. Check if yours is in the list._
 - [Up-to-date Unity documentation](http://interactivelab.github.io/TouchScript/docs/Index.html)  
_Complete up-to-date generated docs with all public API annotated._
 - [Issues](https://github.com/InteractiveLab/TouchScript/issues)  
_Found a bug? Got a feature request? Feel free to post it in Issues._
 - [Unity Forum Thread](http://forum.unity3d.com/threads/172955-TouchScript-%97-multi-touch-library-for-Unity-RELEASED)  
_Want to ask a question about TouchScript? Use the official Unity Forum thread._

## TOC
 - [Main ideas behind TouchScript](Main-Ideas-Behind-TouchScript)
 - [Versions](Versions)
 - [Input sources](Input-Sources)
 - [Touch layers](Layers)
 - [Gestures](Gestures)
 - [DPI and various devices](Display-Devices)
 - [Tutorials](Tutorials)
 - [How to Contribute](How-to-Contribute)  
 - [Version history](Version-History)

## How to contribute?
**TouchScript** is an open source project, like all the other open source projects out there it needs your support!  
You can contribute in several ways:
- **Fix bugs in code** — if you found a bug and know how to fix it, it will be very helpful for the project if you fixed it and submitted a pull request;
- **Add features** — if you need a specific feature for your project and think that it could be useful for others, feel free to submit a pull request;
- **Write tutorials** — if you are not a developer or are not experienced enough to read TouchScript's code, you can still help by updating the docs and writing tutorials. Because good documentation is what makes an open source project alive;
- **Test it on devices** — TouchScript can be used on various devices but we don't have all of them nor we have time to test the library everywhere, so we would be really glad if you could run examples or test your project using TouchScript on devices you have.

If you want to contribute to TouchScript please read [this document](How-to-Contribute).
