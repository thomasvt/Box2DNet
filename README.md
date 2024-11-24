# Box2DNet - .NET wrapper for 'box2d v3.x'

## Description

This is a .NET 8.0 PInvoke wrapper for [Box2d v3.x](https://github.com/erincatto/box2d) for .NET games that have their own engine. 

A custom tool parses the Box2D C code and generates the C# wrapper code.

Next to the generated wrapper, a few reusable helpers are manually added to ease working with native pointers and wiring the new multithreading support of Box2d to .NET Task Parallel Library.

> This is not a rigorously tested wrapper project. I'm doing this for my own game but it turned out pretty complete, so I'm sharing it.

> This may not work in Unity, especially the .NET Task wrapping, it's meant for stand-alone use in .NET code, for instance with Monogame.

### License

You may do whatever you like with the code in this repo. Don't forget to respect the [Box2d v3.x](https://github.com/erincatto/box2d) license, though!

## Manual

Box2DNet uses no abstraction layer: you work with functions and types directly wrapping the original C constructs with the **exact same** identifier. So you can use the original [Box2d manual](https://box2d.org/documentation/) 'as is'.

### Include Box2DNet in your game

There's no nuget package. Just clone ```Box2DNet``` next to your game folder and include the ```.csproj``` into your game solution.

This will build the Box2DNet project along with your game. When you build in DEBUG it will use native ```box2dd.dll```, when you build in RELEASE it will use native ```box2d.dll```.

> The debug version ```box2dd.dll``` will quit your game with an error message when you did something wrong: this helps for debugging your programming mistakes.

### The Box2D API

All Box2D functions are available as C# methods in static class ```Box2dNet.Interop.B2Api```. Original comments are also available, so code completion is quite rich.

NOT included:

* the timer functions (b2CreateTimer, ..): use .NET timers :)
* b2World_Draw and b2DefaultDebugDraw: temporary, due to translation difficulties. I intend to manually port this code, as it will probably not change much in the future.
* b2DynamicTree_X: these were hard to wrap automatically. But most games don't need to use this directly, I think.

### Dealing with IntPtr

C pointers become ```IntPtr``` in C#, so you cannot see what struct they point to. To help with this, the original struct name is added in comment in the generated C# code:

Example:

``` C#
/// <summary>
/// Array of sensor begin touch events
/// </summary>
public IntPtr /* b2SensorBeginTouchEvent* */ beginEvents;
```

If the IntPtr is an array, like in the example, you can use C# extension method ```NativeArrayAsSpan``` to loop over the contents:

``` C#
var hitEvents = B2Api.b2World_GetContactEvents(b2WorldId);
// use helper extension method to efficiently read the native hitEvents array with little code:
foreach (var @event in hitEvents.beginEvents.NativeArrayAsSpan<b2ContactBeginTouchEvent>(hitEvents.beginCount))
{
    Console.WriteLine($"!!!!!!!   HIT detected between {@event.shapeIdA} and {@event.shapeIdB}");
}
```

If the IntPtr is for passing a game object into Box2D, like ```userData```, you can create a ```NativeHandle``` for your game object to get an IntPtr for it that you can pass to the native Box2D side:

``` C#
_handle = new NativeHandle<Ball>(this); // create a handle for the .NET object.

var shapeDef = B2Api.b2DefaultShapeDef();
// now tag the Box2d Shape with a handle to our .NET game object so we can always find the .NET game object back:
shapeDef.userData = _handle.IntPtr;
```

After this, you can get the .NET game object back from the IntPtr, eg. when getting hit events from Box2D:

``` C#
var ball = NativeHandle<Ball>.GetObjectFromIntPtr(B2Api.b2Shape_GetUserData(@event.shapeIdA));
```

> Don't forget to ```Dispose``` the NativeHandle instance (```_handle``` in the example) when you remove the game object from your game!

### Multi-threading support

There is .NET TPL wiring code for the new multi-threaded Task system that Box2D uses. Just use ```B2Api.b2DefaultWorldDef_WithDotNetTpl()``` instead of ```B2Api.b2DefaultWorldDef``` to create your Box2D world:

``` C#
var worldDef = useMultiThreading
    ? B2Api.b2DefaultWorldDef_WithDotNetTpl() // <---- this is all it takes for default multi threading
    : B2Api.b2DefaultWorldDef();

var b2WorldId = B2Api.b2CreateWorld(worldDef);
```

### Samples

Check out the ```Box2dNet.Samples``` console app to see working code that gets you started.

## Regenerating the wrapper

Currently, I regenerate every few weeks, as Erin pushes changes. The corresponding Win x64 DLLs (debug and release) are included in this repo too, so you won't have to regenerate yourself. 

### Regenerating yourself:

If you still want to do it yourself: the C# wrapper code can be regenerated with the companion tool ```Box2dWrap```, also in this repo. 
It's a naive C parsing + codegen tool, very specific to the Box2D codebase. It was meant for my eyes only, so it's not the easiest code to find your way in. You are warned :)

It expects commandline parameters: 

```Box2dWrap.exe <box2d-repo-root> <B2Api.cs-file-output>``` 

Example:

```Box2dWrap.exe C:\repos\box2d "C:\repos\Box2dNet\Box2dNet\Interop\B2Api.cs"```

### Building the Box2D DLLs

Steps to rebuild Box2D dlls:

* Clone the latest version of ```erincatto/box2d``` onto your PC
* in ```CMakeLists.txt``` around line 11 add a line ```option(BUILD_SHARED_LIBS "Build using shared libraries" ON)``` which makes it build to dll instead of statically linked lib
* run ```build.cmd``` -> generates a .sln in ```./build```
* if it did not open automatically, open the generated ```./build/box2d.sln``` in Visual Studio
* Rebuild the ```box2d``` project both in Debug and Release to get both .dlls ```box2dd.dll``` and ```box2d.dll```. (don't build the entire sln, it gives errors in the non-static build, atm)
* copy the 2 dlls from ```.\build\bin\Debug``` and ```.\build\bin\Release``` to the Box2dNet project and set to copy on build (currently I use the csproj directly in my game's .sln)
* rerun ```Box2DWrap``` to also regenerate the latests Box2d into the Box2dNet project file ```./Interop/B2Api.cs```.

The generator gives several warnings about the "exclude-list", which is deliberate, and therfore ok.

If you get other warnings or errors, though, some of the new C code is incompatible with my tool. You can let me know, and I will try to fix them.

