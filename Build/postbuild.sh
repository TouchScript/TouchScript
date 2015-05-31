for file in "${@:3}" ; do 
	cp "$file" "$1/UnityPackages/TouchScript/Assets/TouchScript/$2/"
	cp "$file" "$1/UnityPackages/TouchScript.Android/Assets/TouchScript/$2/"
	cp "$file" "$1/UnityPackages/TouchScript.iOS/Assets/TouchScript/$2/"
	cp "$file" "$1/UnityPackages/TouchScript.TUIO/Assets/TouchScript/$2/"
	cp "$file" "$1/UnityPackages/TouchScript.WindowsPhone/Assets/TouchScript/$2/"
	cp "$file" "$1/UnityPackages/TouchScript.WindowsStore/Assets/TouchScript/$2/"

	if [ -f "custom_postbuild.sh" ]; then
	    ./custom_postbuild.sh $1 $2 $file
	fi
done