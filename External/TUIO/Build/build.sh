printf "\n\e[1;36mBuilding External/TUIOsharp.\e[0;39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SLN="$DIR/../TUIOsharp/TUIOsharp.sln"

"$DIR/../../../Build/utils/build_solution.sh" $SLN
cp "$DIR/../TUIOsharp/TUIOsharp/bin/Release/TUIOsharp.dll" "$DIR/../../../Source/Assets/TouchScript/Modules/TUIO/Libraries/"
cp "$DIR/../TUIOsharp/TUIOsharp/bin/Release/OSCsharp.dll" "$DIR/../../../Source/Assets/TouchScript/Modules/TUIO/Libraries/"