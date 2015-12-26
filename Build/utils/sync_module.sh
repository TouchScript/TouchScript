#!/bin/bash

# 1. Backs up tracked files by git
# 2. Copies everything from Source folder
# 3. Restores backed files

if [[ $# -eq 0 ]] ; then
    printf "\e[31mUsage: sync_module.sh <path to module folder>\e[39m\n"
    exit 0
fi

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

printf "\e[1;33mSynchronizing module ${1##*/}.\e[0;39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd "$1"
TMP="./___tmp"
rm -rf $TMP
mkdir $TMP
ROOTFOLDER="Assets/TouchScript"
IFS=$'\n'
for f in $(git ls-tree -r --name-only HEAD | grep "^$ROOTFOLDER/") ; do
	echo "$f"
	FOLDER=$(dirname "$f")/
	FILENAME=$(basename "$f")
	TMPFOLDER="$TMP/${FOLDER##$ROOTFOLDER/}"
	mkdir -p "$TMPFOLDER"
	if (! cp "./$f" "$TMPFOLDER$FILENAME") ; then
		printf "\e[31mError copying $f to $TMPFOLDER$FILENAME!\e[39m\n"
		exit 0;
	fi 
done

rm -rf ./$ROOTFOLDER
if (! cp -rf "$DIR/../../Source/Assets/TouchScript" "./Assets/") ; then
	printf "\e[31mError copying TouchScript to ${1}!\e[39m\n"
	exit 0;
fi 
if (! cp -r "$TMP/." "./$ROOTFOLDER/") ; then
	printf "\e[31mError copying temporary files to ${1}!\e[39m\n"
	exit 0;
fi
rm -rf $TMP