printf "\n\e[32mCompiling solution.\e[39m\n"

# Mac
if [ "$(uname)" == "Darwin" ]; then
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Clean --c:Release TouchScript.sln 
    /Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Build --c:Release TouchScript.sln  

# Windows
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe TouchScript.sln -p:Configuration=Release -clp:ErrorsOnly
fi