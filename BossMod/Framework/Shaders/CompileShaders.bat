@echo off
setlocal EnableExtensions

REM Dx11ArenaRenderer shader compiler
REM Baseline: D3D_FEATURE_LEVEL_11_0 + Shader Model 5.0
REM
REM Optional:
REM   CompileShaders.bat "C:\full\path\to\fxc.exe"
REM
REM With no argument, this checks PATH and then installed Windows SDKs.

set "HERE=%~dp0"
set "OUT=%HERE%Compiled"
set "LOG=%HERE%shader_compile.log"
set "TMPLOG=%TEMP%\Dx11ArenaRenderer_FXC_%RANDOM%_%RANDOM%.tmp"
set "FXC=%~1"

REM Start a fresh log for this run. FXC output (including warnings/errors)
REM is copied to both the console and this file by the compile helpers below.
> "%LOG%" echo Dx11ArenaRenderer shader compile log
>> "%LOG%" echo Started: %date% %time%
>> "%LOG%" echo.

if defined FXC goto :validate_fxc

REM Try PATH first.
for /f "delims=" %%F in ('where fxc.exe 2^>nul') do (
    if not defined FXC set "FXC=%%F"
)
if defined FXC goto :validate_fxc

REM Search Windows 10/11 SDK versions, newest directory name first.
if defined ProgramFiles(x86) (
    for /f "delims=" %%D in ('dir /b /ad /o-n "%ProgramFiles(x86)%\Windows Kits\10\bin\10.*" 2^>nul') do (
        if not defined FXC if exist "%ProgramFiles(x86)%\Windows Kits\10\bin\%%D\x64\fxc.exe" (
            set "FXC=%ProgramFiles(x86)%\Windows Kits\10\bin\%%D\x64\fxc.exe"
        )
    )
)

REM Older SDK fallback.
if not defined FXC if defined ProgramFiles(x86) if exist "%ProgramFiles(x86)%\Windows Kits\8.1\bin\x64\fxc.exe" (
    set "FXC=%ProgramFiles(x86)%\Windows Kits\8.1\bin\x64\fxc.exe"
)

if not defined FXC (
    echo ERROR: fxc.exe was not found.
    echo Install a Windows SDK, add FXC to PATH, or pass its full path as argument 1.
    >> "%LOG%" echo ERROR: fxc.exe was not found.
    >> "%LOG%" echo Install a Windows SDK, add FXC to PATH, or pass its full path as argument 1.
    >> "%LOG%" echo Finished with errors: %date% %time%
    echo.
    echo Log saved to:
    echo   %LOG%
    pause
    exit /b 1
)

:validate_fxc
if exist "%FXC%" goto :fxc_ok

REM If argument 1 was an executable name rather than a full path, resolve through PATH.
for /f "delims=" %%F in ('where "%FXC%" 2^>nul') do (
    set "FXC=%%F"
    goto :fxc_ok
)

echo ERROR: FXC not found: "%FXC%"
>> "%LOG%" echo ERROR: FXC not found: "%FXC%"
>> "%LOG%" echo Finished with errors: %date% %time%
echo.
echo Log saved to:
echo   %LOG%
pause
exit /b 1

:fxc_ok
if not exist "%OUT%" mkdir "%OUT%"

echo Using FXC:
echo   %FXC%
echo.
echo Target baseline: vs_5_0 / ps_5_0
echo Log:
echo   %LOG%
echo.
>> "%LOG%" echo Using FXC: %FXC%
>> "%LOG%" echo Target baseline: vs_5_0 / ps_5_0
>> "%LOG%" echo.

