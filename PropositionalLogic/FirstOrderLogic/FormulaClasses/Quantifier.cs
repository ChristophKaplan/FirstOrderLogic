namespace FirstOrderLogic;

public class Quantifier : Connective {
    public readonly Variable Variable;

    public Quantifier(LogicSymbol symbol, Variable variable) : base(symbol) {
        Variable = variable;
    }
    
    public override string ToString() {
        return $"{base.ToString()} {Variable}";
    }

    public override bool Equals(object? obj) {
        return obj is Quantifier quantifier && Symbol == quantifier.Symbol && Variable.Equals(quantifier.Variable);
    }
    
    public override int GetHashCode() {
        return HashCode.Combine(Symbol, Variable);
    }
}
