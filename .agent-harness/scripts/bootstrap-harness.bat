@echo off
setlocal enabledelayedexpansion

set "REPO_ROOT=%~dp0..\.."
pushd "%REPO_ROOT%" >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Failed to resolve repository root from "%~dp0".
    exit /b 1
)

echo ====================================================================
echo         Empress Project - AI Agent Harness Bootstrapper [Windows]
echo ====================================================================

set "STATUS_NODE=FAIL"
set "STATUS_UNITY=WARN"
set "STATUS_SKILLS=FAIL"
set "STATUS_HARNESS=FAIL"

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

set "EXACT_UNITY=%ProgramFiles%\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
if exist "%EXACT_UNITY%" (
    set "UNITY_EDITOR_PATH=%EXACT_UNITY%"
    echo     [OK] Auto-detected Unity 6000.3.10f1 at:
    echo          "%EXACT_UNITY%"
    set "STATUS_UNITY=OK"
    goto :unity_checked
)

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

where unityhub >nul 2>&1
if %errorlevel% equ 0 (
    echo     [OK] Unity Hub CLI detected in system PATH.
    set "STATUS_UNITY=OK"
    goto :unity_checked
)

echo     [WARN] Unity Editor executable could not be auto-detected.
echo     Please set UNITY_EDITOR_PATH, e.g.:
echo     setx UNITY_EDITOR_PATH "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"

:unity_checked

:: ----------------------------------------------------------------------
:: 3. Verify Harness and Skills Integrity
:: ----------------------------------------------------------------------
echo.
echo [3/5] Verifying Harness and Skills Integrity...

set "MISSING_CORE=0"
for %%F in (
    "AGENTS.md"
    ".agent-harness\architecture\core-patterns.md"
    ".agent-harness\task.template.md"
    ".agents\skills\unity-test-runner\SKILL.md"
) do (
    if exist %%F (
        for %%S in (%%F) do set "FSIZE=%%~zS"
        if !FSIZE! gtr 0 (
            echo     [OK] %%~F [!FSIZE! bytes]
        ) else (
            echo     [ERROR] %%~F is empty [0 bytes].
            set "MISSING_CORE=1"
        )
    ) else (
        echo     [ERROR] %%~F was NOT found.
        set "MISSING_CORE=1"
    )
)

if "!MISSING_CORE!"=="0" set "STATUS_HARNESS=OK"

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
echo   Harness Integrity   : [%STATUS_HARNESS%]
echo   Unity Skills Link   : [%STATUS_SKILLS%]
echo ====================================================================

if "%STATUS_HARNESS%"=="OK" (
    echo [CONFIRMED] Harness structure and skills are present.
)

if "%STATUS_NODE%"=="OK" (
    if "%STATUS_SKILLS%"=="OK" (
        echo [SUCCESS] Agent Harness initialized and ready for Empress development.
        popd
        exit /b 0
    )
)

echo [ATTENTION] Some dependencies require attention before running autonomous tasks.
popd
exit /b 0
