#!/bin/bash

# Builds a solution on OSX and Windows.
# Assumes that Unity is located either in /Applications/Unity/ or c:\Program Files\Unity\.

if [[ $# -eq 0 ]] ; then
    printf "\e[31mUsage: build_solution.sh <path to solution>\e[39m\n"
    exit 0
fi

printf "\e[1;33mBuilding ${1}.\e[0;39m\n"

# Mac
if [ "$(uname)" == "Darwin" ]; then
	UNITYPATH="/Applications/Unity/Unity.app"
    if [ ! -d "$UNITYPATH" ]; then
        printf "\e[31mCouldn't find Unity at $UNITYPATH!\e[39m\n"
        exit 0;
    fi
	"$UNITYPATH/Contents/Frameworks/MonoBleedingEdge/bin/xbuild" /p:Configuration=Release /clp:ErrorsOnly "${1}"

# Windows
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ] || [ "$(expr substr $(uname -s) 1 10)" == "MINGW64_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ${1} -p:Configuration=Release -clp:ErrorsOnly
fi