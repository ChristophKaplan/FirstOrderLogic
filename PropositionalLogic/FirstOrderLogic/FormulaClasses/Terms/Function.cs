namespace FirstOrderLogic;

public class Function : Term {
    public readonly Term[] Terms;
    public bool IsConstant => Terms.Length == 0;
    public Function(string termSymbol, Term[] terms) : base(termSymbol) {
        Terms = terms;
    }

    protected Function(string termSymbol) : base(termSymbol) {
        Terms = Array.Empty<Term>();
    }
    
    public override string ToString() {
        return IsConstant ? base.ToString() : $"{base.ToString()}({string.Join<Term>(",", Terms)})";
    }

    public void SubstituteTerm(Term term, Term replacement) {
        for (var i = 0; i < Terms.Length; i++) {
            if (Terms[i].Equals(term)) {
                Terms[i] = replacement;
            }
            else if(Terms[i] is Function function) {
                function.SubstituteTerm(term, replacement);
            }
        }
    }
}