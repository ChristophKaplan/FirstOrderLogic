using System.Diagnostics;

namespace PropositionalLogic.Helpers;

public class InputOutput {
    private const string path = "../../../SaveFiles/";

    public static void WriteFile(string fileName, string text) {
        File.WriteAllText(path + fileName, text);
    }

    private static string ReadFile(string fileName) {
        return File.ReadAllText(path + fileName);
    }

    public static void OpenFile(string fileName) {
        var p = new ProcessStartInfo($"{path}{fileName}", "--new") { UseShellExecute = true };
        Process.Start(p);
    }
}