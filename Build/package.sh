#!/bin/bash

# 1. Builds external dlls
# 2. Synchronizes modules
# 3. Builds module packages
# 4. Builds TouchScript package

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ASSETSTORE=$(cd "$DIR/../AssetStore/" && pwd)
SOURCE=$(cd "$DIR/../Source/" && pwd)
MODULES=$(cd "$DIR/../Modules/" && pwd)
UNITYPACKAGE=$(cd "$DIR/../" && pwd)/TouchScript.unitypackage

$DIR/build_external.sh
$DIR/sync_modules.sh

rm -rf "$ASSETSTORE/Assets/TouchScript"
rm -f "$ASSETSTORE/Assets/TouchScript.meta"
cp -r "$SOURCE/Assets/TouchScript" "$ASSETSTORE/Assets/"
cp "$SOURCE/Assets/TouchScript.meta" "$ASSETSTORE/Assets/"

if [ -d "$MODULES" ]; then
	for i in $(ls -d "$MODULES/"*/); do 
		FILE="${i%%/}/Build/package.sh"
		if [ -f $FILE ]; then
		   	"$FILE" "$ASSETSTORE/Assets/TouchScript/Modules"
		fi
	done
fi

printf "\n\e[1;36mPackaging TouchScript.unitypackage.\e[0;39m\n"

$DIR/utils/build_package.sh "$ASSETSTORE" "$UNITYPACKAGE" "Assets/TouchScript"