namespace FirstOrderLogic;

public abstract class AtomicSentence : Sentence {
    public string Symbol;
    public bool IsConstant => Tautology || Contradiction;
    public bool Tautology => Symbol.Equals(Connective.SymbolToString(Connective.LogicSymbol.TRUE));
    public bool Contradiction => Symbol.Equals(Connective.SymbolToString(Connective.LogicSymbol.FALSE));

    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }

    public AtomicSentence(AtomicSentence other) {
        Parent = other.Parent;
        Symbol = other.Symbol;
    }
    
    public void NegateNullary() {
        if (Tautology) {
            Symbol = Connective.SymbolToString(Connective.LogicSymbol.FALSE);
        } else if (Contradiction) {
            Symbol = Connective.SymbolToString(Connective.LogicSymbol.TRUE);
        }
    }
    
    public override string ToString() => Symbol;
}