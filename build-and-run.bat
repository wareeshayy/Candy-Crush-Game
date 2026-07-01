@echo off
setlocal
cd /d "%~dp0"

echo === Candy Crush Build and Run ===
echo.

set "GPP="
where g++ >nul 2>&1 && set "GPP=g++"

if not defined GPP (
    for /r "%~dp0tools" %%G in (g++.exe) do set "GPP=%%G"
)

if not defined GPP (
    echo No C++ compiler found.
    echo.
    echo Install one of these, then run this script again:
    echo   1. Visual Studio with "Desktop development with C++"
    echo   2. MinGW-w64 / WinLibs and add g++ to PATH
    echo   3. Or download llvm-mingw to tools\ folder:
    echo      https://github.com/mstorsjo/llvm-mingw/releases
    echo.
    pause
    exit /b 1
)

echo Using compiler: %GPP%
echo Compiling Source.cpp...

"%GPP%" -o "%~dp0CandyCrush.exe" "%~dp0Candy-Crush-Cpp\Source.cpp"
if errorlevel 1 (
    echo.
    echo COMPILE FAILED.
    pause
    exit /b 1
)

echo.
echo Build successful: %~dp0CandyCrush.exe
echo Starting game...
echo.
start "" "%~dp0CandyCrush.exe"
exit /b 0
