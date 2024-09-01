using LRParser.Language;

namespace FirstOrderLogic;

public class LogicalConstant : ILanguageObject {
    public enum LSymbol {
        AND,
        OR,
        NOT,
        IMPLIES,
        TRUE,
        FALSE,
        ExistentialQuantifier,
        UniversalQuantifier,
    }

    private readonly LSymbol _quantifierType;

    public LogicalConstant(LSymbol quantifierType) {
        _quantifierType = quantifierType;
    }

    public static implicit operator LSymbol(LogicalConstant constant) {
        return constant._quantifierType;
    }

    public static implicit operator LogicalConstant(LSymbol symbol) {
        return new LogicalConstant(symbol);
    }

    public override string ToString() {
        return LSymbolToString(_quantifierType);
    }

    private string LSymbolToString(LSymbol @operator) {
        return @operator switch {
            LSymbol.AND => "\u2227",
            LSymbol.OR => "\u2228",
            LSymbol.NOT => "\u00ac",
            LSymbol.IMPLIES => "\u21d2",
            LSymbol.ExistentialQuantifier => "\u2203",
            LSymbol.UniversalQuantifier => "\u2200",
            _ => throw new Exception($"Error: {this} not found.")
        };
    }
}

public class Quantifier : LogicalConstant {
    private readonly Variable _variable;

    public Quantifier(LSymbol quantifierType, Variable variable) : base(quantifierType) {
        _variable = variable;
    }

    public override string ToString() {
        return $"{base.ToString()} {_variable}";
    }
}

public abstract class Term : ILanguageObject {
    private readonly string _termSymbol;

    public Term(string termSymbol) {
        _termSymbol = termSymbol;
    }

    public override string ToString() {
        return _termSymbol;
    }
}

public class Function : Term {
    private readonly Term[] _terms;

    public Function(string termSymbol, Term[] terms) : base(termSymbol) {
        _terms = terms;
    }

    public override string ToString() {
        return $"{base.ToString()}({string.Join<Term>(",", _terms)})";
    }
}

public class Constant : Term {
    public Constant(string termSymbol) : base(termSymbol) {
    }
}

public class Variable : Term {
    public Variable(string termSymbol) : base(termSymbol) {
    }
}

public abstract class Sentence : ILanguageObject {
    public Sentence Parent { get; private set; }

    public readonly List<Sentence> Children = new();

    public bool IsLiteral =>
        this is AtomicSentence || (this is ComplexSentence { IsNegation: true } complexSentence && complexSentence.Children[0] is AtomicSentence);

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence.Parent = this;
    }

    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index, sentence);
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
        return "Sentence";
    }
}

public class ComplexSentence : Sentence {
    private readonly LogicalConstant _operator;

    public bool IsNegation => Children.Count == 1;

    public bool IsQuantifier =>
        _operator == LogicalConstant.LSymbol.ExistentialQuantifier || _operator == LogicalConstant.LSymbol.UniversalQuantifier;

    public ComplexSentence(Sentence p, LogicalConstant @operator, Sentence q) {
        _operator = @operator;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(LogicalConstant @operator, Sentence p) {
        _operator = @operator;
        AddChild(p);
    }

    public override string ToString() {
        if (IsNegation || IsQuantifier) {
            return $"{_operator} {Children[0]}";
        }

        return $"({Children[0]} {_operator} {Children[1]})";
    }
}

public abstract class AtomicSentence : Sentence {
    protected readonly string Symbol;

    protected AtomicSentence(string symbol) {
        Symbol = symbol;
    }

    public override string ToString() => Symbol;
}

public class Proposition : AtomicSentence {
    public Proposition(string propositionSymbol) : base(propositionSymbol) {
    }

    public override string ToString() => Symbol;
}

public class Predicate : AtomicSentence {
    private readonly Term[] _terms;

    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        _terms = terms;
    }

    public override string ToString() {
        return $"{Symbol}({string.Join<Term>(",", _terms)})";
    }
}