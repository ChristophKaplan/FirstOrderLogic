namespace FirstOrderLogic;

public class Quantifier : Connective {
    public readonly Variable Variable;

    public Quantifier(LogicSymbol symbol, Variable variable) : base(symbol) {
        Variable = variable;
    }

    public override string ToString() {
        return $"{base.ToString()} {Variable}";
    }
}
