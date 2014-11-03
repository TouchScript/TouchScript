DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

for i in $(ls -d "$DIR/../UnityPackages/"*/); do 
	path="${i%%/}"
	name="${path##*/}"

	if [[ "$name" != "TouchScript" ]]; then 
		"$DIR/sync_folder.sh" "$DIR/../UnityPackages/TouchScript" "$path" 
	fi
done