namespace FirstOrderLogic;

public class Predicate : AtomicSentence {
    public readonly Term[] Terms;

    public Predicate(string predicateSymbol, Term[] terms) : base(predicateSymbol) {
        Terms = terms;
    }

    public Predicate(Predicate other) : base(other) {
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
}
