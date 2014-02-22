#!bash

/c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ../TouchScript.sln -p:Configuration=Release
read -p "Press any key to continue... " -n 1 -s

for i in $(ls -d */); do 
	name=${i%%/}
	if [ "$name" != "TouchScript" ]; then 
		./sync_examples.sh "$name" 
	fi
done