# 1. Backs up tracked files by git
# 2. Copies everything from Source folder
# 3. Restores backed files

if [ ! -d "${1}" ]; then
    printf "\e[31mError! Folder '${1}' does not exist.\e[39m\n"
    exit 0
fi

printf "\e[1;33mSynchronizing module ${1##*/}.\e[0;39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd "$1"
tmp="./___tmp"
rm -rf $tmp
mkdir $tmp
rootfolder="Assets/TouchScript"
IFS=$'\n'
for f in $(git ls-tree -r --name-only HEAD | grep "^$rootfolder/") ; do
#	echo "$f"
	folder=$(dirname "$f")/
	filename=$(basename "$f")
	tmpfolder="$tmp/${folder##$rootfolder/}"
	mkdir -p "$tmpfolder"
	if (! cp "./$f" "$tmpfolder$filename") ; then
		printf "\e[31mError copying $f to $tmpfolder$filename!\e[39m\n"
		exit 0;
	fi 
done

rm -rf ./$rootfolder
if (! cp -rf "$DIR/../../Source/Assets/TouchScript" "./Assets/") ; then
	printf "\e[31mError copying TouchScript to ${1}!\e[39m\n"
	exit 0;
fi 
if (! cp -r "$tmp/." "./$rootfolder/") ; then
	printf "\e[31mError copying temporary files to ${1}!\e[39m\n"
	exit 0;
fi
rm -rf $tmp