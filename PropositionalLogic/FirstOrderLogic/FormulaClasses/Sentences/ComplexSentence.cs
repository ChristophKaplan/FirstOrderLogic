namespace FirstOrderLogic;

public class ComplexSentence : Sentence {
    public readonly Connective Connective;
    public bool IsNegation => Connective == Connective.LogicSymbol.NEGATION;
    public bool IsQuantifier =>
        Connective == Connective.LogicSymbol.EXISTENTIAL || Connective == Connective.LogicSymbol.UNIVERSAL;

    public ComplexSentence(Sentence p, Connective connective, Sentence q) {
        Connective = connective;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(Connective connective, Sentence p) {
        Connective = connective;
        AddChild(p);
    }
    
    public ComplexSentence(ComplexSentence other) {
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

    public Sentence GetOtherSide(Sentence sentence) {
        if(Children.Count != 2) {
            throw new Exception("Error: ComplexSentence must have two children.");
        }
        if(Children[0].Equals(sentence)) {
            return Children[1];
        }
        if(Children[1].Equals(sentence)) {
            return Children[0];
        }
        throw new Exception("Error: Sentence not found in ComplexSentence.");
    }

    public override void SubstituteTerm(Term term, Term replacement) {
        foreach (var child in Children) {
            child.SubstituteTerm(term, replacement);
        }
    }

    public override string ToString() {
        if (IsNegation || IsQuantifier) {
            return $"{Connective} {Children[0]}";
        }

        return $"({Children[0]} {Connective} {Children[1]})";
    }
}