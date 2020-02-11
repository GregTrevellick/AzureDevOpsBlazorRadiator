@echo off
cls
dotnet build "C:\_Cgit\AzureDevOpsBlazorRadiator\Src\BlazingPoints.sln" /p:configuration=release
rem pause

rem vsmp_pat not used
cd "C:\_Cgit\AzureDevOpsBlazorRadiator\Src\BlazingPoints"
tfx extension create --rev-version --manifest-globs vss-extension.private.json
rem manually upload the vsix into vsmp edit screen
