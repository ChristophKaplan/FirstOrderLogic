namespace FirstOrderLogic;

public class ComplexSentence : Sentence {
    public readonly Connective _operator;
    public bool IsNegation => _operator == Connective.LogicSymbol.NEGATION;
    public bool IsQuantifier =>
        _operator == Connective.LogicSymbol.EXISTENTIAL || _operator == Connective.LogicSymbol.UNIVERSAL;

    public ComplexSentence(Sentence p, Connective @operator, Sentence q) {
        _operator = @operator;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(Connective @operator, Sentence p) {
        _operator = @operator;
        AddChild(p);
    }
    
    public ComplexSentence(ComplexSentence other) {
        _operator = other._operator;
        Parent = other.Parent;
        foreach (var child in other.Children) {
            AddChild(child.Clone());
        }
    }

    public void FlipOperator() {
        _operator.Symbol = _operator.Symbol switch {
            Connective.LogicSymbol.CONJUNCTION => Connective.LogicSymbol.DISJUNCTION,
            Connective.LogicSymbol.DISJUNCTION => Connective.LogicSymbol.CONJUNCTION,
            Connective.LogicSymbol.EXISTENTIAL => Connective.LogicSymbol.UNIVERSAL,
            Connective.LogicSymbol.UNIVERSAL => Connective.LogicSymbol.EXISTENTIAL,

            _ => throw new Exception($"Error: {this._operator.Symbol} not found.")
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
    
    public override string ToString() {
        if (IsNegation || IsQuantifier) {
            return $"{_operator} {Children[0]}";
        }

        return $"({Children[0]} {_operator} {Children[1]})";
    }
}