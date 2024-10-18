namespace FirstOrderLogic;

public class Function : Term {
    public readonly Term[] Terms;
    public int Arity => Terms.Length;
    public bool IsConstant => Arity == 0;

    public Function(string termSymbol, Term[] terms) : base(termSymbol) {
        Terms = terms;
    }

    protected Function(string termSymbol) : base(termSymbol) {
        Terms = Array.Empty<Term>();
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
    
    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return EqualSignature((Function)obj) && Terms.SequenceEqual(((Function)obj).Terms);
    }
    
    public override int GetHashCode() {
        return TermSymbol.GetHashCode();
    }
    
    public bool EqualSignature(Function other) => TermSymbol.Equals(other.TermSymbol) && Arity == other.Arity;
    
    public override string ToString() {
        return IsConstant ? base.ToString() : $"{base.ToString()}({string.Join<Term>(",", Terms)})";
    }
}