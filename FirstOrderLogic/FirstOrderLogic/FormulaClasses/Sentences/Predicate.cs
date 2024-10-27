namespace FirstOrderLogic;

public interface IPredicate : IAtomicSentence {
    Term[] Terms { get; }
    Variable[] GetVariables();
    bool HasBoundVariables();
    bool EqualSignature(IPredicate other);
}

public class Predicate : AtomicSentence, IPredicate {
    public Term[] Terms { get; }
    public override int Arity => Terms.Length;
    public override ISentence Clone() => new Predicate(this);
    public bool EqualSignature(IPredicate other) => Symbol == other.Symbol && Arity == other.Arity;

    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        Terms = terms;
    }
    
    public Predicate(string predicateSymbol, Term[] terms, int time) : base(predicateSymbol, time) {
        Terms = terms;
    }

    private Predicate(IPredicate other) : base(other) {
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

    public override bool Equals(object? obj) {
        return base.Equals(obj) && Terms.SequenceEqual(((Predicate)obj).Terms);
    }
    
    public override int GetHashCode() {
        var hash = base.GetHashCode();
        return Terms.Aggregate(hash, (current, term) => HashCode.Combine(current, term.GetHashCode()));
    }

    public override string ToString() {
        return $"{Symbol}({string.Join<Term>(",", Terms)}){(Time.HasValue ? $"^{Time}" : "")}";
    }
}
