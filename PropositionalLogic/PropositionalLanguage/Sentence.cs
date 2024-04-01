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

    public string ToHTML() {
        throw new NotImplementedException();
    }
}

public class Function : ILanguageObject {
    public readonly string Func;
    public readonly ILanguageObject[] Parameters;

    public Function(string func, params ILanguageObject[] parameters) {
        Func = func;
        Parameters = parameters;
    }

    public string ToHTML() {
        throw new NotImplementedException();
    }
}

public abstract class Sentence : ILanguageObject {
    private Sentence _parent;
    public readonly List<Sentence> Children = new();

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence._parent = this;
    }
    
    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index,sentence);
        sentence._parent = this;
    }
    
    public void Reparent(Sentence parentOfThis) {
        if (parentOfThis._parent == null) {
            return;
        }
        
        Sentence parent = parentOfThis._parent;
        Sentence found = null;
        foreach (var childInParent in parent.Children) {
            if (childInParent.Equals(parentOfThis)) {
                found = childInParent;
            }
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
            return atomicSentence.Symbol;
        }

        if (this is ComplexSentence complexSentence) {
            if (complexSentence.Operator.Equals(LogicalConstant.LSymbol.NOT)) {
                return $"{complexSentence.Operator} {complexSentence.Children[0]}";
            }

            return $"({complexSentence.Children[0]} {complexSentence.Operator} {complexSentence.Children[1]})";
        }

        return "Sentence";
    }

    public string ToHTML() {
        throw new NotImplementedException();
    }
}

public class AtomicSentence : Sentence {
    public string Symbol;
    public bool IsTruthValue { get => Tautology || Falsum; }
    public bool Tautology { get => Symbol.Equals(LogicalConstant.LSymbol.TRUE.ToString());}
    public bool Falsum { get => Symbol.Equals(LogicalConstant.LSymbol.FALSE.ToString());}
    
    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }
    public AtomicSentence(LexValue symbol) {
        Symbol = symbol.Value;
    }
}

public class ComplexSentence : Sentence {
    public readonly LogicalConstant.LSymbol Operator;

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
}