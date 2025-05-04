> Latest regen from Box2D v3.1: **2025/05/04**

# Intro

This is a thin [Box2d v3](https://github.com/erincatto/box2d) wrapper that stays true to the original API. 

The main objective for this wrapper is to be:

* very thin, as if you were working directly with the original C library.
* performance: prevent data copying, prevent shortlived heap allocations.

Because of these, Box2dNet gives you full control over the API with the same names/contracts as the original, but with a bit of manual labour here and there.

> I don't use Unity and therefore cannot support it. This wrapper is meant to run in standard .NET runtimes, for instance combined with Monogame or Godot.

# QUICKSTART

* Add the [Box2dNet nuget package](https://www.nuget.org/packages/Box2dNet/) to your game's solution.
* Start calling Box2D API methods from class `B2Api` (in `Box2dNet.Interop`). Their identifiers are the same as the original Box2D API, on purpose. 
* Work on your game!

The upside of a thin wrapper is you don't need to learn a different API, it's the same. The downside is you have to deal with the pointers (IntPtr). See section `Dealing with pointers (IntPtr)` in this manual for making that easier. Box2dNet also contains some helper code to minimize the pointer plumbing.

> When you build your game in DEBUG it will use the native debug dll ```box2dd.dll``` and show assertions if you do something wrong. When you build in RELEASE it will use the native production dll ```box2d.dll```.

# License

You may do whatever you like with the code in this repo. Don't forget to respect the [Box2d v3.x](https://github.com/erincatto/box2d) license, though!


# What's included?

All Box2D API functions are available as C# static methods with the exact same identifier in static class ```Box2dNet.Interop.B2Api```. Original comments are also available, so code completion is quite rich.

NOT included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2DynamicTree_X: too little value for too much effort to support this in my codegen tool. This is the spatial tree used internally by Box2D. I think Erin exposed it for public use because people may want to use it for other purposes (?). But you don't need this for normal Box2D use.

# Dealing with pointers (IntPtr)

The largest down-side of PInvoke wrappers is that all C pointers become `IntPtr` in .NET. Because of this, the helpful identifier of the `struct` or `delegate` is lost in C#. 

To help with this, Box2dNet mentions the original C type in the C# generated comments wherever possible. Code completion should therefore show this information. Worst case, you can GoToSource (F12) on anything and will find the helpful name in /* comment */ just next to `IntPtr` in the wrapper's source.

To help you with IntPtrs, the following sections show solutions for most use cases:

## Callbacks: passing in delegates to IntPtr parameters

Some functions or structs require you to pass in a delegate to a callback method. These are always `IntPtr`, 

To find out what parameters and return type your callback function should have, you must:

* check the generated comments to find the identifier of the original C type
* search for that identifier in the B2Api.cs to find the C# delegate definition.
* create your callback function in C#
* pass a function pointer retrieves with `Marshal.GetFunctionPointerForDelegate` to the Box2D native side.

Here is an example on how to call `b2World_OverlapCircle` with a callback delegate of type `b2OverlapResultFcn`:

``` C#
public void Update()
{
    var circle = new b2Circle(Vector2.Zero, 10);
    var filter = new b2QueryFilter(PhysicsLayer.Query, PhysicsLayer.RobotCore);
    _list.Clear();
    B2Api.b2World_OverlapCircle(_b2WorldId, circle, b2Transform.Zero, filter, 
        Marshal.GetFunctionPointerForDelegate((b2OverlapResultFcn)QueryCallback), IntPtr.Zero);
}

private bool QueryCallback(b2ShapeId shapeId, IntPtr context) // <-- delegate 'b2OverlapResultFcn'
{
    _list.Add(shapeId); // or get a corresponding .NET object using 'userData' (see samples) or some dictionary.
    return true;
}
```

## Reading native arrays from IntPtr

Some structs received from native Box2D contain arrays. To read those arrays Box2dNet provides convenience method `NativeArrayAsSpan` to loop over the native contents without making temporary copies or allocating an iterator.

Example: field `IntPtr b2ContactEvents.beginEvents` shows in its comment that you should read it as an array of `b2ContactBeginTouchEvent`:

``` C#
var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
// use helper extension method to efficiently read the native hitEvents array with little code:
foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
{
    Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
}
```

> Note that you must know the size of the array, which is always provided by Box2D in a sibling field.

## Pass a reference to my .NET object into native Box2D as IntPtr

If you want to pass a .NET object reference into native Box2D, like tagging a shape with `userData`, you must also do this using an `IntPtr`.

To do this, you allocate a `NativeHandle` for your .NET object and pass the handle's `IntPtr` to Box2D. Example:

``` C#
_handle = NativeHandle.Alloc(ball); // allocate a IntPtr handle for the .NET object and return it as IntPtr.

var shapeDef = B2Api.b2DefaultShapeDef();
// now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
shapeDef.userData = _handle;
var circle = new b2Circle() { radius = 1 };
var b2ShapeId = B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);
```

After this, when Box2D passes the `IntPtr` back to you somewhere, you can get hold of the corresponding .NET object like this:

``` C#
var userDataIntPtr = B2Api.b2Shape_GetUserData(shapeId);
var ball = NativeHandle<Ball>.GetObjectFromIntPtr(userDataIntPtr);
```

Note that you must keep the `NativeHandle` alive as long as you want the `IntPtr` held by native Box2D to remain valid.
When you're fully done with it, eg. when the game object is removed from your game, you must not forget to free it, like this: 

``` C#
NativeHandle.Free(_handle);
```

# Multi-threading support

Box2dNet comes with .NET integration for the new multi-threaded task system that Box2D uses. 

Simply use `B2Api.b2DefaultWorldDef_WithDotNetTpl()` instead of `B2Api.b2DefaultWorldDef` to create your Box2D world:

``` C#
var worldDef = useMultiThreading
    ? B2Api.b2DefaultWorldDef_WithDotNetTpl() // <---- this is all it takes for default multi threading
    : B2Api.b2DefaultWorldDef();

var b2WorldId = B2Api.b2CreateWorld(worldDef);
```

Note that multithreading incurs a severe performance penalty because of the additional infrastructure. You only gain net-profit from multi threading when you simulate a lot of bodies. Measure your specific use cases.

> the TPL in `b2DefaultWorldDef_WithDotNetTpl` stands for Task Parallel Library, which is the name of the .NET Task framework.

# Samples

Most specific techniques are described in the manual above, but you can also check out the `Box2dNet.Samples` console app in this repo to see working code that gets you started on common usecases like detecting collisions.

# Regenerating the wrapper

Currently, I regenerate every few weeks for the latest version of Box2D. The resulting Win x64 DLLs (debug and release) are included in this repo, so generally, you won't have to regenerate yourself. 

But if you must, perform these 2 steps:

## 1 - Rebuilding the Box2D DLLs

> Note: this is for Windows

Make sure you have `cmake` installed; use the .msi from https://cmake.org/download.

* Clone or pull the latest version of `erincatto/box2d`
* in `CMakeLists.txt` around line 11 add a line `option(BUILD_SHARED_LIBS "Build using shared libraries" ON)` which makes it build to dll instead of statically linked lib
* run `box2d\create_sln.bat` -> generates a full .NET solution (and .sln) in folder `box2d\build`
* if it did not open automatically, open the generated ```.\build\box2d.sln``` in Visual Studio
* **Re**build the `box2d` project (not the entire solution) both in Debug and Release to get both debug dll `box2dd.dll` and production dll `box2d.dll`.
* Also clone this repo (Box2dNet) 
* Open `Box2dNet.sln` in Visual Studio and copy the freshly built dlls and pdbs from `box2d\build\bin\Debug` and `box2d\build\bin\Release` into the Box2dNet project and ensure their Copy to Output Directory is set to *copy if newer* in the properties (alt+enter)

## 2 - Regenerate the C# code

The C# wrapper code can be regenerated with the companion codegen tool ```Box2dGen```, also in this repo. 
It's a naive C parsing + codegen tool, very specific to the Box2D codebase. It was originally meant for my eyes only, so it'll require some digging on your part.

It expects commandline parameters: 

`Box2dNetGen.exe <box2d-repo-root> <B2Api.cs-file-output>`

Example:

`Box2dNetGen.exe C:\repos\box2d "C:\repos\Box2dNet\Box2dNet\Interop\B2Api.cs"`

> Make sure the second parameter points to the existing `B2Api.cs` file in your local Box2dNet repo so it gets overwritten.

The generator gives several warnings about the "exclude-list", which is deliberate, and therefore to be ignored.
If you get other warnings or errors, though, some of the new C code is incompatible with my tool. You can let me know if I'm not working on it already, or you can give it a shot yourself and send me a PR.

## 3 - Use Box2dNet

Build the now updated Box2dNet solution and use the dll in your game, or directly refer to the Box2dNet project from your game's solution.
