# Box2dNet

[![NuGet](https://img.shields.io/nuget/v/Box2dNet.svg)](https://www.nuget.org/packages/Box2dNet/)

This is a thin [Box2d v3](https://github.com/erincatto/box2d) wrapper that stays true to the original API. 

The main objective for this wrapper is to be:

* very thin, as if you were working directly with the original C library.
* performant: prevent data copying, prevent heap allocations.

Because of these, Box2dNet gives you full control over the API with the same names/contracts as the original, but with a bit of manual labour here and there.

> I don't use Unity and therefore cannot support it. This wrapper is meant to run in standard .NET runtimes, for instance combined with Monogame or Godot.

# QUICKSTART

* Add the [![NuGet](https://img.shields.io/nuget/v/Box2dNet.svg)](https://www.nuget.org/packages/Box2dNet/) to your game's solution.
* Start calling Box2D API methods from class `B2Api` (in `Box2dNet.Interop`). Their identifiers are the same as the original Box2D API, on purpose. 
* Work on your game!

The upside of a thin wrapper is you don't need to learn a different API, it's the same. The downside is you have to deal with the pointers (IntPtr). See section `Dealing with pointers (IntPtr)` in this manual for making that easier. Box2dNet also contains some helper code to minimize the pointer plumbing.

# License

You may do whatever you like with the code in this repo. Don't forget to respect the [Box2d v3.x](https://github.com/erincatto/box2d) license, though!

# What's included?

* Virtually all Box2D API functions are available as C# static methods with the original identifier in static class ```Box2dNet.Interop.B2Api```. Original comments are also available, so code completion is quite rich.
* Ready-to-use wiring for running Box2D's multi threading system in .NET Tasks. See further down this manual.
* A growing set of quality-of-life helpers. Some included are:
  * Reading native arrays from result structs as .NET Spans with the `~AsSpan` sibling property. 
  * API functions that take delegate pointers (IntPtr) have an overload with strongly typed .NET delegate.
  * A few IEquatable implementations, b2Rot.GetAngle()/FromAngle(x) etc

## **NOT** included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2DynamicTree_X: little value for much effort on my side. This is the spatial tree used internally by Box2D. Erin exposed these because people may want to use the tree elsewhere, but you don't need these functions for normal Box2D use.

## Where's the DEBUG build of Box2D ?

Box2D has a debug version that shows assert messages when you do something wrong. Currently, there is no Box2dNet nuget package that includes this debug-version of Box2D. If you get into trouble and want to see these messages, you can switch from using the nuget version to simply copying the Box2dNet project (https://github.com/thomasvt/Box2DNet/tree/main/Box2dNet) as a sibling project into your game's .sln. This will make it use the debug version of Box2D with the assert messages when you run your game in Debug target.

# Multi-threading support

Box2dNet comes with .NET integration for the new multi-threaded task system that Box2D uses. 

Simply use `B2Api.b2DefaultWorldDef_WithDotNetTpl()` instead of `B2Api.b2DefaultWorldDef` to create your Box2D world:

``` C#
var worldDef = useMultiThreading
    ? B2Api.b2DefaultWorldDef_WithDotNetTpl() // <---- this is all it takes for default multi threading
    : B2Api.b2DefaultWorldDef();

var b2WorldId = B2Api.b2CreateWorld(worldDef);
```

Note that multithreading incurs quite some overhead so you only gain net-profit when you simulate a lot of bodies. Measure your specific use cases!

> the 'TPL' in `b2DefaultWorldDef_WithDotNetTpl` stands for Task Parallel Library, which is the name of .NET's Task framework.

# Dealing with pointers (IntPtr)

The largest down-side of PInvoke wrappers is that many C pointers become `IntPtr` in .NET. Because of this, the type that was there in C of the `struct` or `delegate` is lost and replaced by `IntPtr` in C#. 

To help with this, Box2dNet has several helpers to deal with IntPtr, or to remove the need to deal with them at all.

Several situations remain, though, where you need to do it yourself. The following sections show solutions for most of these cases.

The original C type of the pointer is in the generated comments. Code completion should show this information. Worst case, you'll have to look in the sources here on gitHub.

## Reading native arrays from IntPtr (without copying)

Some structs received from native Box2D contain IntPtrs to arrays. Box2dNet provides companion properties called `~AsSpan` for these with which you can read the data directly from native memory, strongly typed (so no allocations for copies or iterators). 

> If this companion property is not there, you can convert the IntPtr yourself to a Span using Box2dNet's convenience method `NativeArrayAsSpan(count)` which is simply what the `~AsSpan` does behind the scenes.

Example: struct `b2ContactEvents` contains an array in field `IntPtr beginEvents` which you can read easily using companion `beginEventsAsSpan`:

``` C#
var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
foreach (var @event in **hitEvents.beginEventsAsSpan**)
{
    Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
}
```

## Pass a reference to a .NET object into native Box2D as IntPtr (eg. UserData)

If you want to pass a .NET object reference into native Box2D, like when tagging a Shape or Body with `userData`, you must pass in an `IntPtr` to the object. But .NET objects are not guaranteed to stay at the same address in memory.

The solution is to allocate a `NativeHandle` for your .NET object and pass *that* to Box2D. Example:

``` C#
_handle = NativeHandle.Alloc(ball); // allocate a IntPtr handle for the .NET object and return it as IntPtr.

var shapeDef = B2Api.b2DefaultShapeDef();
// now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
shapeDef.userData = _handle;
var circle = new b2Circle() { radius = 1 };
var b2ShapeId = B2Api.b2CreateCircleShape(b2BodyId, in shapeDef, in circle);
```

After this, when Box2D passes the `IntPtr` back to .NET somewhere (eg. through `b2X_GetUserData()`) you can get hold of the corresponding .NET object like this:

``` C#
var userDataIntPtr = B2Api.b2Shape_GetUserData(shapeId);
var ball = NativeHandle<Ball>.GetObjectFromIntPtr(userDataIntPtr);
```

Note that you must keep the `NativeHandle` alive as long as you want the `IntPtr` held by native Box2D to remain valid.
When you're fully done with it, eg. when the game object is removed from your game, you must not forget to free it, like this: 

``` C#
NativeHandle.Free(_handle);
```

> A tip to avoid needing to use NativeHandle with UserData: abuse the UserData pointer by setting it to your gameobject's numerical ID (a normal int or long). IntPtr is just a numerical value, it doesn't have to be an actual pointer address: `new IntPtr(entity.Id)` works just fine.

## Callbacks: setting struct.fields that are callback delegates.

> As of Box2dNet v3.1.5 all Box2D functions that have callback delegates parameters have a companion overload that takes in the strongly typed delegate. eg. use `b2World_SetPreSolveCallback(b2WorldId worldId, **b2PreSolveFcn fcn**, IntPtr context)`, not `b2World_SetPreSolveCallback(b2WorldId worldId, **IntPtr fcn**, IntPtr context)`

Some Box2D struct fields are delegate pointers. These are `IntPtr` and must be set to a pointer to a method with the same definition as the delegate requires.

To do this, you must:

* check the generated comments to find the identifier of the original C delegate type
* write or generate a method that matches the delegate
* assign a function pointer retrieved with `Marshal.GetFunctionPointerForDelegate` to the IntPtr struct field.

Here is an example:

``` C#
    ...
    var worldDef = b2DefaultWorldDef();
    worldDef.enqueueTask = Marshal.GetFunctionPointerForDelegate((b2EnqueueTaskCallback)EnqueueTaskCallback);
}

private static IntPtr EnqueueTaskCallback(IntPtr /* b2TaskCallback */ task, int itemCount, int minRange, IntPtr taskContext, IntPtr userContext)
{
    ...
}
```

# Samples

Most specific techniques are described in the manual above, but you can also check out the `Box2dNet.Samples` console app in this repo to see working code that gets you started on common usecases like detecting collisions.

# Regenerating the wrapper

Normally, you don't need to regenerate the wrapper yourself. I regenerate every few weeks for the latest version of Box2D. The resulting Win x64 DLLs (debug and release) are also included in this repo and in the Nuget package. 

But if you need to do it yourself anyway, perform these steps:

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

# History

## 2025/06/15: v3.1.7 for Box2D v3.1.1

* Regenerated for updated Box2D 
* Quite some changes in the Box2D original code since previous time. Box2dNetGen was refactored and adjusted accordingly.

> Box2D started using 3-part version labels messing up my version system where the first two parts are Box2D's version and the third is for Box2dNet changes. I will switch to another system as of Box2D v3.2.0

## 2025/06/15: v3.1.6 for Box2D v3.1

* Small adjustment for Godot. https://github.com/thomasvt/Box2DNet/issues/2

## 2025/05/06: v3.1.5 for Box2D v3.1

* Added Span-based convenience properties like `b2BodyEvents.moveEventsAsSpan`.
* Box2d API methods that take delegate pointers (IntPtr) were annoying to use as you couldn't see the actual delegate. Box2DNet now also generates an overload for these methods with the actual delegate as parameter. eg. `b2World_CastShape(b2WorldId worldId, in b2ShapeProxy proxy, Vector2 translation, b2QueryFilter filter, __b2CastResultFcn fcn__, IntPtr context)`

## 2025/05/04: v3.1.4 for Box2D v3.1

* Regenerated for updated Box2D
* Started publishing `Box2dNet` as a nuget package

... older history not logged ...
