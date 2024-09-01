namespace FirstOrderLogic;

public class Quantifier : Connective {
    private readonly Variable _variable;

    public Quantifier(LogicSymbol symbol, Variable variable) : base(symbol) {
        _variable = variable;
    }

    public override string ToString() {
        return $"{base.ToString()} {_variable}";
    }
}
