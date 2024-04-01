using System.Text;
using LRParser.Language;
using ConsoleTables;
using PropositionalLogic.Helpers;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject {
    public readonly List<Interpretation> Interpretations;
    public readonly List<Sentence> Sentences;
    
    public InterpretationSet(List<Interpretation> interpretations, params Sentence[] sentences) {
        Interpretations = interpretations;
        Sentences = sentences.ToList();
    }

    public string Analyze() {
        string result = "";
        foreach ((Sentence p, Sentence q, bool truth) r in  GetSemanticConsequences()) {
            result += $"{r.p} \u22a8 {r.q} = {r.truth} <br>\n";
        }
        return result;
    }

    public List <(Sentence p, Sentence q, bool truth)> GetSemanticConsequences(bool onlyTrue = false) {
        List<AtomicSentence> atomsFromInts = Interpretations.SelectMany(x => x.TruthValues.Keys).Distinct().ToList();
        List<Sentence> merged = new(Sentences);
        merged.AddRange(atomsFromInts);
        var comb = merged.DifferentCombinations(2);
        var result = new List<(Sentence p, Sentence q, bool truth)>();
        foreach (var c in comb) {
            var prem = c.First();
            var conc = c.Last();
            var semCon = SemanticConsequence(prem, conc);
            if(onlyTrue && !semCon) continue;
            result.Add((prem, conc, semCon));
        }
        
        foreach (var c in comb) {
            var prem = c.Last();
            var conc = c.First();
            var semCon = SemanticConsequence(prem, conc);
            if(onlyTrue && !semCon) continue;
            result.Add((prem, conc, semCon));
        }

        result.Sort((x, y) => x.truth.CompareTo(y.truth));
        
        return result;
    }
    
    public bool SemanticConsequence(Sentence prem, Sentence conc) {
        var premMod = Models(prem).ToHashSet();
        var concMod = Models(conc).ToHashSet();
        return premMod.IsSubsetOf(concMod);
    }

    public List<Interpretation> Models(Sentence sentence) {
        var models = new List<Interpretation>();

        foreach (var interpretation in Interpretations) {
            var mod = interpretation.Evaluate(sentence);
            if (mod) { models.Add(interpretation); }
        }

        return models;
    }

    public void SortBy(string variable) {
        var a = new AtomicSentence(variable);
        Interpretations.Sort((x, y) => x.TruthValues[a].CompareTo(y.TruthValues[a]));
    }
    
    private string ToHTMLTable((string[] col, string[][] rows) table) {
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

    private string ToConsoleTable((string[] col, string[][] rows) table) {
        var columns = table.col;
        var rows = table.rows;
        
        var consoleTable = new ConsoleTable(columns);

        foreach (var row in rows) consoleTable.AddRow(row);
        
        return consoleTable.ToMinimalString();
    }

    private (string[] col, string[][] rows) ToTable(List<Interpretation> interpretations, List<Sentence> sentences) {
        var columns = interpretations[0].TruthValues.Keys.Select(key => key.ToString()).ToList();
        columns.AddRange(sentences.Select(sentence => sentence.ToString()));

        var rows = new List<string[]>();

        foreach (var interpretation in interpretations) {
            var row = new List<string>();
            row.AddRange(interpretation.TruthValues.Values.Select(value => value ? "1":"0").ToList());
            row.AddRange(sentences.Select(sentence => interpretation.Evaluate(sentence)? "1":"0"));
            rows.Add(row.ToArray());
        }

        return (columns.ToArray(), rows.ToArray());
    }

    public override string ToString() {
        return ToConsoleTable(ToTable(Interpretations, Sentences));
    }

    public string ToHTML() {
        return ToHTMLTable(ToTable(Interpretations, Sentences));
    }
}