#!bash

printf "Synchronizing TouchScript folder...\n"
for i in $(ls -d */); do 
	name=${i%%/}
	if [ "$name" != "TouchScript" ]; then 
		./sync_examples.sh "$name" 
	fi
done

printf "\nCompiling projects...\n"
/c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ../TouchScript.sln -p:Configuration=Release -clp:ErrorsOnly