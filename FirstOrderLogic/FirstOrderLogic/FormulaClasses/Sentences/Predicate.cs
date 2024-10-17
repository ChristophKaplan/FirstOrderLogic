namespace FirstOrderLogic;

public interface IPredicate : IAtomicSentence, ILiteral {
    Term[] Terms { get; }
    Variable[] GetVariables();
    bool HasBoundVariables();
}

public class Predicate : AtomicSentence, IPredicate, ILiteral {
    public IPredicate Pred => this;
    public Term[] Terms { get; set; }

    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        Terms = terms;
    }

    public Predicate(IPredicate other) : base(other) {
        Terms = other.Terms;
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

    public override string ToString() {
        return $"{Symbol}({string.Join<Term>(",", Terms)})";
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
}
