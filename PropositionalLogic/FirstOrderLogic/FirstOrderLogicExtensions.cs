using LRParser.Language;

namespace FirstOrderLogic;

public static class FirstOrderLogicExtensions
{
    public static Connective ToLogicalConstant(this LexValue lexValue) {
        switch (lexValue.Value) {
            case "OR":
            case "||":
                return Connective.LogicSymbol.DISJUNCTION;
            case "AND":
            case "&&":
                return Connective.LogicSymbol.CONJUNCTION;
            case "NOT":
            case "!":
                return Connective.LogicSymbol.NEGATION;
            case "IMPLIES":
            case "=>":
                return Connective.LogicSymbol.IMPLICATION;
            case "TRUE":
                return Connective.LogicSymbol.TRUE;
            case "FALSE":
                return Connective.LogicSymbol.FALSE;
            case "FORALL":
                return Connective.LogicSymbol.UNIVERSAL;
            case "EXISTS":    
                return Connective.LogicSymbol.EXISTENTIAL;
                
            default:
                throw new Exception($"Unknown Logic Symbol: {lexValue}");
        }
    }
    
    public static Sentence Simplify(this FirstOrderLogic logic, Sentence sentence, out List<Sentence> steps) {
        var clone = sentence.Clone();
        steps = new List<Sentence>();
        
        
        TransformationFOL.Transform(TransformationFOL.EquivType.SimplifyConstants, ref clone);
        steps.Add(clone.Clone());
        TransformationFOL.Transform(TransformationFOL.EquivType.DissolveImplication,ref clone);
        steps.Add(clone.Clone());
        TransformationFOL.Transform(TransformationFOL.EquivType.PushNegation, ref clone);
        steps.Add(clone.Clone());
        TransformationFOL.Transform(TransformationFOL.EquivType.DoubleNegation, ref clone);
        steps.Add(clone.Clone());
        TransformationFOL.Transform(TransformationFOL.EquivType.Absorption, ref clone);
        steps.Add(clone.Clone());
        TransformationFOL.Transform(TransformationFOL.EquivType.AssociationAndIdem, ref clone);
        
        Console.WriteLine("simplification done.");
        return clone;
    }
}