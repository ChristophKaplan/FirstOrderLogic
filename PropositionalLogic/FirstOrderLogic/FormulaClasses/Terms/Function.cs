namespace FirstOrderLogic;

public class Function : Term {
    private readonly Term[] _terms;

    public Function(string termSymbol, Term[] terms) : base(termSymbol) {
        _terms = terms;
    }

    public override string ToString() {
        return $"{base.ToString()}({string.Join<Term>(",", _terms)})";
    }
}