printf "\n\e[32mCompiling TUIOSharp.\e[39m\n"

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SLN="$DIR/../TUIOsharp/TUIOsharp.sln"

# Mac
if [ "$(uname)" == "Darwin" ]; then
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Clean --c:Release $SLN 
	/Applications/Unity/MonoDevelop.app/Contents/MacOS/mdtool build --t:Build --c:Release $SLN

# Windows
elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    /c/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe $SLN -p:Configuration=Release -clp:ErrorsOnly
fi

cp "$DIR/../TUIOsharp/TUIOsharp/bin/Release/TUIOsharp.dll" "$DIR/../../../Source/Assets/TouchScript/Modules/TUIO/Libraries/"
cp "$DIR/../TUIOsharp/TUIOsharp/bin/Release/OSCsharp.dll" "$DIR/../../../Source/Assets/TouchScript/Modules/TUIO/Libraries/"