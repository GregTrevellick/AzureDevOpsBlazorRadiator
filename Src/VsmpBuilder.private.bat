@echo off
cls
dotnet build "D:\_Dgit\_MINE_ACTIVE\AzureDevOpsBlazorRadiator\Src\BlazingPoints.sln" /p:configuration=release
rem pause

rem vsmp_pat not used
cd "D:\_Dgit\_MINE_ACTIVE\AzureDevOpsBlazorRadiator\Src\BlazingPoints"
tfx extension create --rev-version --manifest-globs vss-extension.private.json
rem manually upload the vsix into vsmp edit screen
