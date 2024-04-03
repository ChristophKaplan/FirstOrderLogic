using System.Text;

namespace PropositionalLogic.Helpers;

public class MarkUpGenerator {
    public const string latexPath = "Latex/AIG_thesis/";
    public const string htmlPath = "HTML/";

    public static void ExportLaTex(string fileName) {
        string pdfFileName = fileName.Substring(0, fileName.Length - 3) + "pdf";
        InputOutput.RunCommand("rm", pdfFileName, InputOutput.ExportFolderPath + latexPath);
        InputOutput.RunCommand("texify", $"--clean --pdf {fileName}", InputOutput.ExportFolderPath + latexPath);
        InputOutput.OpenFile(pdfFileName, latexPath);
    }

    public static void ExportHTML(string fileName, string content) {
        InputOutput.WriteFile("forgetCompare.html", content, htmlPath);
        InputOutput.OpenFile("forgetCompare.html", htmlPath);
    }

    public static string ToHTMLTable((string[] col, string[][] rows) table) {
        var htmlTable = new StringBuilder();
        htmlTable.Append("<table style=\"border: 1px solid black;\">");
        htmlTable.Append("<tr style=\"border: 1px solid black;\">");
        foreach (var col in table.col) htmlTable.Append($"<th style=\"padding: 5px;\">{col}</th>");
        htmlTable.Append("</tr>");
        foreach (var row in table.rows) {
            htmlTable.Append("<tr style=\"border: 1px solid black;\">");
            foreach (var cell in row) htmlTable.Append($"<td style=\"padding: 5px; text-align: center;\">{cell}</td>");
            htmlTable.Append("</tr>");
        }

        htmlTable.Append("</table>");
        return htmlTable.ToString();
    }

    public static string ToLaTexTable((string[] col, string[][] rows) table, bool displayMath = true) {
        var latexTable = new StringBuilder();

        var x = "|";
        for (var i = 0; i < table.col.Length; i++) {
            x += " c |";
        }

        if (displayMath) latexTable.Append("\\begin{displaymath}\n");
        latexTable.Append($"\\begin{{tabular}}{{{x}}}\n");

        for (var i = 0; i < table.col.Length - 1; i++) {
            latexTable.Append($"{table.col[i]} & ");
        }

        latexTable.Append($"{table.col[^1]} \\\\\n");

        latexTable.Append("\\hline\n");

        foreach (var row in table.rows) {
            for (var i = 0; i < row.Length - 1; i++) {
                latexTable.Append($"{row[i]} & ");
            }

            latexTable.Append($"{row[^1]} \\\\\n");
        }

        latexTable.Append("\\end{tabular}\n");
        if (displayMath) latexTable.Append("\\end{displaymath}\n");

        
        
        return ReplaceUnicodeToLaTex(latexTable.ToString());
    }

    public static string ReplaceUnicodeToLaTex(string input) {
        return input.Replace("∧", @"$\land$")
            .Replace("∨", @"$\lor$")
            .Replace("¬", @"$\lnot$")
            .Replace("→", @"$\rightarrow$")
            .Replace("↔", @"$\leftrightarrow$")
            .Replace("⊤", @"$\top$")
            .Replace("⊥", @"$\bot$")
            .Replace("⊨", @"$\models$");
    }

    public static string FigureCompare(string a, string b) {
        return @"\begin{figure}
                \centering
                \begin{minipage}{0.45\textwidth}
                    \centering
                    \resizebox{\textwidth}{!}{" +
               a +
               @"}
                \caption{first figure}
                \end{minipage}\hfill
                \begin{minipage}{0.45\textwidth}
                    \centering
                    \resizebox{\textwidth}{!}{" +
               b +
               @"}
                \caption{second figure}
                \end{minipage}
            \end{figure}";
    }
    
    public static string SentenceToLaTex(Sentence sentence) {
        return ReplaceUnicodeToLaTex(sentence.ToString());
    }
    
    public static string SentenceToForest(Sentence sentence) {
        void DFS(Sentence sentence, ref string result) {
            result += "[";
            if (sentence is AtomicSentence atomic) {
                result += $"{SentenceToLaTex(atomic)}]";
                return;
            } 
            
            result += ((ComplexSentence)sentence).OperatorToString();
            
            foreach (var child in sentence.Children) {
                DFS(child, ref result);
            }
            
            result += "]";
        }

        var result = "";
        DFS(sentence, ref result);
        return @"\begin{forest}" + ReplaceUnicodeToLaTex(result) + @"\end{forest}";
    }
}