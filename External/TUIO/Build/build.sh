#!/bin/bash

printf "\n\e[1;36mBuilding External/TUIOsharp.\e[0;39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT=$(cd "$DIR/../TUIOsharp" && pwd)
LIBS=$(cd "$DIR/../../../Source/Assets/TouchScript/Modules/TUIO/Libraries/" && pwd)
SLN="$PROJECT/TUIOsharp.sln"

"$DIR/../../../Build/utils/build_solution.sh" $SLN "Any CPU"
cp "$PROJECT/TUIOsharp/bin/Release/TUIOsharp.dll" "$LIBS"
cp "$PROJECT/TUIOsharp/bin/Release/OSCsharp.dll" "$LIBS"