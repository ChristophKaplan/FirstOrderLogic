namespace FirstOrderLogic;

public interface ILiteral : IPredicate, IComplexSentence {
    IPredicate Predicate { get; }
    Term[] Terms { get; }
}

public class Literal : Sentence, ILiteral {
    public Term[] Terms { get; }
    public Connective Connective { get; }
    public string Symbol { get; set; }
    public bool IsNullaryConstant { get; }
    public bool Tautology { get; }
    public bool Contradiction { get; }
    public bool IsNegation { get; }
    public bool IsQuantifier => false;
    public IPredicate Predicate => this is IPredicate predicate ? predicate : (IPredicate)Children[0];
    public Variable[] GetVariables() => Predicate.GetVariables();
    public bool HasBoundVariables() => Predicate.HasBoundVariables();
    
    public override void SubstituteTerm(Term term, Term replacement) {
        Predicate.SubstituteTerm(term, replacement);
    }
    
    public override void Negate() {
        if(this is IPredicate predicate) {
            predicate.Negate();
        }
        
        if(this is IComplexSentence complexSentence) {
            complexSentence.Negate();
        }
    }

    public void FlipOperator() {
        throw new NotImplementedException();
    }
    public ISentence GetSiblingOf(ISentence sentence) {
        throw new NotImplementedException();
    }
}