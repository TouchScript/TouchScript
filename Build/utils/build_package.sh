#!/bin/bash

# Assumes that Unity is located either in /Applications/Unity/ or c:\Program Files\Unity\.

if [[ $# -lt 3 ]] ; then
    printf "\e[31mUsage: build_package.sh <project folder> <package path> <folders>\e[39m\n"
    exit 0
fi

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

printf "\n\e[32mBuilding a package from project '$1' to '$2'.\e[39m\n"

LOG="__log.txt"
# ".*" -> .*
TEMP="${3%\"}"
TEMP="${TEMP#\"}"
EXPORTFOLDERS="$TEMP"

if [ -f "$2" ]; then
    rm $2
fi

cd $1

# Mac
if [ "$(uname)" == "Darwin" ]; then
    UNITYPATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    if [ ! -f "$UNITYPATH" ]; then
        printf "\e[31mCouldn't find Unity at $UNITYPATH!\e[39m\n"
        exit 0;
    fi
    PROJECTFOLDER=$(pwd)
    SEPARATOR="/"

# Windows
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ] || [ "$(expr substr $(uname -s) 1 10)" == "MINGW64_NT" ]; then
    UNITYPATH="/c/Program Files/Unity/Editor/Unity.exe"
    if [ ! -f "$UNITYPATH" ]; then
        printf "\e[31mCouldn't find Unity at $UNITYPATH!\e[39m\n"
        exit 0;
    fi
    # Windows path
    PROJECTFOLDER=$(pwd | sed -e 's/^\/\([a-z]\)/\1:/g' -e 's/\//\\/g')
    SEPARATOR="\\"
fi

LOG="${PROJECTFOLDER}${SEPARATOR}${LOG}"
"$UNITYPATH" -batchmode -logFile $LOG -projectPath "$PROJECTFOLDER" -exportPackage $EXPORTFOLDERS $2 -quit
if [ ! -f $2 ]; then
	cat "$LOG"
	printf "\e[31mFailed to build $2!\e[39m\n"
    rm "$LOG"
    exit 0
fi

rm "$LOG"