# Field of View tool
2D top-down perspective "Field of View" game mechanics tool.
Whole system is about performing cyclical check whether the target is inside character's field of view and its not covered by any obstacle.

Presented system has built-in editor tool, which visualises character's field of view by drawing debug circles and lines on scene.
Every parameter that makes up "Field of View" can be modified via inspector to suit Your needs.

<img src="images/fov_overview.gif">

## Setup
1. attach component to game object:
<img src="images/fov_add.png">

2. adjust values:
<img src="images/fov_inspector.png">

3. set target inside script:
```csharp
private void Awake()
    {
        // set target here
        // target = 
    }
```

## Usage Example:
<img src="images/fov_example.gif">

## My package
Here is a link for my custom package, which i used in this project:
https://github.com/Skallu0711/Skallu-Utils
