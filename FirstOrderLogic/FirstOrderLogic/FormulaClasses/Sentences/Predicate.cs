namespace FirstOrderLogic;

public interface IPredicate : IAtomicSentence {
    Term[] Terms { get; }
    Variable[] GetVariables();
    bool HasBoundVariables();
    bool EqualSignature(IPredicate other);
}

public class Predicate : AtomicSentence, IPredicate, ILiteral {
    public IPredicate Pred => this;
    public Term[] Terms { get; set; }

    public override int Arity => Terms.Length;
    
    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        Terms = terms;
    }

    public Predicate(IPredicate other) : base(other) {
        Terms = new Term[other.Terms.Length];
        for (int i = 0; i < other.Terms.Length; i++) {
            Terms[i] = other.Terms[i].Clone();
        }
    }

    public override void SubstituteTerm(Term term, Term replacement) {
        for (var i = 0; i < Terms.Length; i++) {
            if (Terms[i].Equals(term)) {
                Terms[i] = replacement;
            }
            else if(Terms[i] is Function function) {
                function.SubstituteTerm(term, replacement);
            }
        }
    }
    
    public Variable[] GetVariables() {
        var variables = new List<Variable>();
        foreach (var term in Terms) {
            variables.AddRange(term.GetVariables());
        }
        return variables.ToArray();
    }

    public bool HasBoundVariables() {
        ISentence current = this;
        while (current.Parent != null) {
            current = current.Parent;
            
            if(current is IComplexSentence { IsQuantifier: true }) {
                return true;
            }
        }
        
        return false;
    }
    
    public bool EqualSignature(IPredicate other) => Symbol == other.Symbol && Arity == other.Arity;
    
    public override string ToString() {
        return $"{Symbol}({string.Join<Term>(",", Terms)})";
    }
}
