using Microsoft.Win32;
using System;

// Small C# script which parse the current directory (except the roslyn one which is used to execute this script)
// to create a .bat file which allow to open Chrome with as many tabs there are items in the parsed folder
// it filter items by extension (skiping binaries and scripts), but allowing media files
// this bat is hidden because only used to check the commands and arguments
// This programm will auto start this generated bat file 
 
Console.WriteLine($"CurrentDir : {Environment.CurrentDirectory}");

string roslynFolderNameToExclude = "roslynScriptFromMSBUILD14.0";
string archivesFolderNameToExclude = "Archives";

var batFile = Environment.CurrentDirectory + "\\ChromeTV.bat";
using (FileStream fs = new FileStream(batFile, FileMode.OpenOrCreate)) 
{
	using (StreamWriter writer = new StreamWriter(fs))
	{	
		string[] fileEntries = 
			System.IO.Directory.GetFiles(Environment.CurrentDirectory, "*.*", System.IO.SearchOption.AllDirectories)
			.Where(d => !d.Contains(roslynFolderNameToExclude))
			.Where(d => !d.Contains(archivesFolderNameToExclude))
			.ToArray();
		
		writer.Write($"start chrome");
		
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
			if (ext == ".webm" || ext == ".WEBM" || ext == ".JPG" || ext == ".jpg" || ext == ".png" || ext == ".PNG")
			{	
				var uri = new Uri(absFile);
				entry = Uri.UnescapeDataString(uri.AbsoluteUri).ToString();				
				writer.Write($" \"{entry}\"");
			}
			if(ext == ".url")
			{
				entry = File.ReadLines(absFile).Where(l => l.StartsWith("URL=")).First();
				entry = entry.Split('=').AsEnumerable().ElementAt(1);

				writer.Write($" \"{entry}\"");
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
		
		Start(batFile);
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

public static void Start(string batFile)
{
	Console.WriteLine($"Start Chrome with : {batFile}");
	var processInfo = new ProcessStartInfo("cmd.exe", $"/c \"{batFile}\"");
	Process.Start(processInfo);
}
