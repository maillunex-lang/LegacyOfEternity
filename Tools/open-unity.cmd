@echo off
setlocal
set "LOE_PROJECT=%~dp0..\Client"
if defined UNITY_EDITOR_EXE if exist "%UNITY_EDITOR_EXE%" goto launch
set "UNITY_EDITOR_EXE=%ProgramFiles%\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe"
if exist "%UNITY_EDITOR_EXE%" goto launch
set "UNITY_EDITOR_EXE=%ProgramFiles(x86)%\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe"
if exist "%UNITY_EDITOR_EXE%" goto launch
echo Unity 6000.3.27f1 not found. Install via Unity Hub, or set UNITY_EDITOR_EXE to the Unity.exe path.
exit /b 2
:launch
start "" "%UNITY_EDITOR_EXE%" -projectPath "%LOE_PROJECT%"
