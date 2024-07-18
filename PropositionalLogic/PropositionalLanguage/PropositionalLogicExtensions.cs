using LRParser.Language;

namespace PropositionalLogic;

public static class PropositionalLogicExtensions {
    
    public static InterpretationSet SwitchAll(this PropositionalLogic logic, InterpretationSet set, AtomicSentence variable) {
        var list = new List<Interpretation>();
        foreach (var interpretation in set.Interpretations) {
            var s = interpretation.Switch(interpretation, variable);
            if (s != null)list.Add(s);
        }
        return new InterpretationSet(list, set.Sentences.ToArray());
    }
    
    public static List<Sentence> UnfoldEquivalence(this PropositionalLogic logic, Sentence sentence, bool completeSteps = false) {
        var simplified = logic.Simplify(sentence, out var steps);
        
        var transformationSteps = new List<Sentence> { sentence };
        if(completeSteps) transformationSteps.AddRange(steps);
        transformationSteps.Add(simplified);
        transformationSteps = transformationSteps.Distinct().ToList();
        
        return transformationSteps;
    }
    
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
            default:
                throw new Exception($"Unknown Logic Symbol: {lexValue}");
        }
    }

    public static InterpretationSet Int(this PropositionalLogic logic,List<AtomicSentence> signature, params Sentence[] sentences)
    {
        var showChildren = false;
        List<Sentence> list = new();
        foreach (var sen in sentences)
        {
            list.Add(sen);
            list.AddRange(sen.GetComplexChildren());
        }

        signature ??= logic.GenerateSignature(sentences);
        return new InterpretationSet(logic.GenerateInterpretations(signature), showChildren ? list.ToArray() : sentences);
    }

    public static InterpretationSet Mod(this PropositionalLogic logic,List<AtomicSentence> signature, Sentence sentence) {
       var i = Int(logic, signature, sentence);
        return new InterpretationSet(i.Models(sentence), sentence);
    }
    
    public static InterpretationSet SwMod(this PropositionalLogic logic,List<AtomicSentence> signature, Sentence sentence, AtomicSentence switchMe) {
        var intSet = Int(logic, signature, sentence);
        var switched = SwitchAll(logic, intSet, switchMe);
        var list = new List<Interpretation>();
        for (int i = 0; i < intSet.Interpretations.Count; i++) {
            if (switched.Interpretations[i].IsModel(sentence)) {
                list.Add(intSet.Interpretations[i]);
            }
        }
        
        return new InterpretationSet(list, sentence);
    }
    
    public static InterpretationSet Intersection(this PropositionalLogic logic,InterpretationSet a, InterpretationSet b) {
        return new InterpretationSet( a.Interpretations.Intersect(b.Interpretations).ToList(), a.Sentences.Intersect(b.Sentences).ToArray());
    }
    
    public static Sentence Forget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        
        var lhs = Substitute(logic, sentence, forgetMe, new AtomicSentence(LogicalConstant.LSymbol.TRUE.ToString()));
        var rhs = Substitute(logic, sentence, forgetMe, new AtomicSentence(LogicalConstant.LSymbol.FALSE.ToString()));
        var n = new ComplexSentence(lhs, LogicalConstant.LSymbol.OR, rhs);
        return n;
    }
    
    public static Sentence SkepForget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = Substitute(logic, sentence, forgetMe, new AtomicSentence(LogicalConstant.LSymbol.TRUE.ToString()));
        var rhs = Substitute(logic, sentence, forgetMe, new AtomicSentence(LogicalConstant.LSymbol.FALSE.ToString()));
        var n = new ComplexSentence(lhs, LogicalConstant.LSymbol.AND, rhs);
        return n;
    }
    
    public static Sentence Substitute(this PropositionalLogic logic, Sentence sentence, AtomicSentence subMe, AtomicSentence subWith) {
        if(sentence.Equals(subMe) && subMe.Parent == null) {
            return subWith;
        }
        
        var copy = sentence.GetCopy();
        copy.SubstituteAtom(subMe, subWith);
        return copy;
    }
    
    public static Sentence Simplify(this PropositionalLogic logic, Sentence sentence, out List<Sentence> steps) {
        var old = sentence;
        var copy = sentence.GetCopy();
        
        var changed = true;
        steps = new List<Sentence>();
        
        /*while (changed) {
            changed = !old.Equals(copy);
            if(changed) steps.Add(old);
            old = copy.GetCopy();
            Console.WriteLine("step: " + old);
        }*/
        
        Transformation.Transform(Transformation.EquivType.SimplifyConstants, ref copy);
        steps.Add(copy.GetCopy());
        Transformation.Transform(Transformation.EquivType.DissolveImplication,ref copy);
        steps.Add(copy.GetCopy());
        Transformation.Transform(Transformation.EquivType.DeMorgan, ref copy);
        steps.Add(copy.GetCopy());
        Transformation.Transform(Transformation.EquivType.DoubleNegation, ref copy);
        steps.Add(copy.GetCopy());
        Transformation.Transform(Transformation.EquivType.Absorption, ref copy);
        steps.Add(copy.GetCopy());
        Transformation.Transform(Transformation.EquivType.AssociationAndIdem, ref copy);

        Console.WriteLine("simplify done");
        return copy;
    }
}