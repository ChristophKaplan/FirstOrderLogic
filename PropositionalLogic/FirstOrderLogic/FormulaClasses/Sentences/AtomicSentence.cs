namespace FirstOrderLogic;

public abstract class AtomicSentence : Sentence {
    public string Symbol;

    public bool IsConstant => Tautology || Contradiction;
    public bool Tautology => Symbol.Equals(Connective.LogicSymbol.TRUE.ToString());
    public bool Contradiction => Symbol.Equals(Connective.LogicSymbol.FALSE.ToString());

    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }

    public AtomicSentence(AtomicSentence other) {
        Parent = other.Parent;
        Symbol = other.Symbol;
    }
    
    public void NegateNullary() {
        if (Tautology) {
            Symbol = Connective.LogicSymbol.FALSE.ToString();
        } else if (Contradiction) {
            Symbol = Connective.LogicSymbol.TRUE.ToString();
        }
    }
    
    public override string ToString() => Symbol;
}