using LRParser.Language;
using Helpers;

namespace FirstOrderLogic;

public static class FirstOrderLogicExtensions
{
    public static Connective.LogicSymbol ToLogicalConstant(this LexValue lexValue) {
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

    private delegate void TransformationDelegate(ref ISentence sentence);
    public static ISentence Simplify(this FirstOrderLogic logic, ISentence sentence, out List<ISentence> steps) {
        steps = new List<ISentence>();
        var clone = sentence.Clone();
        
        var transformations = new List<TransformationDelegate> {
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.SimplifyConstants, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveBiconditional, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveImplication, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.PushNegation, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DoubleNegation, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.Absorption, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.AssociationAndIdem, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.PullQuantifier, ref s),
            (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.RemoveDuplicateQuantifier, ref s),
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

        Logger.Log("simplification done.");
        return clone;
    }
    
    public static ISentence SkolemForm(this FirstOrderLogic logic, ISentence sentence) {
        var clone = sentence.Clone();
        
        //1. is PNF ?
        var complexSentence = (IComplexSentence) clone;
        var  universalQantifiers = complexSentence.GetQuantifiers(Connective.LogicSymbol.UNIVERSAL);
        var  existentialQuantifiers = complexSentence.GetQuantifiers(Connective.LogicSymbol.EXISTENTIAL);
       
        Dictionary<Variable, Function> substitution = new();

        foreach (var exist in existentialQuantifiers) {
            var args = new List<Term>();
            foreach (var universal in universalQantifiers) {
                args.Add(universal.Variable);
            }

            //TODO: semantics of skolem function ??
            var skolemFunction = new Function("sk", args.ToArray());
            substitution.Add(exist.Variable, skolemFunction);
        }
        
        foreach (var var in substitution.Keys) {
            clone.SubstituteTerm(var, substitution[var]);
        }
        
        //remove quantifiers
        TransformationFOL.Transform(TransformationFOL.EquivType.RemoveQuantifier, ref clone);
        
        return clone;
    }
    
    public static List<Clause> GetClauseSet(this ISentence sentence, List<Clause> clauseSet = null) {
        if (!sentence.IsCNF()) { throw new Exception("Sentence is not in CNF"); }
        
        clauseSet ??= new List<Clause>();
        var clone = sentence.Clone(); 
        
        if(clone.IsDisjunctionOfLiterals())
        {
            var clauseList = clone.GetLiterals();
            clauseSet.Add(new Clause(clauseList.ToArray()));
            return clauseSet;
        }
        
        foreach (var child in sentence.Children)
        {
            child.GetClauseSet(clauseSet);
        }
        
        return clauseSet;
    }
}