call :compile mesh_vs.hlsl                      vs_5_0 mesh_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile primitive_triangle_stroke_vs.hlsl vs_5_0 primitive_triangle_stroke_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile mesh_ps.hlsl                      ps_5_0 mesh_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile world_overlay_vs.hlsl             vs_5_0 world_overlay_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile world_overlay_ps.hlsl             ps_5_0 world_overlay_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile stroke_vs.hlsl                    vs_5_0 stroke_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile stroke_ps.hlsl                    ps_5_0 stroke_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile world_line_vs.hlsl                vs_5_0 world_line_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile world_curve_vs.hlsl               vs_5_0 world_curve_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile projected_arrow_vs.hlsl           vs_5_0 projected_arrow_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile projected_arrow_ps.hlsl           ps_5_0 projected_arrow_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile projected_shape_vs.hlsl           vs_5_0 projected_shape_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile projected_shape_ps.hlsl           ps_5_0 projected_shape_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile text_vs.hlsl                      vs_5_0 text_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile text_ps.hlsl                      ps_5_0 text_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile sprite_ps.hlsl                    ps_5_0 sprite_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile world_text_vs.hlsl                 vs_5_0 world_text_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile world_text_ps.hlsl                 ps_5_0 world_text_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile analytic_vs.hlsl                  vs_5_0 analytic_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile analytic_ps.hlsl                  ps_5_0 analytic_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile outline_shape_vs.hlsl             vs_5_0 outline_shape_vs.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile outline_shape_ps.hlsl             ps_5_0 outline_shape_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile_define outline_shape_ps.hlsl      ps_5_0 outline_unclipped_ps.cso UNCLIPPED_OUTLINE=1
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile_define outline_shape_ps.hlsl      ps_5_0 outline_clip_edge_ps.cso CLIP_EDGE_ONLY=1
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

call :compile arena_sdf_outline_ps.hlsl         ps_5_0 arena_sdf_outline_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile arena_sdf_stencil_ps.hlsl         ps_5_0 arena_sdf_stencil_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile custom_sdf_fill_ps.hlsl           ps_5_0 custom_sdf_fill_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile custom_outline_ps.hlsl            ps_5_0 custom_outline_ps.cso
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)
call :compile_define custom_outline_ps.hlsl     ps_5_0 custom_clip_edge_ps.cso CLIP_EDGE_ONLY=1
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

echo.
echo All 29 Shader Model 5.0 shaders compiled successfully.
echo Output:
echo   %OUT%
echo Log:
echo   %LOG%
>> "%LOG%" echo.
>> "%LOG%" echo All 29 Shader Model 5.0 shaders compiled successfully.
>> "%LOG%" echo Output: %OUT%
>> "%LOG%" echo Finished: %date% %time%
if exist "%TMPLOG%" del /q "%TMPLOG%" >nul 2>&1
exit /b 0


:compile
echo [FXC] %~1  ^(%~2^) -^> %~3
>> "%LOG%" echo [FXC] %~1  ^(%~2^) -^> %~3
"%FXC%" /nologo /E main /T %~2 /O3 /Ges /Fo "%OUT%\%~3" "%HERE%%~1" > "%TMPLOG%" 2>&1
set "FXC_RC=%ERRORLEVEL%"
if exist "%TMPLOG%" (
    type "%TMPLOG%"
    type "%TMPLOG%" >> "%LOG%"
    del /q "%TMPLOG%" >nul 2>&1
)
if not "%FXC_RC%"=="0" (
    echo ERROR: Failed to compile %~1
    >> "%LOG%" echo ERROR: Failed to compile %~1 ^(exit code %FXC_RC%^)
    >> "%LOG%" echo Finished with errors: %date% %time%
    >> "%LOG%" echo.
    echo Log saved to:
    echo   %LOG%
    exit /b %FXC_RC%
)
>> "%LOG%" echo.
exit /b 0


:compile_define
echo [FXC] %~1  ^(%~2, %~4^) -^> %~3
>> "%LOG%" echo [FXC] %~1  ^(%~2, %~4^) -^> %~3
"%FXC%" /nologo /E main /T %~2 /O3 /Ges /D %~4 /Fo "%OUT%\%~3" "%HERE%%~1" > "%TMPLOG%" 2>&1
set "FXC_RC=%ERRORLEVEL%"
if exist "%TMPLOG%" (
    type "%TMPLOG%"
    type "%TMPLOG%" >> "%LOG%"
    del /q "%TMPLOG%" >nul 2>&1
)
if not "%FXC_RC%"=="0" (
    echo ERROR: Failed to compile %~1 with %~4
    >> "%LOG%" echo ERROR: Failed to compile %~1 with %~4 ^(exit code %FXC_RC%^)
    >> "%LOG%" echo Finished with errors: %date% %time%
    >> "%LOG%" echo.
    echo Log saved to:
    echo   %LOG%
    exit /b %FXC_RC%
)
>> "%LOG%" echo.
exit /b 0
