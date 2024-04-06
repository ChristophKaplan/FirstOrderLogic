using System.Numerics;
using LRParser.Language;
using ConsoleTables;
using PropositionalLogic.Helpers;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject {
    public readonly List<Interpretation> Interpretations;
    public readonly List<Sentence> Sentences;
    public List<AtomicSentence> GetSignature() {
        return Interpretations.Count > 0 ? Interpretations[0].Assignment.Keys.ToList() : new List<AtomicSentence>();
    }

    public int FindPosInSignature(string variable) {
        var signature = GetSignature();
        for (var i = 0; i < signature.Count; i++) {
            if (signature[i].Symbol == variable) {
                return i;
            }
        }
        return -1;
    }
    
    public InterpretationSet(List<Interpretation> interpretations, params Sentence[] sentences) {
        Interpretations = interpretations;
        Sentences = sentences.ToList();
    }

    public string Analyze() {
        string result = "";
        foreach ((Sentence p, Sentence q, bool truth) r in  GetSemanticConsequences()) {
            result += $"{r.p} \u22a8 {r.q} = {r.truth}\n";
        }
        return result;
    }

    public List <(Sentence p, Sentence q, bool truth)> GetSemanticConsequences(bool onlyTrue = false) {
        List<AtomicSentence> atomsFromInts = Interpretations.SelectMany(x => x.Assignment.Keys).Distinct().ToList();
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

    public InterpretationSet ForceAll(AtomicSentence variable) {
        var list = new List<Interpretation>();
        foreach (var interpretation in Interpretations) {
            var f = interpretation.Force(interpretation, variable);
            if (f != null) list.Add(f);
        }

        return new InterpretationSet(list, Sentences.ToArray());
    }
    
    public InterpretationSet SwitchAll(AtomicSentence variable) {
        var list = new List<Interpretation>();
        foreach (var interpretation in Interpretations) {
            var s = interpretation.Switch(interpretation, variable);
            if (s != null)list.Add(s);
        }
        return new InterpretationSet(list, Sentences.ToArray());
    }
    

    
    public void SortBy(string variable) {
        var a = new AtomicSentence(variable);
        Interpretations.Sort((x, y) => x.Assignment[a].CompareTo(y.Assignment[a]));
    }

    private string ToConsoleTable((string[] col, string[][] rows) table) {
        var columns = table.col;
        var rows = table.rows;
        
        var consoleTable = new ConsoleTable(columns);

        foreach (var row in rows) consoleTable.AddRow(row);
        
        return consoleTable.ToMinimalString();
    }
    
    public (string[] col, string[][] rows) ToTable(List<string> altSenColumn = null) => ToTable(Interpretations, Sentences, altSenColumn);
    private (string[] col, string[][] rows) ToTable(List<Interpretation> interpretations, List<Sentence> sentences, List<string> altSenColumn) {
        var columns = GetSignature().Select(key => key.ToString()).ToList();
        
        if (altSenColumn == null) {
            columns.AddRange(sentences.Select(sentence => sentence.ToString()));
        }
        else {
            columns.AddRange(altSenColumn);
        }

        var rows = new List<string[]>();

        foreach (var interpretation in interpretations) {
            var row = new List<string>();
            row.AddRange(interpretation.Assignment.Values.Select(value => value ? "1":"0").ToList());
            row.AddRange(sentences.Select(sentence => interpretation.Evaluate(sentence)? "1":"0"));
            rows.Add(row.ToArray());
        }

        return (columns.ToArray(), rows.ToArray());
    }

    public override string ToString() {
        return ToConsoleTable(ToTable());
    }

    public string ToHTML() {
        return MRKUPGen.ToHTMLTable(ToTable());
    }
    
    public string ToLaTex(){
        var table = ToTable();
        var manager = new MRKUPGen.TkizMarkerManager();
        return MRKUPGen.ToLaTexTable(table, manager);
    }
}