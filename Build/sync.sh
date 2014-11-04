DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
defaultProjectsPath=$(cd "$DIR/../UnityPackages/" && pwd)

for i in $(ls -d "$defaultProjectsPath/"*/); do 
	path="${i%%/}"
	name="${path##*/}"

	if [[ "$name" != "TouchScript" ]]; then 
		"$DIR/sync_folder.sh" "$defaultProjectsPath/TouchScript" "$path" 
	fi
done