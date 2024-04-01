using System.Text;
using LRParser.Language;

namespace PropositionalLogic;

public class Interpretation : ILanguageObject{
    public readonly Dictionary<AtomicSentence, bool> TruthValues = new();
    
    public Interpretation() { }

    private Interpretation(Interpretation other) {
        foreach (var kv in other.TruthValues) {
            TruthValues.Add(kv.Key, kv.Value);
        }
    }
    
    public Interpretation Switch(AtomicSentence variable) {
        var interpretation = new Interpretation(this);
        interpretation.TruthValues[variable] = !interpretation.TruthValues[variable];
        return interpretation;
    }
    
    public void Add(AtomicSentence atom, bool truthValue) {
        TruthValues.TryAdd(atom, truthValue);
    }

    private bool Evaluate(AtomicSentence atomicSentence) {
        if (atomicSentence.Tautology) return true;
        if (atomicSentence.Falsum) return false;
        
        if (TruthValues.TryGetValue(atomicSentence, out var value)) {
            return value;
        }

        throw new Exception($"Error: {atomicSentence} not found in interpretation.");
    }

    private bool Evaluate(ComplexSentence complexSentence) {
        return complexSentence.Operator switch {
            LogicalConstant.LSymbol.NOT => !Evaluate(complexSentence.Children[0]),
            LogicalConstant.LSymbol.AND => Evaluate(complexSentence.Children[0]) && Evaluate(complexSentence.Children[1]),
            LogicalConstant.LSymbol.OR => Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]),
            LogicalConstant.LSymbol.IMPLIES => !Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }

    public bool Evaluate(Sentence sentence) {
        return sentence switch {
            AtomicSentence atomicSentence => Evaluate(atomicSentence),
            ComplexSentence complexSentence => Evaluate(complexSentence),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }

    public bool EqualVariables(Interpretation other) {
        if (TruthValues.Count != other.TruthValues.Count) {
            return false;
        }
        foreach (var kv in TruthValues) {
            if (!other.TruthValues.TryGetValue(kv.Key, out var value)) {
                return false;
            }
        }
        
        return true;
    }
    
    public override bool Equals(object? obj) {
        return GetHashCode().Equals(obj?.GetHashCode());
    }

    public override int GetHashCode() {
        var hash = 17;
        foreach (var kv in TruthValues) {
            var (key, value) = kv;
            hash = HashCode.Combine(hash ,key, value);
        }

        return hash;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in TruthValues) {
            sb.Append($"{key}={value}, ");
        }

        return sb.ToString();
    }

    public string ToHTML() {
        throw new NotImplementedException();
    }
}