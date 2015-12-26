#!/bin/bash

# Calls build scripts for all sub-folders in External folder

printf "\n\e[1;36mBUILDING EXTERNAL PROJECTS.\e[0;39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
EXTERNAL=$(cd "$DIR/../External/" && pwd)

for i in $(ls -d "$EXTERNAL/"*/); do 
	FILE="${i%%/}/Build/build.sh"
	if [ -f $FILE ]; then
	   	$FILE
	fi
done