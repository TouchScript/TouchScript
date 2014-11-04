printf "\n\e[32mCompiling solution.\e[39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Mac
if [ "$(uname)" == "Darwin" ]; then
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Clean --c:Release "$DIR/../TouchScript.sln" 
    /Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Build --c:Release "$DIR/../TouchScript.sln"  

# Windows
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe "$DIR/../TouchScript.sln" -p:Configuration=Release -clp:ErrorsOnly
fi