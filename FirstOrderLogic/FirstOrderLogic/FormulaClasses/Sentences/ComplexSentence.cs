namespace FirstOrderLogic;

public interface IComplexSentence : ISentence{
    Connective Connective { get; }
    bool IsNegation { get; }
    bool IsQuantifier { get; }
    void FlipOperator();
    ISentence GetSiblingOf(ISentence sentence);
    Quantifier[] GetQuantifiers(Connective.LogicSymbol quantifier);
}
public class ComplexSentence : Sentence, IComplexSentence{
    public Connective Connective { get; set; }
    public bool IsNegation => Connective == Connective.LogicSymbol.NEGATION;
    public bool IsQuantifier => Connective == Connective.LogicSymbol.EXISTENTIAL || Connective == Connective.LogicSymbol.UNIVERSAL;
    
    public ComplexSentence(ISentence p, Connective.LogicSymbol logicSymbol, ISentence q) {
        Connective = new Connective(logicSymbol);
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(Connective.LogicSymbol logicSymbol, ISentence p) {
        Connective = new Connective(logicSymbol);
        AddChild(p);
    }
    
    public ComplexSentence(Connective connective, ISentence p) {
        Connective = connective;
        AddChild(p);
    }

    public ComplexSentence(IComplexSentence other) {
        Connective = other.Connective.Clone();
        Parent = null; //other.Parent; //TODO: def not only assign the parent, maybe clone it and all the other siblings ?
        
        foreach (var child in other.Children) {
            AddChild(child.Clone());
        }
    }

    public override ISentence Clone() => new ComplexSentence(this);

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

    public Quantifier[] GetQuantifiers(Connective.LogicSymbol quantifier) {
        var quantifiers = new List<Quantifier>();
        if (Connective.Symbol == quantifier) {
            quantifiers.Add((Quantifier)Connective);
        }
        
        foreach (var child in Children) {
            if (child is IComplexSentence complex) { 
                quantifiers.AddRange(complex.GetQuantifiers(quantifier));
            }
        }

        return quantifiers.ToArray();
    }

    public override void SubstituteTerm(Term term, Term replacement) {
        foreach (var child in Children) {
            child.SubstituteTerm(term, replacement);
        }
    }
    
    public override ISentence Negate() {
        var negated = IsNegation ? Children[0] : new ComplexSentence(Connective.LogicSymbol.NEGATION, Clone());
        negated.SetParentToParentOf(this);
        return negated;
    }

    public override string ToString() {
        return Children.Count == 1 ? $"{Connective} {Children[0]}" : $"({Children[0]} {Connective} {Children[1]})";
    }

    public IPredicate Pred => Children[0] as IPredicate;
    public Term[] Terms => Pred.Terms;
}