This generates a C# wrapper (PInvoke) from the Box2d v3 source code.

* in CMAkeLists.txt at line 11 i added ```option(BUILD_SHARED_LIBS "Build using shared libraries" ON)```
  which then builds to dll instead of statically linked lib
* then run build.cmd
* this creates a .sln (and opens it in VS.net)
* build this in Debug and Release to get both box2d.dll (release) and box2dd.dll (debug)