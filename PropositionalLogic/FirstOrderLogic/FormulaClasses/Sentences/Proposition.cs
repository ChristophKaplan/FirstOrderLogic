namespace FirstOrderLogic;

public class Proposition : AtomicSentence {
    public Proposition(string propositionSymbol) : base(propositionSymbol) {
    }

    public Proposition(Proposition other) : base(other) {
    }

    public override void SubstituteTerm(Term term, Term replacement) {
        //no terms
    }

    public override string ToString() => Symbol;
}