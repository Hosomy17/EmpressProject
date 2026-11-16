@echo off
setlocal enabledelayedexpansion

echo ====================================================================
echo         Empress Project — AI Agent Harness Bootstrapper [Windows]
echo ====================================================================

set "STATUS_NODE=FAIL"
set "STATUS_UNITY=WARN"
set "STATUS_SKILLS=FAIL"
set "STATUS_SOT=FAIL"

:: ----------------------------------------------------------------------
:: 1. Verify Node.js and npx
:: ----------------------------------------------------------------------
echo.
echo [1/5] Verifying Node.js and npx environment...
where node >nul 2>&1
if %errorlevel% neq 0 (
    echo     [ERROR] Node.js is not installed or not in PATH.
    echo     Please install Node.js v18+ from https://nodejs.org/
    goto :node_done
)

for /f "tokens=*" %%v in ('node -v 2^>nul') do set "NODE_VER=%%v"
echo     [OK] Node.js !NODE_VER! found.

where npx >nul 2>&1
if %errorlevel% neq 0 (
    echo     [ERROR] npx is not installed or not in PATH.
    goto :node_done
)

for /f "tokens=*" %%v in ('npx -v 2^>nul') do set "NPX_VER=%%v"
echo     [OK] npx !NPX_VER! found.
set "STATUS_NODE=OK"

:node_done

:: ----------------------------------------------------------------------
:: 2. Verify Unity CLI / Editor Executable (6000.3.10f1)
:: ----------------------------------------------------------------------
echo.
echo [2/5] Verifying Unity CLI installation (6000.3.10f1)...

if defined UNITY_EDITOR_PATH (
    if exist "%UNITY_EDITOR_PATH%" (
        echo     [OK] Using custom UNITY_EDITOR_PATH:
        echo          "%UNITY_EDITOR_PATH%"
        set "STATUS_UNITY=OK"
        goto :unity_checked
    ) else (
        echo     [WARN] UNITY_EDITOR_PATH was defined but file was not found:
        echo            "%UNITY_EDITOR_PATH%"
    )
)

:: Check exact project version first
set "EXACT_UNITY=%ProgramFiles%\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
if exist "%EXACT_UNITY%" (
    set "UNITY_EDITOR_PATH=%EXACT_UNITY%"
    echo     [OK] Auto-detected Unity 6000.3.10f1 at:
    echo          "%EXACT_UNITY%"
    set "STATUS_UNITY=OK"
    goto :unity_checked
)

:: Fallback: Check default Unity Hub Editor installation path
set "HUB_EDITOR_DIR=%ProgramFiles%\Unity\Hub\Editor"
if exist "%HUB_EDITOR_DIR%" (
    for /f "delims=" %%D in ('dir /b /ad /o-n "%HUB_EDITOR_DIR%" 2^>nul') do (
        if exist "%HUB_EDITOR_DIR%\%%D\Editor\Unity.exe" (
            set "DETECTED_UNITY=%HUB_EDITOR_DIR%\%%D\Editor\Unity.exe"
            echo     [WARN] Project requires 6000.3.10f1 but found %%D:
            echo            "!DETECTED_UNITY!"
            set "STATUS_UNITY=OK"
            set "UNITY_EDITOR_PATH=!DETECTED_UNITY!"
            goto :unity_checked
        )
    )
)

:: Check unityhub CLI
where unityhub >nul 2>&1
if %errorlevel% equ 0 (
    echo     [OK] Unity Hub CLI detected in system PATH.
    set "STATUS_UNITY=OK"
    goto :unity_checked
)

echo     [WARN] Unity Editor executable could not be auto-detected.
echo     Please set the UNITY_EDITOR_PATH environment variable, e.g.:
echo     setx UNITY_EDITOR_PATH "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"

:unity_checked

:: ----------------------------------------------------------------------
:: 3. Verify Source of Truth (SoT) Integrity
:: ----------------------------------------------------------------------
echo.
echo [3/5] Verifying Architecture Source of Truth [SoT]...

set "SOT_FILE="
if exist ".agent-harness\source-of-truth.md" set "SOT_FILE=.agent-harness\source-of-truth.md"
if not defined SOT_FILE (
    if exist ".agent-harness\source-of-truth.template.md" set "SOT_FILE=.agent-harness\source-of-truth.template.md"
)

