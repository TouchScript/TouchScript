#!/bin/bash

printf "\n\e[1;36mBuilding External/WindowsTouch.\e[0;39m\n"

UNAME=$(uname -s)
if [ "${UNAME:0:10}" != "MINGW32_NT" ] && [ "${UNAME:0:10}" != "MINGW64_NT" ]; then
	printf "\e[31mNeed to build WindowsTouch.dll on Windows!\e[39m\n"
	exit 0
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT=$(cd "$DIR/../" && pwd)
SLN="$PROJECT/WindowsTouch.sln"

"$DIR/../../../Build/utils/build_solution.sh" $SLN x86
"$DIR/../../../Build/utils/build_solution.sh" $SLN x64