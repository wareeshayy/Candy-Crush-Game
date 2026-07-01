@echo off
setlocal
cd /d "%~dp0Unity-CandyCrush"

set "HUB=C:\Program Files\Unity Hub\Unity Hub.exe"
if exist "%HUB%" (
    echo Opening Unity Hub with Unity-CandyCrush project...
    start "" "%HUB%" -- --projectPath "%~dp0Unity-CandyCrush"
    exit /b 0
)

for /d %%E in ("C:\Program Files\Unity\Hub\Editor\*") do (
    if exist "%%E\Editor\Unity.exe" (
        echo Opening Unity Editor...
        start "" "%%E\Editor\Unity.exe" -projectPath "%~dp0Unity-CandyCrush"
        exit /b 0
    )
)

echo Unity is not installed.
echo.
echo To run the Unity version:
echo   1. Install Unity Hub from https://unity.com/download
echo   2. Install Unity 2022.3 LTS
echo   3. Open the Unity-CandyCrush folder in Unity Hub
echo   4. Menu: Candy Crush - Setup Full Project
echo   5. Open Assets/Scenes/MainGame.unity and press Play
echo.
pause
exit /b 1
