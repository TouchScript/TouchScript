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
pwd
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
	cp "./$f" "$tmpfolder$filename"
done

rm -rf ./$rootfolder
cp -rf "$1/Assets/TouchScript" "./Assets/"
cp -r "$tmp/." "./$rootfolder/"
rm -rf $tmp