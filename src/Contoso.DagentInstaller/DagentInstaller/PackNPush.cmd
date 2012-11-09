@echo off
echo packing packages
pushd ..\..
nuget pack Contoso.Dagent.4\Contoso.Dagent.4.csproj -OutputDirectory nuget\packages -Build -Prop Configuration=Release
nuget push nuget\packages\Dagent.1.0.0.0.nupkg -s http://nuget.degdarwin.com
::copy nuget\packages\Dagent.1.0.0.0.nupkg C:\T_\Packages\Dagent.1.0.0.0.nupkg
popd
::pause