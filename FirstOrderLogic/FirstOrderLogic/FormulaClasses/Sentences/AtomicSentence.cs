namespace FirstOrderLogic;

public interface IAtomicSentence : ISentence {
    string Symbol { get; set; }
    bool IsNullaryConstant { get; }
    bool Tautology { get; }
    bool Contradiction { get; }
}

public abstract class AtomicSentence : Sentence, IAtomicSentence {
    public string Symbol { get; set; }
    public bool IsNullaryConstant => Tautology || Contradiction;
    public bool Tautology => Symbol.Equals(Connective.SymbolToString(Connective.LogicSymbol.TRUE));
    public bool Contradiction => Symbol.Equals(Connective.SymbolToString(Connective.LogicSymbol.FALSE));

    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }

    public AtomicSentence(IAtomicSentence other) {
        Parent = null; //other.Parent;
        Symbol = other.Symbol;
    }
    
    public override void Negate() {
        if (Tautology) {
            Symbol = Connective.SymbolToString(Connective.LogicSymbol.FALSE);
        } else if (Contradiction) {
            Symbol = Connective.SymbolToString(Connective.LogicSymbol.TRUE);
        }
    }
    
    public override string ToString() => Symbol;
}