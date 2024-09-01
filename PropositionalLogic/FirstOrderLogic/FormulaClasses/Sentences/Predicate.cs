namespace FirstOrderLogic;

public class Predicate : AtomicSentence {
    private readonly Term[] _terms;

    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        _terms = terms;
    }

    public Predicate(Predicate other) : base(other) {
        _terms = other._terms;
    }

    public override string ToString() {
        return $"{Symbol}({string.Join<Term>(",", _terms)})";
    }
}
