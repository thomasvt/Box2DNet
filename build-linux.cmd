@echo off

echo Building box2d (release)

docker run --rm -v C:\repos\box2d:/src -w /src gcc:13 bash -c "apt-get update -qq && apt-get install -y -qq cmake >/dev/null && cmake -S . -B build-linux -DBUILD_SHARED_LIBS=ON -DBOX2D_SAMPLES=OFF -DBOX2D_UNIT_TESTS=OFF -DCMAKE_BUILD_TYPE=Release && cmake --build build-linux -j"

echo Building box2d (debug)

docker run --rm -v C:\repos\box2d:/src -w /src gcc:13 bash -c "apt-get update -qq && apt-get install -y -qq cmake >/dev/null && cmake -S . -B build-linux-debug -DBUILD_SHARED_LIBS=ON -DBOX2D_SAMPLES=OFF -DBOX2D_UNIT_TESTS=OFF -DCMAKE_BUILD_TYPE=Debug && cmake --build build-linux-debug -j"

echo Builds are now in
echo * C:\repos\box2d\build-linux\src
echo * C:\repos\box2d\build-linux-debug\src
pause