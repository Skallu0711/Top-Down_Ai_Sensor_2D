# Field of View
Simple "Field of View" game mechanics tool designed to be used in 2D top-down perspective games.

<img src="images/project.gif">

## How does this mechanics work?
Script peforms cyclical operations of checking whether the target is inside the character's field of view.
Every "zone check" operation is is delayed by some time to increase performance. This value is modifiable via script.

The basic steps of "zone check":
1. Check if the target is inside outer view radius
2. Then check if it is inside inner view radius and if it is not behind any obstacle.
3. If target is inside outer view radius but outside the inner radius, check if its inside view angle, then check if it is not behind any obstacle.

## Setup
1. Set values that suit your needs inside inspector
2. Set game object that will be considered as target
```csharp
private void Awake()
    {
        // set target here
        // target = 
    }
```

## FOV editor tool
This script has built-in editor tool that visualizes character's field of view by drawing debug circles and lines on scene.
Every parameter that makes up the "Field of View" can be modified via inspector to suit Your needs.

<img src="images/fov_handles.png">

## My package
Here is a link for my custom package, which i used in this project:
https://github.com/Skallu0711/Skallu-Utils
