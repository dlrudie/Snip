@echo off
setlocal enabledelayedexpansion

set sourceDir=%~1
set targetDir=%~2

set resgenPath="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\resgen.exe"

set fileToResgen[0]="%sourceDir%Resources\Strings"
set fileToResgen[1]="%sourceDir%Resources\Strings.de-AT"
set fileToResgen[2]="%sourceDir%Resources\Strings.de-CH"
set fileToResgen[3]="%sourceDir%Resources\Strings.de-DE"
set fileToResgen[4]="%sourceDir%Resources\Strings.fr-FR"
set fileToResgen[5]="%sourceDir%Resources\Strings.nb-NO"
set fileToResgen[6]="%sourceDir%Resources\Strings.nl-NL"
set fileToResgen[7]="%sourceDir%Resources\Strings.sv-SE"
set fileToResgen[8]="%sourceDir%Resources\Strings.es-CL"
set fileToResgen[9]="%sourceDir%Resources\Strings.pl-PL"
set fileToResgen[10]="%sourceDir%Resources\Strings.cs-CZ"
set fileToResgen[11]="%sourceDir%Resources\Strings.el-GR"
set fileToResgen[12]="%sourceDir%Resources\Strings.da-DK"
set fileToResgen[13]="%sourceDir%Resources\Strings.zh-TW"


%resgenPath% /compile !fileToResgen[0]!.txt !fileToResgen[1]!.txt !fileToResgen[2]!.txt !fileToResgen[3]!.txt !fileToResgen[4]!.txt !fileToResgen[5]!.txt !fileToResgen[6]!.txt !fileToResgen[7]!.txt !fileToResgen[8]!.txt !fileToResgen[9]!.txt !fileToResgen[10]!.txt !fileToResgen[11]!.txt !fileToResgen[12]!.txt !fileToResgen[13]!.txt

rem for /l %%n in (0,1,6) do (
rem     copy /Y !fileToResgen[%%n]!.resources "bin\Debug\Resources"
rem )

if not exist "%targetDir%Resources\nul" mkdir "%targetDir%Resources"

for /f "tokens=2 delims==" %%s in ('set fileToResgen[') do copy /Y %%s.resources "%targetDir%Resources"
