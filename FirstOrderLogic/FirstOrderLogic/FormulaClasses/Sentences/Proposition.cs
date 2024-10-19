namespace FirstOrderLogic;

public interface IProposition : IAtomicSentence { }

public class Proposition : AtomicSentence, IProposition {
    public Proposition(string propositionSymbol) : base(propositionSymbol) {
    }

    public Proposition(IProposition other) : base(other) {
    }
    
    public override ISentence Clone() => new Proposition(this);


    public override void SubstituteTerm(Term term, Term replacement) {
        //no terms
    }

    public override string ToString() => Symbol;
}