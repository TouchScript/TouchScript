printf "\e[32mBuilding packages...\e[39m\n"

exportFolders="Assets/TouchScript/Devices Assets/TouchScript/Editor Assets/TouchScript/Plugins Assets/TouchScript/Prefabs Assets/TouchScript/Textures"

rm -rf _AssetStore/Assets/TouchScript
cp -r TouchScript/Assets/TouchScript _AssetStore/Assets
rm -rf _AssetStore/Assets/TouchScript/Examples*
mkdir -p _AssetStore/Assets/TouchScript/Examples
mkdir -p _AssetStore/Assets/TouchScript/Packages

if [ "$(uname)" == "Darwin" ]; then
    unityPath="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    folder=$(pwd)
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    unityPath="/c/Program Files (x86)/Unity/Editor/Unity.exe"
    # Windows path
    folder=$(pwd | sed -e 's/^\/\(\w\)/\1:/gi' -e 's/\//\\/gi')
fi

for i in $(ls -d */); do 
	name=${i%%/}
	if [[ $name != _* ]]; then
		printf "\e[32mBuilding $name.\e[39m\n" 
		if [ $name == Scaleform ] ; then
			toExport="$exportFolders Assets/StreamingAssets Assets/TouchScript/Modules"
		elif [ $name == PlayMaker ] ; then
			toExport="$exportFolders Assets/PlayMakerUnity2D Assets/TouchScript/Modules"
		else
			toExport=$exportFolders
		fi

		package="$folder/_AssetStore/Assets/TouchScript/Packages/$name.unitypackage"
		"$unityPath" -batchmode -projectPath "$folder/$name" -exportPackage $toExport $package -quit
		"$unityPath" -batchmode -projectPath "$folder/$name" -exportPackage $toExport Assets/TouchScript/Examples "$folder/_AssetStore/Assets/TouchScript/Examples/$name.Examples.unitypackage" -quit
		if [ ! -f $package ]; then
			printf "\e[31mFailed to build package!\e[39m\n"
		fi
	fi
done

printf "\e[32mBuilding $folder/TouchScript.unitypackage.\e[39m\n" 
"$unityPath" -batchmode -projectPath "$folder/_AssetStore" -exportPackage Assets/TouchScript "$folder/TouchScript.unitypackage" -quit