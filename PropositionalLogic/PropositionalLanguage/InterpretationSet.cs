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

    public bool IsEmptySet => Interpretations.Count == 0;
    public bool IsOmega => Interpretations.Count == Math.Pow(2, GetSignature().Count);
    
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
            if (interpretation.IsModel(sentence)) { models.Add(interpretation); }
        }
        return models;
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

        if (IsEmptySet) {
            rows.Add(new []{@"$\emptyset$"});
        }
        
        foreach (var interpretation in interpretations) {
            var row = new List<string>();
            row.AddRange(interpretation.Assignment.Values.Select(value => value ? "1":"0").ToList());
            row.AddRange(sentences.Select(sentence => interpretation.Evaluate(sentence)? "1":"0"));
            rows.Add(row.ToArray());
        }

        //rows.Add(new []{$"\\multicolumn{{{columns.Count}}}{{c}}{{{AnalyzeLatex()}}}"});
        return (columns.ToArray(), rows.ToArray());
    }
    
    private string ToSetString() {
        var result = "{";
        if(IsEmptySet) return result + "empty set}";
        if(IsOmega) return result + "Omega}";
        
        for (var i = 0; i < Interpretations.Count-1; i++) {
            result += Interpretations[i].To01String() + ", ";
        }
        result += Interpretations[^1].To01String();
        
        return result + "}";
    }

    public override string ToString() {
        bool table = false;
        return table ? ToConsoleTable(ToTable()) : ToSetString();
    }
}