printf "\e[32mSynchronizing TouchScript folder...\e[39m\n"
for i in $(ls -d */); do 
	name=${i%%/}
	if [[ "$name" != "TouchScript" && "$name" != _* ]]; then 
		./sync_examples.sh "$name" 
	fi
done

printf "\n\e[32mCompiling projects...\e[39m\n"
if [ "$(uname)" == "Darwin" ]; then
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Clean --c:Release ../TouchScript.sln 
    /Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Build --c:Release ../TouchScript.sln  
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ../TouchScript.sln -p:Configuration=Release -clp:ErrorsOnly
fi