if not defined SOT_FILE (
    echo     [ERROR] source-of-truth.md was NOT found in .agent-harness/
    goto :sot_done
)

for %%F in ("!SOT_FILE!") do set "SOT_SIZE=%%~zF"
if !SOT_SIZE! gtr 0 (
    echo     [OK] Source of Truth verified and readable:
    echo          "!SOT_FILE!" [!SOT_SIZE! bytes]
    set "STATUS_SOT=OK"
) else (
    echo     [ERROR] "!SOT_FILE!" is empty [0 bytes].
)

set "TASK_TPL_FILE="
if exist ".agent-harness\task.template.md" set "TASK_TPL_FILE=.agent-harness\task.template.md"
if defined TASK_TPL_FILE (
    for %%F in ("!TASK_TPL_FILE!") do set "TPL_SIZE=%%~zF"
    if !TPL_SIZE! gtr 0 (
        echo     [OK] Task template verified and readable:
        echo          "!TASK_TPL_FILE!" [!TPL_SIZE! bytes]
    )
) else (
    echo     [WARN] task.template.md was NOT found in .agent-harness/
)

:sot_done

:: ----------------------------------------------------------------------
:: 4. Prepare Workspace Runtime Directories
:: ----------------------------------------------------------------------
echo.
echo [4/5] Ensuring workspace runtime directories exist...
if not exist "Logs" (
    mkdir "Logs" >nul 2>&1
    echo     [OK] Created Logs directory.
) else (
    echo     [OK] Logs directory present.
)
if not exist ".agent-harness\tasks" (
    mkdir ".agent-harness\tasks" >nul 2>&1
    echo     [OK] Created .agent-harness\tasks directory.
) else (
    echo     [OK] .agent-harness\tasks directory present.
)
if not exist "Assets\Editor\AgentAutomation" (
    mkdir "Assets\Editor\AgentAutomation" >nul 2>&1
    echo     [OK] Created Assets\Editor\AgentAutomation directory.
) else (
    echo     [OK] Assets\Editor\AgentAutomation directory present.
)

:: ----------------------------------------------------------------------
:: 5. Initialize Unity Skills (Unity-Technologies/skills)
:: ----------------------------------------------------------------------
echo.
echo [5/5] Initializing Unity Skills ecosystem...
if "%STATUS_NODE%"=="OK" (
    echo     Registering Unity-Technologies/skills via npx...
    call npx --yes skills add Unity-Technologies/skills >nul 2>&1
    if !errorlevel! equ 0 (
        echo     [OK] Unity AI Agent Skills registered successfully.
        set "STATUS_SKILLS=OK"
    ) else (
        echo     [WARN] Automatic registration returned non-zero. Verifying skills CLI...
        call npx --yes skills --version >nul 2>&1
        if !errorlevel! equ 0 (
            echo     [OK] npx skills CLI is operational.
            set "STATUS_SKILLS=OK"
        ) else (
            echo     [ERROR] Failed to run 'npx skills'. Check internet connectivity or npm permissions.
        )
    )
) else (
    echo     [SKIP] Skipping skills initialization due to missing Node/npx.
)

:: ----------------------------------------------------------------------
:: Summary Report
:: ----------------------------------------------------------------------
echo.
echo ====================================================================
echo                     BOOTSTRAP SUMMARY
echo ====================================================================
echo   Project             : Empress (2D Action-Platformer)
echo   Unity Version       : 6000.3.10f1
echo   Node / npx CLI      : [%STATUS_NODE%]
echo   Unity Editor CLI    : [%STATUS_UNITY%]
echo   Architecture SoT    : [%STATUS_SOT%]
echo   Unity Skills Link   : [%STATUS_SKILLS%]
echo ====================================================================

if "%STATUS_SOT%"=="OK" (
    echo [CONFIRMED] Source of Truth is active and ready for agent guidance.
)

if "%STATUS_NODE%"=="OK" (
    if "%STATUS_SKILLS%"=="OK" (
        echo [SUCCESS] Agent Harness initialized and ready for Empress development.
        exit /b 0
    )
)

echo [ATTENTION] Some dependencies require attention before running autonomous tasks.
exit /b 0
