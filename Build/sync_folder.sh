# 1. Backs up tracked files by git
# 2. Copies everything (i.e. examples) from TouchScript folder
# 3. Restores backed files

if [[ $# -lt 2 ]] ; then
    printf "\e[31mUsage: sync_folder.sh <folder to sync> <target folder>\e[39m\n"
    exit 0
fi

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

if [ ! -d "${2}" ]; then
    printf "\e[31mError! Folder '${2}' does not exist.\e[39m\n"
    exit 0
fi

printf "\n\e[32mSynchronizing $1 with $2.\e[39m\n"

cd "$2"
tmp="./___tmp"
rm -rf $tmp
mkdir $tmp
rootfolder="Assets/TouchScript"
IFS=$'\n'
for f in $(git ls-tree -r --name-only HEAD | grep "^$rootfolder/") ; do
	echo "$f"
	folder=$(dirname "$f")/
	filename=$(basename "$f")
	tmpfolder="$tmp/${folder##$rootfolder/}"
	mkdir -p "$tmpfolder"
	if (! cp "./$f" "$tmpfolder$filename") ; then
		printf "\e[31mERROR BUILDING!\e[39m\n"
		exit 0;
	fi 
done

rm -rf ./$rootfolder
if (! cp -rf "$1/Assets/TouchScript" "./Assets/") ; then
	printf "\e[31mERROR BUILDING!\e[39m\n"
	exit 0;
fi 
if (! cp -r "$tmp/." "./$rootfolder/") ; then
	printf "\e[31mERROR BUILDING!\e[39m\n"
	exit 0;
fi
rm -rf $tmp