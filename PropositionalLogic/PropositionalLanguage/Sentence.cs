using LRParser.Language;

namespace PropositionalLogic;

public class LogicalConstant : ILanguageObject {
    public enum LSymbol{
        AND,
        OR,
        NOT,
        IMPLIES,
        TRUE,
        FALSE
    }
    
    private readonly LSymbol Symbol;
    
    public LogicalConstant(LSymbol symbol) {
        Symbol = symbol;
    }
    
    public static implicit operator LSymbol(LogicalConstant constant)
    {
        return constant.Symbol;
    }
    
    public static implicit operator LogicalConstant(LSymbol symbol)
    {
        return new LogicalConstant(symbol);
    }
    
    public override string ToString() {
        return Symbol.ToString();
    }
}

public class Function : ILanguageObject {
    public readonly string Func;
    public readonly ILanguageObject[] Parameters;

    public Function(string func, params ILanguageObject[] parameters) {
        Func = func;
        Parameters = parameters;
    }
}

public abstract class Sentence : ILanguageObject {
    public Sentence Parent { get; private set; }
    
    public readonly List<Sentence> Children = new();

    public bool IsLiteral => this is AtomicSentence ||
                             (this is ComplexSentence { IsNegation: true } complexSentence && complexSentence.Children[0] is AtomicSentence);
    
    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence.Parent = this;
    }
    
    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index,sentence);
        sentence.Parent = this;
    }
    
    public void Reparent(Sentence parentOfThis) {
        if (parentOfThis.Parent == null) {
            return;
        }
        
        Sentence parent = parentOfThis.Parent;
        Sentence found = null;
        foreach (var childInParent in parent.Children) {
            if (childInParent.Equals(parentOfThis)) {
                found = childInParent;
            }
        }

        if (found == null) {
            throw new Exception($"this not found in Parent.Children");
        }
        
        var index = parent.Children.IndexOf(found);
        parent.Children.RemoveAt(index);
        parent.InsertChild(index, this);
    }
    
    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        if (this is AtomicSentence atomicSentence) {
            if (atomicSentence.Contradiction) return "\u22A5";
            if (atomicSentence.Tautology) return "\u22A4";
            return atomicSentence.Symbol;
        }

        if (this is ComplexSentence complexSentence) {
            if (complexSentence.IsNegation) {
                return $"{complexSentence.OperatorToString()} {complexSentence.Children[0]}";
            }

            return $"({complexSentence.Children[0]} {complexSentence.OperatorToString()} {complexSentence.Children[1]})";
        }

        return "Sentence";
    }

    protected string GetOperatorStringSymbol(LogicalConstant.LSymbol @operator) {
        return @operator switch {
            LogicalConstant.LSymbol.AND => "\u2227",
            LogicalConstant.LSymbol.OR => "\u2228",
            LogicalConstant.LSymbol.NOT => "\u00ac",
            LogicalConstant.LSymbol.IMPLIES => "\u21d2",
            _ => throw new Exception($"Error: {this} not found.")
        };
    }
}

public class AtomicSentence : Sentence {
    public string Symbol;
    public bool IsConstant { get => Tautology || Contradiction; }
    public bool Tautology { get => Symbol.Equals(LogicalConstant.LSymbol.TRUE.ToString());}
    public bool Contradiction { get => Symbol.Equals(LogicalConstant.LSymbol.FALSE.ToString());}
    
    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }
    public AtomicSentence(LexValue symbol) {
        Symbol = symbol.Value;
    }
    
    public void FlipTruthValue() {
        if (Tautology) {
            Symbol = LogicalConstant.LSymbol.FALSE.ToString();
        } else if (Contradiction) {
            Symbol = LogicalConstant.LSymbol.TRUE.ToString();
        }
    }
}

public class ComplexSentence : Sentence {
    public LogicalConstant.LSymbol Operator;
    public string OperatorToString() => GetOperatorStringSymbol(Operator);
    
    public bool IsNegation => Children.Count == 1; 
    
    public ComplexSentence(Sentence p, LogicalConstant.LSymbol @operator, Sentence q) {
        Operator = @operator;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(LogicalConstant.LSymbol @operator, Sentence p) {
        Operator = @operator;
        AddChild(p);
    }
    
    public ComplexSentence(LogicalConstant.LSymbol @operator) {
        Operator = @operator;
    }
    
    public void FlipOperator() {
        Operator = Operator switch {
            LogicalConstant.LSymbol.AND => LogicalConstant.LSymbol.OR,
            LogicalConstant.LSymbol.OR => LogicalConstant.LSymbol.AND,
            _ => throw new Exception($"Error: {this} not found.")
        };
    }

    public Sentence GetOtherSide(Sentence sentence) {
        if(Children.Count != 2) {
            throw new Exception("Error: ComplexSentence must have two children.");
        }
        if(Children[0].Equals(sentence)) {
            return Children[1];
        }
        if(Children[1].Equals(sentence)) {
            return Children[0];
        }
        throw new Exception("Error: Sentence not found in ComplexSentence.");
    }
}