if [[ $# -lt 3 ]] ; then
    printf "\e[31mUsage: package_project.sh <project folder> <package name> <folders>\e[39m\n"
    exit 0
fi

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

printf "\n\e[32mBuilding package for '$1' to '$2'.\e[39m\n"

log="__log.txt"
# ".*" -> .*
temp="${3%\"}"
temp="${temp#\"}"
exportFolders="$temp"

if [ -f "$2" ]; then
    rm $2
fi

cd $1

if [ "$(uname)" == "Darwin" ]; then
    unityPath="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    projectFolder=$(pwd)
    separator="/"
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    unityPath="/c/Program Files (x86)/Unity/Editor/Unity.exe"
    # Windows path
    projectFolder=$(pwd | sed -e 's/^\/\([a-z]\)/\1:/g' -e 's/\//\\/g')
    separator="\\"
fi

log="${projectFolder}${separator}${log}"
"$unityPath" -batchmode -logFile $log -projectPath "$projectFolder" -exportPackage $exportFolders $2 -quit
if [ ! -f $2 ]; then
	cat "$log"
	printf "\e[31mFailed to build $2!\e[39m\n"
    rm "$log"
    exit 0
fi

rm "$log"