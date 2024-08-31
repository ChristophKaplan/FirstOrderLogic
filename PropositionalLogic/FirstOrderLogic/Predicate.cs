using LRParser.Language;

namespace FirstOrderLogic;


public class LogicalConstant : ILanguageObject {
    public enum LSymbol{
        AND,
        OR,
        NOT,
        IMPLIES,
        TRUE,
        FALSE,
        EXISTS,
        FORALL,
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

public abstract class Term : ILanguageObject
{
    private readonly string _termSymbol;
    
    public Term(string termSymbol) {
        _termSymbol = termSymbol;
    }
    
    public override string ToString() {
        return _termSymbol;
    }
}

public class Function : Term
{
    private readonly Term[] _terms;
    public Function(string termSymbol, Term[] terms) : base(termSymbol)
    {
        _terms = terms;
    }
    
    public override string ToString() {
        return $"{base.ToString()}({string.Join<Term>(",", _terms)})";
    }
}

public class Constant : Term
{
    public Constant(string termSymbol) : base(termSymbol) { }
}

public class Variable : Term
{
    public Variable(string termSymbol) : base(termSymbol) { }
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
            return atomicSentence.ToString();
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
}


public abstract class AtomicSentence : Sentence{
    protected readonly string _symbol;

    protected AtomicSentence(string symbol)
    {
        _symbol = symbol;
    }
}

public class Proposition : AtomicSentence{
    public Proposition(string propositionSymbol): base(propositionSymbol) { }
    public override string ToString() => _symbol;
}

public class Predicate : AtomicSentence{
    private readonly Term[] _terms;
    
    public Predicate(string predicateSymbol, Term[] terms): base(predicateSymbol) {
        _terms = terms;
    }
    
    public override string ToString() {
        return $"{_symbol}({string.Join<Term>(",", _terms)})";
    }
}