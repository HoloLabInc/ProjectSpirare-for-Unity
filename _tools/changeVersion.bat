@echo off

if "%~1"=="" (
  echo Please specify version
  echo Example: change-version.bat 1.0.0
  exit /b 1
)

set "version=%~1"
set "basedir=%~dp0.."

:: update packages in %basedir%/packages folder
for /D %%d in (%basedir%\packages\*) do (
  pushd %%d
  npm version %version%
  popd
)

:: update unity projects in %basedir%/unity folder
for /D %%d in (%basedir%\unity\*) do (
  if exist "%%d\ProjectSettings\ProjectSettings.asset" (
    pushd "%%d\ProjectSettings"
    powershell -Command "(Get-Content ProjectSettings.asset) -replace 'bundleVersion: .*', 'bundleVersion: %version%' | Set-Content ProjectSettings.asset"
    popd
  )
)

cd /d %basedir%
