namespace FirstOrderLogic;

public interface IComplexSentence : ISentence, ILiteral{
    Connective Connective { get; }
    bool IsNegation { get; }
    bool IsQuantifier { get; }
    void FlipOperator();
    ISentence GetSiblingOf(ISentence sentence);
}

public class ComplexSentence : Sentence, IComplexSentence, ILiteral {
    public Connective Connective { get; set; }
    public bool IsNegation => Connective == Connective.LogicSymbol.NEGATION;
    public bool IsQuantifier => Connective == Connective.LogicSymbol.EXISTENTIAL || Connective == Connective.LogicSymbol.UNIVERSAL;
    
    public ComplexSentence(ISentence p, Connective connective, ISentence q) {
        Connective = connective;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(Connective connective, ISentence p) {
        Connective = connective;
        AddChild(p);
    }

    public ComplexSentence(IComplexSentence other) {
        Connective = other.Connective;
        Parent = other.Parent;
        foreach (var child in other.Children) {
            AddChild(child.Clone());
        }
    }

    public void FlipOperator() {
        Connective.Symbol = Connective.Symbol switch {
            Connective.LogicSymbol.CONJUNCTION => Connective.LogicSymbol.DISJUNCTION,
            Connective.LogicSymbol.DISJUNCTION => Connective.LogicSymbol.CONJUNCTION,
            Connective.LogicSymbol.EXISTENTIAL => Connective.LogicSymbol.UNIVERSAL,
            Connective.LogicSymbol.UNIVERSAL => Connective.LogicSymbol.EXISTENTIAL,
            _ => throw new Exception($"Error: {this.Connective.Symbol} not found.")
        };
    }

    public ISentence GetSiblingOf(ISentence sentence) {
        if (Children.Count != 2) {
            throw new Exception("Error: ComplexSentence must have two children.");
        }

        if (Children[0].Equals(sentence)) {
            return Children[1];
        }

        if (Children[1].Equals(sentence)) {
            return Children[0];
        }

        throw new Exception("Error: Sentence not found in ComplexSentence.");
    }

    public override void SubstituteTerm(Term term, Term replacement) {
        foreach (var child in Children) {
            child.SubstituteTerm(term, replacement);
        }
    }

    public override void Negate() {
        var negated = IsNegation ? Children[0] : new ComplexSentence(Connective.LogicSymbol.NEGATION, Clone());
        negated.SetParentToParentOf(this);
    }

    public override string ToString() {
        return Children.Count == 1 ? $"{Connective} {Children[0]}" : $"({Children[0]} {Connective} {Children[1]})";
    }

    public IPredicate Pred => Children[0] as IPredicate;
    public Term[] Terms => Pred.Terms;
}