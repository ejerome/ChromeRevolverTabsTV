using Microsoft.Win32;
using System;

// Small C# script which parse the current directory (except the roslyn one which is used to execute this script)
// to create a .bat file which allow to open Chrome with as many tabs there are items in the parsed folder
// it filter items by extension (skiping binaries and scripts), but allowing media files
// this bat is hidden because only used to check the commands and arguments, as
// this programm will auto start this generated bat file 

// Then, in addition to this file, you could create a windows shortcut which
// will call this script with the roslyn csi.exe using working directory 
// where you have all your files you want to show in tabs (.url, .jpg, .webp...).
 
Console.WriteLine($"CurrentDir : {Environment.CurrentDirectory}");

string roslynFolderNameToExclude = "roslynScriptFromMSBUILD14.0";

var batFile = Environment.CurrentDirectory + "\\ChromeTV.bat";
using (FileStream fs = new FileStream(batFile, FileMode.OpenOrCreate)) 
{
	using (StreamWriter writer = new StreamWriter(fs))
	{	
		string[] fileEntries = 
			System.IO.Directory.GetFiles(Environment.CurrentDirectory, "*.*", System.IO.SearchOption.AllDirectories)
			.Where(d => !d.StartsWith(roslynFolderNameToExclude)).ToArray();
		
		foreach (var file in fileEntries)
		{
			string absFile = UNCPath(file);
			string entry = string.Empty;
			
			// filter by extension
			string ext = Path.GetExtension(absFile);
			if (ext == ".dll" || ext == ".csx" || ext == ".exe" || ext == ".lnk" || ext == ".bat")
			{
				continue; // skip bat, exe and exe shortcut link (exclude all binaries and scripts)
			}
			if (ext == ".jpg" || ext == ".png")
			{		
				var uri = new Uri(absFile);
				entry = Uri.UnescapeDataString(uri.AbsoluteUri);
				
				writer.WriteLine($"start chrome \"{entry}\" &");
			}
			if(ext == ".url")
			{
				entry = absFile;
				writer.WriteLine($"\"{entry}\" &");
			}
			
			if(!string.IsNullOrWhiteSpace(entry))
				Console.WriteLine($"Ready to open Chrome with : {entry}");
		}
		writer.WriteLine();
		
		// Flush the writer in order to get a correct stream position for truncating
		writer.Flush();
		
		// Set the stream length to the current position in order to truncate leftover text
		fs.SetLength(fs.Position);
		
		System.IO.File.SetAttributes(batFile, System.IO.File.GetAttributes(batFile) | System.IO.FileAttributes.Hidden);
		
		Console.WriteLine($"RestartChrome with : {batFile}");
		RestartChrome(batFile);
	}
}

// utility method
public static string UNCPath(string absPath)
{
    if (!absPath.StartsWith(@"\\"))
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Network\\" + absPath[0]))
        {
            if (key != null)
            {
                return key.GetValue("RemotePath").ToString() + absPath.Remove(0, 2).ToString();
            }
        }
    }
    return absPath;
}

public static void RestartChrome(string batFile)
{
	Process [] chromeInstances = Process.GetProcessesByName("chrome");

	foreach(Process p in chromeInstances)
		p.Kill();
	
	var processInfo = new ProcessStartInfo("cmd.exe", "/c " + batFile);
	Process.Start(processInfo);
}

// jesnault