@echo off
echo Refreshing packages:
pushd ..\BclEx-Abstract
PowerShell -Command ".\psake.ps1"
popd

:: BclEx-Abstract
set SRC=C:\_GITHUB\BclEx-Abstract\Release
::
echo BclEx-Abstract.1.0.0
pushd packages\BclEx-Abstract.1.0.0
xcopy %SRC%\BclEx-Abstract.1.0.0.nupkg . /Y/Q
..\..\tools\7za.exe x -y BclEx-Abstract.1.0.0.nupkg -ir!lib
popd
::
echo BclEx-Abstract.Log4Net.1.0.0
pushd packages\BclEx-Abstract.Log4Net.1.0.0
xcopy %SRC%\BclEx-Abstract.Log4Net.1.0.0.nupkg . /Y/Q
..\..\tools\7za.exe x -y BclEx-Abstract.Log4Net.1.0.0.nupkg -ir!lib
popd
::
echo BclEx-Abstract.RhinoServiceBus.1.0.0
pushd packages\BclEx-Abstract.RhinoServiceBus.1.0.0
xcopy %SRC%\BclEx-Abstract.RhinoServiceBus.1.0.0.nupkg . /Y/Q
..\..\tools\7za.exe x -y BclEx-Abstract.RhinoServiceBus.1.0.0.nupkg -ir!lib
popd
::
echo BclEx-Abstract.Unity.1.0.0
pushd packages\BclEx-Abstract.Unity.1.0.0
xcopy %SRC%\BclEx-Abstract.Unity.1.0.0.nupkg . /Y/Q
..\..\tools\7za.exe x -y BclEx-Abstract.Unity.1.0.0.nupkg -ir!lib
popd

::pause