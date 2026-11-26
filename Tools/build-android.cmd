@echo off
setlocal
set "LOE_ROOT=%~dp0.."
if not defined UNITY_EDITOR_EXE set "UNITY_EDITOR_EXE=%ProgramFiles%\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe"
if not exist "%UNITY_EDITOR_EXE%" (
  echo ERROR: Unity 6000.3.27f1 not found. Set UNITY_EDITOR_EXE to your Unity.exe location.
  exit /b 2
)
cd /d "%LOE_ROOT%\Client"
if errorlevel 1 exit /b 3
"%UNITY_EDITOR_EXE%" -batchmode -nographics -quit -projectPath "%CD%" -executeMethod LegacyOfEternity.Editor.AndroidPreviewBuilder.Build -logFile "%LOE_ROOT%\Docs\unity-android-build.log"
set "LOE_EXIT=%ERRORLEVEL%"
echo Unity Android build exit code: %LOE_EXIT%
exit /b %LOE_EXIT%
