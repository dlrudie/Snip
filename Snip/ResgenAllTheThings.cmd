@echo off
setlocal enabledelayedexpansion

set sourceDir=%~1
set targetDir=%~2

set resgenPath="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\resgen.exe"

set fileToResgen[0]="%sourceDir%Resources\Strings"
set fileToResgen[1]="%sourceDir%Resources\Strings.de-AT"
set fileToResgen[2]="%sourceDir%Resources\Strings.de-CH"
set fileToResgen[3]="%sourceDir%Resources\Strings.de-DE"
set fileToResgen[4]="%sourceDir%Resources\Strings.fr-FR"
set fileToResgen[5]="%sourceDir%Resources\Strings.nb-NO"
set fileToResgen[6]="%sourceDir%Resources\Strings.nl-NL"
set fileToResgen[7]="%sourceDir%Resources\Strings.sv-SE"


%resgenPath% /compile !fileToResgen[0]!.txt !fileToResgen[1]!.txt !fileToResgen[2]!.txt !fileToResgen[3]!.txt !fileToResgen[4]!.txt !fileToResgen[5]!.txt !fileToResgen[6]!.txt !fileToResgen[7]!.txt

rem for /l %%n in (0,1,6) do (
rem     copy /Y !fileToResgen[%%n]!.resources "bin\Debug\Resources"
rem )

if not exist "%targetDir%Resources\nul" mkdir "%targetDir%Resources"

for /f "tokens=2 delims==" %%s in ('set fileToResgen[') do copy /Y %%s.resources "%targetDir%Resources"
