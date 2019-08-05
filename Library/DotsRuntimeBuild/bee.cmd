@ECHO OFF
set bee=%~dp0..\PackageCache\com.unity.tiny@0.16.1-preview\DotsPlayer\bee~\bee.exe
if [%1] == [] (%bee% -t) else (%bee% %*)
