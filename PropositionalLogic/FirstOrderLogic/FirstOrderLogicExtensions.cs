using LRParser.Language;

namespace FirstOrderLogic;

public static class FirstOrderLogicExtensions
{
    public static LogicalConstant ToLogicalConstant(this LexValue lexValue) {
        switch (lexValue.Value) {
            case "OR":
            case "||":
                return LogicalConstant.LSymbol.OR;
            case "AND":
            case "&&":
                return LogicalConstant.LSymbol.AND;
            case "NOT":
            case "!":
                return LogicalConstant.LSymbol.NOT;
            case "IMPLIES":
            case "=>":
                return LogicalConstant.LSymbol.IMPLIES;
            case "TRUE":
                return LogicalConstant.LSymbol.TRUE;
            case "FALSE":
                return LogicalConstant.LSymbol.FALSE;
            case "FORALL":
                return LogicalConstant.LSymbol.FORALL;
            case "EXISTS":    
                return LogicalConstant.LSymbol.EXISTS;
                
            default:
                throw new Exception($"Unknown Logic Symbol: {lexValue}");
        }
    }
}