Launch as many compatible files found in the current directory as Chrome tabs.

Then you only need a Chrome extension to loop over these tabs with a timer (RevolverTabs is a good one).

* Put The script and the roslyn folder in the directory where you have media files (.jpg, .webp) or webbrowser shortcuts (.url)
* Open a command prompt in that folder
* call 'roslynScriptFromMSBUILD14.0\csi.exe "..\ChromeRevolverTabsTV.csx"'
* it will start a new fresh Chrome webBrowser window with all your tabs

To create a "one click" launcher, you could create a windows shortcut which will call this script with the roslyn csi.exe using working directory where you have all your files you want to show in tabs (.url, .jpg, .webp...)
