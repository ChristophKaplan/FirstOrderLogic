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
    
    public Function(Function other) : base(other.TermSymbol) {
        Terms = new Term[other.Terms.Length];
        for (var i = 0; i < other.Terms.Length; i++) {
            Terms[i] = other.Terms[i].Clone();
        }
    }
    
    public void SubstituteTerm(Term term, Term replacement) {
        for (var i = 0; i < Terms.Length; i++) {
            var curTerm = Terms[i];
            if (curTerm.Equals(term)) {
                Terms[i] = replacement;
            }
            else if(curTerm is Function function) {
                function.SubstituteTerm(term, replacement);
            }
        }
    }
    
    public override bool Equals(object? obj) {
        if (obj is not Function other || obj is Variable) {
            return false;
        }
        
        if(!EqualSignature(other)) {
            return false;
        }
        
        if (Terms.Length != other.Terms.Length) {
            return false;
        }
        
        for (var i = 0; i < Terms.Length; i++) {
            if (!Terms[i].Equals(other.Terms[i])) {
                return false;
            }
        }
        
        return true;
    }
    
    public override int GetHashCode() {
        return TermSymbol.GetHashCode();
    }
    
    public bool EqualSignature(Function other) => TermSymbol.Equals(other.TermSymbol) && Arity == other.Arity;
    
    public override string ToString() {
        return IsConstant ? base.ToString() : $"{base.ToString()}({string.Join<Term>(",", Terms)})";
    }
}