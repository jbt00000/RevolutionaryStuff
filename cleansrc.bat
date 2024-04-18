@echo off
setlocal enableextensions

:: Define the root directory to start the search from
set "ROOT_DIR=./src"

:: Remove 'obj' and 'bin' directories recursively
for /d /r "%ROOT_DIR%" %%d in (obj bin) do (
    if exist "%%d" (
        echo Removing directory: %%d
        rd /s /q "%%d"
    )
)

:: Define the root directory to start the search from
set "ROOT_DIR=./src.v2"

:: Remove 'obj' and 'bin' directories recursively
for /d /r "%ROOT_DIR%" %%d in (obj bin) do (
    if exist "%%d" (
        echo Removing directory: %%d
        rd /s /q "%%d"
    )
)

echo Done.
