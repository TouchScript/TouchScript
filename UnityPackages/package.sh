#!bash

printf "\e[32mBuilding packages...\e[39m\n"

# Windows path
folder=$(pwd | sed -e 's/\/d/d:/gi' -e 's/\//\\/gi')
exportFolders="Assets/TouchScript/Devices Assets/TouchScript/Editor Assets/TouchScript/Plugins Assets/TouchScript/Prefabs Assets/TouchScript/Textures"

rm -rf _AssetStore/Assets/TouchScript
cp -r TouchScript/Assets/TouchScript _AssetStore/Assets
rm -rf _AssetStore/Assets/TouchScript/Examples*
mkdir -p _AssetStore/Assets/TouchScript/Examples
mkdir -p _AssetStore/Assets/TouchScript/Packages

for i in $(ls -d */); do 
	name=${i%%/}
	if [[ $name != _* ]]; then
		printf "\e[32mBuilding $name.\e[39m\n" 
		if [ $name == TouchScript.WindowsPhone ] ; then
			toExport="$exportFolders Assets/Plugins"
		elif [ $name == Scaleform ] ; then
			toExport="$exportFolders Assets/StreamingAssets"
		else
			toExport=$exportFolders
		fi
		"/c/Program Files (x86)/Unity/Editor/Unity.exe" -batchmode -projectPath "$folder\\$name" -exportPackage $toExport "$folder\\_AssetStore\\Assets\\TouchScript\\Packages\\$name.unitypackage" -quit
		"/c/Program Files (x86)/Unity/Editor/Unity.exe" -batchmode -projectPath "$folder\\$name" -exportPackage Assets/TouchScript/Examples "$folder\\_AssetStore\\Assets\\TouchScript\\Examples\\$name.Examples.unitypackage" -quit
	fi
done

"/c/Program Files (x86)/Unity/Editor/Unity.exe" -batchmode -projectPath "$folder\\_AssetStore" -exportPackage Assets/TouchScript "$folder\\TouchScript.unitypackage" -quit