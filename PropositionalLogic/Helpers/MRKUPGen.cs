using System.Numerics;
using System.Text;
using System.Globalization;

namespace PropositionalLogic.Helpers;

public class MRKUPGen {
    public const string latexPath = "Latex/AIG_thesis/";
    public const string htmlPath = "HTML/";
    static PropositionalLogic _logicTEMP = new();

    public static void ExportLaTex(string fileName) {
        Console.WriteLine("generate pdf...");
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

    public class TkizMarkerManager {
        private struct Marker {
            public string name;
            public string shape;
            public string color;
            public float opacity;
            public Vector2 From;
            public Vector2 To;
        }

        List<Marker> markers = new();

        public void AddMarker(Vector2 from, Vector2 to, string shape = "line", string color = "black", float opacity = 1.0f) {
            markers.Add(new Marker() {
                name = Random.Shared.Next().ToString(),
                shape = shape,
                color = color,
                opacity = opacity,
                From = from,
                To = to
            });
        }

        public string GetMarker(int x, int y) {
            var search = new Vector2(x, y);
            string marker = string.Empty;
            foreach (var m in markers) {
                if (m.From == search) {
                    marker += $"\\tikzmark{{{m.name}Start}}";
                }

                if (m.To == search) {
                    marker += $"\\tikzmark{{{m.name}End}}";
                }
            }

            return marker;
        }

        public string GetMarkerDefs() {
            var result = string.Empty;
            foreach (var marker in markers) {
                switch (marker.shape) {
                    case "line":
                        result +=
                            $"\\tikz[remember picture] \\draw[overlay, line width=1pt, {marker.color}] ([xshift=.25em,yshift=1.0em]pic cs:{marker.name}Start) -- ([xshift=.25em,yshift=-0.5em]pic cs:{marker.name}End);\n";
                        break;
                    case "rectangle":
                        result +=
                            $"\\tikz[remember picture, overlay] \\fill[{marker.color}, opacity={marker.opacity.ToString(CultureInfo.InvariantCulture)}] ([xshift=-0.5em,yshift=1.0em]pic cs:{marker.name}Start) rectangle ([xshift=1.0em,yshift=-0.5em]pic cs:{marker.name}End);\n";
                        break;
                }
            }

            return result;
        }
    }


    public static string ToLaTexTable((string[] col, string[][] rows) table, TkizMarkerManager tkizMarker, string tableLineColor = "black") {
        var latexTable = new StringBuilder();

        var defC = "|";
        for (var i = 0; i < table.col.Length; i++) {
            defC += " c |";
        }

        latexTable.Append($"\\arrayrulecolor{{{tableLineColor}}}\n"); // Set the color of the lines to white
        latexTable.Append($"\\begin{{tabular}}{{{defC}}}\n");

        for (var i = 0; i < table.col.Length - 1; i++) {
            latexTable.Append($"{table.col[i]} & ");
        }

        latexTable.Append($"{table.col[^1]} \\\\\n");

        latexTable.Append("\\hline\n");

        for (var y = 0; y < table.rows.Length; y++) {
            var row = table.rows[y];
            for (var x = 0; x < row.Length - 1; x++) {
                latexTable.Append($"{tkizMarker.GetMarker(x, y)}{row[x]} & ");
            }

            latexTable.Append($"{tkizMarker.GetMarker(row.Length - 1, y)}{row[^1]}\\\\\n");
        }

        latexTable.Append("\\end{tabular}\\\n");
        latexTable.Append(tkizMarker.GetMarkerDefs());

        return ReplaceUnicodeToLaTex(latexTable.ToString(), true);
    }

    public static string ReplaceUnicodeToLaTex(string input, bool mathMode = false) {
        List<string> replaceSymbols = new() {
            "∧",
            "∨",
            "¬",
            "→",
            "↔",
            "⊤",
            "⊥",
            "⊨",
            "AND",
            "OR"
        };

        foreach (var symbol in replaceSymbols) {
            input = input.Replace(symbol, mathMode ? $"${GetSubstitute(symbol)}$" : GetSubstitute(symbol));
        }

        string GetSubstitute(string input) {
            return input switch {
                "∧" => @"\land",
                "∨" => @"\lor",
                "¬" => @"\lnot",
                "→" => @"\rightarrow",
                "↔" => @"\leftrightarrow",
                "⊤" => @"\top",
                "⊥" => @"\bot",
                "⊨" => @"\models",
                "AND" => @"\land",
                "OR" => @"\lor",
                _ => input
            };
        }

        return input;
    }

    public static string FigureCompare(string figureLeft, string figureRight, string captionLeft, string captionRight, string label) {
        return @"\begin{figure}[H]\captionsetup{font=small}\centering\begin{minipage}{0.49\textwidth}\centering\resizebox{\textwidth}{!}{" +
               figureLeft +
               "}" +
               $"\\caption{{{captionLeft}}}" +
               @"\end{minipage}\hfill\begin{minipage}{0.49\textwidth}\centering\resizebox{\textwidth}{!}{" +
               figureRight +
               "}" +
               $"\\caption{{{captionRight}}}\\label{{fig:{label}}}" +
               @"\end{minipage}\end{figure}";
    }

    public static string Figurize(string content, string caption, string label) {
        return @"\begin{figure}[H]\captionsetup{font=small}\centering\begin{minipage}{\textwidth}\centering
\resizebox{\textwidth}{!}{" +
               content +
               $"}}\\caption{{{caption}}}\\label{{fig:{label}}}" +
               @"\end{minipage}\end{figure}";
    }

    public static string SentenceToForest(Sentence sentence) {
        bool triggered = false;
        var i = 0;
        void DFS(Sentence sentence, ref string result) {
            result += "[";
            if (sentence is AtomicSentence atomic) {
                if (atomic.IsTruthValue) {
                    result += $"{ReplaceUnicodeToLaTex(atomic.ToString(), true)}, name=TruthV" + i++ +
                              ", circle, draw, dotted, minimum size=0.2cm, font=\\tiny, edge=dotted]";
                }
                else {
                    result += $"{ReplaceUnicodeToLaTex(atomic.ToString(), true)}" +
                              ", circle, draw, minimum size=0.2cm, font=\\tiny" +
                              "]";
                }
                return;
            }

            result += ((ComplexSentence)sentence).OperatorToString();

            var simplified = _logicTEMP.Simplify(sentence, out var steps);
            if (simplified is AtomicSentence atomicSentence && atomicSentence.IsTruthValue) {
                if (!triggered) result += ",name=BlockC"+i;
                result += ",for tree={circle, draw, dotted, minimum size=0.2cm, font=\\tiny, edge=dotted}";
                //result += ",tikz={\\node [draw,red,inner sep=0,fit to=tree]{};}";
                triggered = true;
            }
            else {
                result += ",circle, draw, minimum size=0.2cm, font=\\tiny";
            }

            foreach (var child in sentence.Children) {
                DFS(child, ref result);
            }

            result += "]";
        }

        var result = "";
        DFS(sentence, ref result);

        var linkage = string.Empty;
        for (int j = 0; j < i; j++) {
            linkage+= $"\\draw[->,gray] (TruthV{j}) to[out=west,in=south west] (BlockC{j});" + "\n";
        }
        if (!triggered) linkage = string.Empty;
        return @"\begin{forest}" + ReplaceUnicodeToLaTex(result, true) + linkage + @"\end{forest}";
    }

    public static string LaTexEquations(List<string[]> equations, bool inLine = false) {
        var equiv = string.Empty;
        foreach (var eq in equations) {
            if (inLine) {
                equiv += eq[0] + " &\\equiv " + eq[1];
                for (int i = 2; i < eq.Length; i++) {
                    equiv += " \\equiv " + eq[i];
                }

                equiv += "\\\\\n";
            }
            else {
                equiv += eq[0] + " &\\equiv " + eq[1] + " \\\\\n";
                for (int i = 2; i < eq.Length; i++) {
                    equiv += " &\\equiv " + eq[i] + " \\\\\n";
                }
            }
        }

        return "$\\begin{array}{l l}\n" + equiv + "\\end{array}$";
    }
}