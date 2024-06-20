using System.Text;
using LRParser.Language;

namespace PropositionalLogic;

public class Interpretation : ILanguageObject{
    public readonly Dictionary<AtomicSentence, bool> Assignment = new();
    
    public Interpretation() { }

    public bool IsModel(Sentence sentence) {
        return Evaluate(sentence);
    }
    
    public Interpretation(Interpretation other) {
        foreach (var kv in other.Assignment) {
            Assignment.Add(kv.Key, kv.Value);
        }
    }
    
    public void Add(AtomicSentence atom, bool truthValue) {
        Assignment.TryAdd(atom, truthValue);
    }

    private bool Evaluate(AtomicSentence atomicSentence) {
        if (atomicSentence.Verum) return true;
        if (atomicSentence.Falsum) return false;
        
        if (Assignment.TryGetValue(atomicSentence, out var value)) {
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
        if (Assignment.Count != other.Assignment.Count) {
            return false;
        }
        foreach (var kv in Assignment) {
            if (!other.Assignment.TryGetValue(kv.Key, out var value)) {
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
        foreach (var kv in Assignment) {
            var (key, value) = kv;
            hash = HashCode.Combine(hash ,key, value);
        }

        return hash;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in Assignment) {
            sb.Append($"{key}={value}, ");
        }

        return sb.ToString();
    }

    public string To01String()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in Assignment) {
            var s = value ? "1" : "0";
            sb.Append( $"{s}");
        }

        return sb.ToString();
    }

    
    public string ToHTML() {
        throw new NotImplementedException();
    }
    
    public Interpretation Switch(Interpretation other, AtomicSentence variable) {
        if(!AgreeExceptPossibly(other, variable)) return null;
        var interpretation = new Interpretation(this);
        interpretation.Assignment[variable] = !interpretation.Assignment[variable];
        return interpretation;
    }
    
    public Interpretation Force(Interpretation other, AtomicSentence variable) {
        if(!AgreeExceptPossibly(other, variable)) return null;
        var interpretation = new Interpretation(this);
        interpretation.Assignment[variable] = other.Assignment[variable];
        return interpretation;
    }

    private bool AgreeExceptPossibly(Interpretation other, AtomicSentence variable) {
        foreach (var key in Assignment.Keys) {
            if(Assignment.TryGetValue(key,out var value1) && other.Assignment.TryGetValue(key,out var value2))
            {
                if(key.Equals(variable)) continue;
                if (value1 != value2) return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}