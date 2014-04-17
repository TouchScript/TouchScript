printf "\e[32mSynchronizing TouchScript folder...\e[39m\n"
for i in $(ls -d */); do 
	name=${i%%/}
	if [[ "$name" != "TouchScript" && "$name" != _* ]]; then 
		./sync_examples.sh "$name" 
	fi
done

printf "\n\e[32mCompiling projects...\e[39m\n"
/c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ../TouchScript.sln -p:Configuration=Release -clp:ErrorsOnly

printf "\n\e[32mCompiling Windows Phone projects...\e[39m\n"
/c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ../TouchScript.Other/TouchScript.WindowsPhone/TouchScript.WindowsPhone.sln -p:Configuration=Release -clp:ErrorsOnly