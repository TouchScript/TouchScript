# Builds a solution on OSX and Windows.
# build_solution.sh <path to solution>

printf "\e[1;33mBuilding ${1}.\e[0;39m\n"

WIN="$(expr substr $(uname -s) 1 10)"

# Mac
if [ "$(uname)" == "Darwin" ]; then
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Clean --c:Release ${1} 
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Build --c:Release ${1}

# Windows
elif [ "$WIN" == "MINGW32_NT" ] || [ "$WIN" == "MINGW64_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ${1} -p:Configuration=Release -clp:ErrorsOnly
fi