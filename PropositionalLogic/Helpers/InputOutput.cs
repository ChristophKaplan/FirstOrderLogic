using System.Diagnostics;

namespace PropositionalLogic.Helpers;

public static class InputOutput {
    public const string ExportFolderPath = "../../../ExportFiles/";

    public static void WriteFile(string fileName, string text, string folderPath) {
        File.WriteAllText($"{ExportFolderPath}{folderPath}{fileName}", text);
    }

    private static string ReadFile(string fileName, string folderPath) {
        return File.ReadAllText($"{ExportFolderPath}{folderPath}{fileName}");
    }

    public static void OpenFile(string fileName, string folderPath) {
        RunCommand("open", fileName, ExportFolderPath + folderPath);
    }
    
    public static void RunCommand(string command, string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
        };

        using Process process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }
}