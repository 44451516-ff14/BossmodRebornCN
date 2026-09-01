@echo off
setlocal EnableExtensions

REM Generates BossMod's immutable arena MSDF atlas.
REM
REM Usage:
REM   BuildArenaFont.bat "C:\path\to\msdf-atlas-gen.exe" "C:\path\to\Inter-Medium.otf" "C:\path\to\fa-solid-900.ttf"
REM
REM The icon font should be Font Awesome 7 Free Solid to match Dalamud's FontAwesomeIcon codepoints.

set "HERE=%~dp0"
set "OUT=%HERE%Compiled"
set "GEN=%~1"
set "TEXT_FONT=%~2"
set "ICON_FONT=%~3"

if not defined GEN (
    for /f "delims=" %%F in ('where msdf-atlas-gen.exe 2^>nul') do if not defined GEN set "GEN=%%F"
)
if not defined GEN (
    echo ERROR: msdf-atlas-gen.exe not found. Pass it as argument 1 or put it on PATH.
    exit /b 1
)
if not exist "%GEN%" (
    echo ERROR: msdf-atlas-gen not found: "%GEN%"
    exit /b 1
)
if not defined TEXT_FONT (
    set "TEXT_FONT=%HERE%Source\Inter-SemiBold.otf"
)
if not defined ICON_FONT (
    set "ICON_FONT=%HERE%Source\Font Awesome 7 Free-Solid-900.otf"
)
if not exist "%TEXT_FONT%" (
    echo ERROR: text font not found: "%TEXT_FONT%"
    exit /b 1
)
if not exist "%ICON_FONT%" (
    echo ERROR: icon font not found: "%ICON_FONT%"
    exit /b 1
)
if not exist "%OUT%" mkdir "%OUT%"

echo Generating immutable BossMod arena MSDF atlas...
"%GEN%" ^
  -font "%TEXT_FONT%" -fontname ArenaText -charset "%HERE%arena-text.charset" ^
  -and ^
  -font "%ICON_FONT%" -fontname ArenaIcons -charset "%HERE%arena-icons.charset" ^
  -type msdf -format bin -size 64 -pxrange 8 -yorigin bottom -potr ^
  -imageout "%OUT%\arena_font_msdf.rgb" ^
  -json "%OUT%\arena_font_msdf.json"
if errorlevel 1 (
    echo ERROR: MSDF generation failed.
    exit /b 1
)

echo.
echo Generated:
echo   %OUT%\arena_font_msdf.rgb
echo   %OUT%\arena_font_msdf.json
echo.
echo Recompile text_ps.hlsl after changing -pxrange; the shader currently expects 8.
exit /b 0
