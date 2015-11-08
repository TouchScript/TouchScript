xcopy /y "%3" "%1UnityPackages\TouchScript\Assets\TouchScript\%2\"
xcopy /y "%3" "%1UnityPackages\TouchScript.Android\Assets\TouchScript\%2\"
xcopy /y "%3" "%1UnityPackages\TouchScript.iOS\Assets\TouchScript\%2\"
xcopy /y "%3" "%1UnityPackages\TouchScript.TUIO\Assets\TouchScript\%2\"
xcopy /y "%3" "%1UnityPackages\TouchScript.WIndowsPhone\Assets\TouchScript\%2\"
xcopy /y "%3" "%1UnityPackages\TouchScript.WindowsStore\Assets\TouchScript\%2\"

if exist custom_postbuild.bat custom_postbuild.bat "%1" "%2" "%3"