using System.Text;
using LRParser.Language;
using ConsoleTables;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject{
    public readonly List<Interpretation> Interpretations;
    public readonly List<Sentence> Sentences;
  
    public InterpretationSet(List<Interpretation> interpretations, params Sentence[] sentences) {
        Interpretations = interpretations;
        Sentences = sentences.ToList();
    }


    private string ToHTMLTable((string[] col, string[][] rows)  table) {
        var htmlTable = new StringBuilder();
        htmlTable.Append("<table style=\"border: 1px solid black;\">");
        htmlTable.Append("<tr style=\"border: 1px solid black;\">");
        foreach (var col in table.col) htmlTable.Append($"<th style=\"border: 1px solid black;\">{col}</th>");
        htmlTable.Append("</tr>");
        foreach (var row in table.rows) {
            htmlTable.Append("<tr style=\"border: 1px solid black;\">");
            foreach (var cell in row) htmlTable.Append($"<td style=\"border: 1px solid black;\">{cell}</td>");
            htmlTable.Append("</tr>");
        }
        htmlTable.Append("</table>");
        return htmlTable.ToString();
    }
    
    private string ToConsoleTable( (string[] col, string[][] rows)  table) {
        var consoleTable = new ConsoleTable(table.col.ToArray());
        foreach (var row in table.rows) consoleTable.AddRow(row);
        return consoleTable.ToMinimalString();
    }
    
    private (string[] col, string[][] rows) ToTable(List<Interpretation> interpretations, List<Sentence> sentences) {
        var columns = interpretations[0].TruthValues.Keys.Select(key => key.ToString()).ToList();
        columns.AddRange(sentences.Select(sentence => sentence.ToString()));
        
        var rows = new List<List<string>>();
        
        foreach (var interpretation in interpretations) {
            var row = new List<string>();
            row.AddRange(interpretation.TruthValues.Values.Select(value => value ? "1":"0").ToList());
            row.AddRange(sentences.Select(sentence => $"{(interpretation.Evaluate(sentence) ? "1" : "0")}"));
            rows.Add(row);
        }
        
        return (columns.ToArray(), rows.Select(row => row.ToArray()).ToArray());
    }
    
    public override string ToString() {
        return ToConsoleTable(ToTable(Interpretations, Sentences));
    }
    
    public string ToHTML() {
        return ToHTMLTable(ToTable(Interpretations, Sentences));
    }
}