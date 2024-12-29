# Intro

This is Box2dNet, a 'close-to-the-metal' C# wrapper for box2d **v3** (which is the version that came out mid 2024)

Latest regen was **2024/12/22**

# Description

This is a .NET 8.0 PInvoke wrapper for [Box2d v3](https://github.com/erincatto/box2d) for .NET games that have their own engine. 

A custom tool parses the Box2D C code and generates the C# wrapper code.

Next to the generated wrapper, a growing set of convenient helpers is included. eg, for simplifying code to work with the native pointers or wiring the new multithreading support of Box2d to .NET Task Parallel Library.

> I don't use Unity and therefore cannot support it. This wrapper is meant for stand-alone use in .NET code, for instance with Monogame.

# License

You may do whatever you like with the code in this repo. Don't forget to respect the [Box2d v3.x](https://github.com/erincatto/box2d) license, though!

# Manual

Box2DNet has no abstraction layer: you work directly with functions and types mapped from the original C definitions with the same names. You can therefore use the original [Box2d manual](https://box2d.org/documentation/) 'as is' for all your functional needs.

The rest of the manual below talks only about the wrapper technicalities.

## How to include Box2DNet in your game?

There's no nuget package. Just clone the repo next to your game folder and include the ```box2dnet.csproj``` into your game solution.

This will build the Box2DNet project along with your game. When you build in DEBUG it will use the native debug dll ```box2dd.dll```, when you build in RELEASE it will use the native production dll ```box2d.dll```.

> The debug version ```box2dd.dll``` will quit your game with assertion errors when you did something wrong: this helps for debugging your mistakes.

## What's included?

All Box2D API functions are available as C# methods in static class ```Box2dNet.Interop.B2Api```. Original comments are also available, so code completion is quite rich.

NOT included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2World_Draw and b2DefaultDebugDraw: hard to support this code in the codegen tool. I intend to manually port this code some day, as it will probably not change much in the future.
* b2DynamicTree_X: also hard to support these in the codegen tool. But most games don't need to use this directly.

## Dealing with IntPtr

A lot of API functions use pointers as parameters, or in structs. In C# these become ```IntPtr```, so you cannot see which `struct` they originally pointed to in C. To help with this, xml comments showing the original c type are provided wherever applicable in C#: parameters, return types, struct fields.

### Delegate IntPtr parameters 

Some functions require you to pass in a callback method as delegate. These are always IntPtr, but we can see the actual delegate type for that parameter in the generated comment. You should be able to see it through your IDE's tooltips for that parameter, or go to the source and check the comment manually.

Here is an example how to call `b2World_OverlapCircle` with an `IntPtr` callback delegate of type `b2OverlapResultFcn`:

``` C#
public void Update()
{
    var circle = new b2Circle(Vector2.Zero, 10);
    var filter = new b2QueryFilter(PhysicsLayer.Query, PhysicsLayer.RobotCore);
    _list.Clear();
    B2Api.b2World_OverlapCircle(_b2WorldId, circle, b2Transform.Zero, filter, 
        Marshal.GetFunctionPointerForDelegate((b2OverlapResultFcn)QueryCallback), IntPtr.Zero);
}

private bool QueryCallback(b2ShapeId shapeId, IntPtr context)
{
    _list.Add(shapeId); // or get a corresponding .NET object using 'userData' (see samples) or some dictionary.
    return true;
}
```

### Reading native arrays from IntPtr

Some structs you receive from native Box2D contain arrays. For example `b2ContactEvents.beginEvents`, which is an IntPtr, shows in its parameter comment that you should read it as an array of `b2SensorBeginTouchEvent`:

To do this, you can use the provided convenience method `NativeArrayAsSpan` to loop over the native contents. Like this:

``` C#
var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
// use helper extension method to efficiently read the native hitEvents array with little code:
foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
{
    Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
}
```

Note that you must know the size of the array, which is always provided by Box2D in a sibling field.

### Pass a reference to my .NET object into native Box2D as IntPtr

If you want to pass a game object reference into Box2D, like `userData`, you must also do this as an IntPtr.

To do this, you allocate a `NativeHandle` for your .NET object and pass the handle's `IntPtr` to Box2D. Example:

``` C#
_handle = NativeHandle.Alloc(ball); // allocate a IntPtr handle for the .NET object and return it as IntPtr.

var shapeDef = B2Api.b2DefaultShapeDef();
// now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
shapeDef.userData = _handle;
```

After this, when Box2D passes the `IntPtr` back to you somewhere, you can get hold of the corresponding .NET object like this:

``` C#
var userDataIntPtr = B2Api.b2Shape_GetUserData(@event.shapeIdA);
var ball = NativeHandle<Ball>.GetObjectFromIntPtr(userDataIntPtr);
```

Note that you must keep the `NativeHandle` alive as long as you want the `IntPtr` held by native Box2D to remain valid.
When you're fully done with it, eg. when the game object is removed from your game, you must not forget to free it, like this: 

``` C#
NativeHandle.Free(_handle);
```

## Multi-threading support

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

## Samples

Most specific techniques are described in the manual above, but you can also check out the `Box2dNet.Samples` console app in this repo to see working code that gets you started on common usecases like detecting collisions.

# Regenerating the wrapper

Currently, I regenerate every few weeks. The corresponding Win x64 DLLs (debug and release) are included in this repo too, so generally, you won't have to regenerate yourself. 

But if you must:

* regenerate the existing `B2Api.cs` with the `Box2dWrap` tool.
* rebuild and replace the existing two native dlls in the Box2dNet project

## Regenerate the C# code

The C# wrapper code can be regenerated with the companion codegen tool ```Box2dWrap```, also in this repo. 
It's a naive C parsing + codegen tool, very specific to the Box2D codebase. It was meant for my eyes only, so it's not the easiest code to find your way in. You are warned :)

It expects commandline parameters: 

`Box2dWrap.exe <box2d-repo-root> <B2Api.cs-file-output>`

Example:

`Box2dWrap.exe C:\repos\box2d "C:\repos\Box2dNet\Box2dNet\Interop\B2Api.cs"`

> Make sure the second parameter points to the existing `B2Api.cs` file in your local Box2dNet repo so it gets overwritten.

The generator gives several warnings about the "exclude-list", which is deliberate, and therefore to be ignored.
If you get other warnings or errors, though, some of the new C code is incompatible with my tool. You can let me know if I'm not working on it already, or you can give it a shot yourself and send me a PR.

## Rebuilding the Box2D DLLs

(make sure you have `cmake` installed, (use the .msi from https://cmake.org/download))

* Clone the latest version of ```erincatto/box2d``` onto your PC
* in ```CMakeLists.txt``` around line 11 add a line ```option(BUILD_SHARED_LIBS "Build using shared libraries" ON)``` which makes it build to dll instead of statically linked lib
* run ```build.cmd``` -> generates a .sln in ```./build```
* if it did not open automatically, open the generated ```./build/box2d.sln``` in Visual Studio
* **Re**build the ```box2d``` project both in Debug and Release to get both debug dll ```box2dd.dll``` and production dll ```box2d.dll```. (no need to build the entire sln, i have seen the other projects give errors even)
* copy the 2 dlls from ```.\build\bin\Debug``` and ```.\build\bin\Release``` to the Box2dNet project and set to *copy on build*



