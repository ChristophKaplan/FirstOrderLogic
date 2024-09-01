using LRParser.Language;

namespace FirstOrderLogic;

public class Connective : ILanguageObject {
    public enum LogicSymbol {
        CONJUNCTION,
        DISJUNCTION,
        NEGATION,
        IMPLICATION,
        TRUE,
        FALSE,
        EXISTENTIAL,
        UNIVERSAL,
        BICONDITIONAL
    }

    public LogicSymbol Symbol;

    public Connective(LogicSymbol symbol) {
        Symbol = symbol;
    }

    public static implicit operator LogicSymbol(Connective constant) => constant.Symbol; 
    public static implicit operator Connective(LogicSymbol symbol) => new (symbol); 

    public override string ToString() => SymbolToString(Symbol);
    
    public static string SymbolToString(LogicSymbol symbol) {
        return symbol switch {
            LogicSymbol.CONJUNCTION => "\u2227",
            LogicSymbol.DISJUNCTION => "\u2228",
            LogicSymbol.NEGATION => "\u00ac",
            LogicSymbol.IMPLICATION => "\u21d2",
            LogicSymbol.EXISTENTIAL => "\u2203",
            LogicSymbol.UNIVERSAL => "\u2200",
            LogicSymbol.TRUE => "\u22A4",
            LogicSymbol.FALSE => "\u22A5",
            LogicSymbol.BICONDITIONAL => "\u21d4",
            _ => throw new Exception($"Error: {symbol} not found.")
        };
    }
}
