if [[ $# -eq 0 ]] ; then
    echo '\e[31mError! Please supply project root folder.\e[39m'
    exit 0
fi

printf "\n\e[32mSynchronizing TouchScript folder for $1.\e[39m\n"
rm -rf tmp
mkdir tmp
rootfolder="$1/Assets/TouchScript"
IFS=$'\n'
for f in $(git ls-tree -r --name-only HEAD | grep "^$rootfolder/") ; do
	echo "$f"
	folder=$(dirname "$f")/
	filename=$(basename "$f")
	tmpfolder="tmp/${folder##$rootfolder/}"
	mkdir -p "$tmpfolder"
	cp "$f" "$tmpfolder$filename"
done

rm -rf $rootfolder
cp -rf "TouchScript/Assets/TouchScript" "$1/Assets/"
cp -r "tmp/." "$rootfolder/"
rm -rf tmp