using LRParser.Language;

namespace FirstOrderLogic;

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

public class AtomicSentence : ILanguageObject{
    private readonly string _predicateSymbol;
    private readonly Term[] _terms;
    
    public AtomicSentence(string predicateSymbol, Term[] terms) {
        _predicateSymbol = predicateSymbol;
        _terms = terms;
    }
    
    public override string ToString() {
        return $"{_predicateSymbol}({string.Join<Term>(",", _terms)})";
    }
}