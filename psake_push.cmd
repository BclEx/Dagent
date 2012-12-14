@echo off
echo Pushing Dagent
PowerShell -Command ".\psake.ps1"

tools\NuGet.exe push Release\Dagent.1.0.0.nupkg -s http://nuget.degdarwin.com

::pause