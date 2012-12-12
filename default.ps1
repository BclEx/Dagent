properties { 
  $base_dir = resolve-path .
  $nuspecs_dir = "$base_dir\nuspecs"
  $build_dir = "$base_dir\build\"
  $release_dir = "$base_dir\Release"
  $sln_file = "$base_dir\Dagent.sln"
  $tools_dir = "$base_dir\tools"
  $version = "1.0.0" #Get-Version-From-Git-Tag
  $config = "Release"
  $run_tests = $true
}
Framework "4.0"

#include .\psake_ext.ps1
	
task default -depends Package

task Clean {
	remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue
}

task Init -depends Clean {
	new-item $release_dir -itemType directory 
}

task Compile -depends Init {
	msbuild $sln_file /p:"OutDir=$build_dir;Configuration=$config"
}

task Test -depends Compile -precondition { return $run_tests }{
	$old = pwd
	cd $build_dir
	& $tools_dir\xUnit\xunit.console.clr4.exe "$build_dir\Contoso.Dagent.Tests.dll" /noshadow
	cd $old		
}

task Dependency {
	$package_files = @(Get-ChildItem src -include *packages.config -recurse)
	foreach ($package in $package_files)
	{
		Write-Host $package.FullName
		& $tools_dir\NuGet.exe install $package.FullName -o packages
	}
}

task Release -depends Dependency, Compile, Test {
	cd $build_dir
	& $tools_dir\7za.exe a $release_dir\Contoso.Dagent.zip `
		*\Contoso.Dagent.dll `
		*\Contoso.Dagent.Nuget.dll `
    	..\license.txt
	if ($lastExitCode -ne 0) {
		throw "Error: Failed to execute ZIP command"
    }
}

task Package -depends Release {
	$spec_files = @(Get-ChildItem $nuspecs_dir)
	foreach ($spec in $spec_files)
	{
		& $tools_dir\NuGet.exe pack $spec.FullName -o $release_dir -Version $version
	}
}
