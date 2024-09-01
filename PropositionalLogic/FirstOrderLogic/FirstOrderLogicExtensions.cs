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
            case "IFF":
            case "<=>":
                return Connective.LogicSymbol.BICONDITIONAL;
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

    private delegate void TransformationDelegate(ref Sentence sentence);
    public static Sentence Simplify(this FirstOrderLogic logic, Sentence sentence, out List<Sentence> steps) {
        steps = new List<Sentence>();
        var clone = sentence.Clone();
        
        var transformations = new List<TransformationDelegate> {
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.SimplifyConstants, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveBiconditional, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveImplication, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.PushNegation, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DoubleNegation, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.Absorption, ref s),
            (ref Sentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.AssociationAndIdem, ref s)
        };

        while (true) {
            var start = clone.Clone();
            foreach (var transform in transformations) {
                transform(ref clone);
                steps.Add(clone.Clone());
            }
            if (start.Equals(clone)) {
                break;
            }
        }

        Console.WriteLine("simplification done.");
        return clone;
    }
}