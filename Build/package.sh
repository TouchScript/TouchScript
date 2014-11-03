if [[ $# -lt 2 ]] ; then
    printf "\e[31mUsage: package.sh <project folder> <package path> <examples package path>\e[39m\n"
    exit 0
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
exportFolders="Assets/TouchScript/Devices Assets/TouchScript/Editor Assets/TouchScript/Plugins Assets/TouchScript/Prefabs Assets/TouchScript/Textures"
include="${1}/include.txt"
if [ -f "$include" ]; then
    value=`cat "$include"`
    exportFolders="$exportFolders $value"
fi

"${DIR}"/package_project.sh "$1" "$2" "\"$exportFolders\""
if [[ $# -eq 3 ]] ; then
    "${DIR}"/package_project.sh "$1" "$3" "\"$exportFolders Assets/TouchScript/Examples\""
fi