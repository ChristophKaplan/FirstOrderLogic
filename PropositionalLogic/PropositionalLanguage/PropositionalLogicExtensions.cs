using LRParser.Language;

namespace PropositionalLogic;

public static class PropositionalLogicExtensions {
    
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

    public static InterpretationSet SwitchMany(this PropositionalLogic logic, InterpretationSet set, AtomicSentence variable) {
        var list = new List<Interpretation>();
        foreach (var model in set.Interpretations) {
            list.Add(model.Switch(variable));
        }

        return new InterpretationSet(list, set.Sentences.ToArray());
    }

    public static InterpretationSet Int(this PropositionalLogic logic, params Sentence[] sentences)
    {
        var showChildren = false;
        List<Sentence> list = new();
        foreach (var sen in sentences)
        {
            list.Add(sen);
            list.AddRange(sen.GetComplexChildren());
        }
        
        return new InterpretationSet(logic.GenerateInterpretations(sentences), showChildren ? list.ToArray() : sentences);
    }

    public static InterpretationSet Mod(this PropositionalLogic logic, Sentence sentence) {
       var i = Int(logic, sentence);
        return new InterpretationSet(i.Models(sentence), sentence);
    }
    
    public static Sentence Forget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, LogicalConstant.LSymbol.TRUE.ToString());
        rhs.FindReplaceAtom(forgetMe, LogicalConstant.LSymbol.FALSE.ToString());
        var n = new ComplexSentence(lhs, LogicalConstant.LSymbol.OR, rhs);
        return n;
    }

    public static Sentence SkepForget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, LogicalConstant.LSymbol.TRUE.ToString());
        rhs.FindReplaceAtom(forgetMe, LogicalConstant.LSymbol.FALSE.ToString());
        var n = new ComplexSentence(lhs, LogicalConstant.LSymbol.AND, rhs);
        return n;
    }

    public static Sentence Simplify(this PropositionalLogic logic, Sentence sentence, out List<Sentence> steps) {
        var old = sentence;
        var copy = sentence.GetCopy();
        var changed = true;
        steps = new List<Sentence>();
        
        while (changed) {
            SimplifyTruthValues(ref copy);
            changed = !old.Equals(copy);
            if(changed) steps.Add(old);
            old = copy.GetCopy();
        }
        
        steps.Add(copy.GetCopy());
        Absorption(ref copy);
        steps.Add(copy.GetCopy());
        Absorption_Ish(ref copy);
        
        return copy;
    }

    private static void SimplifyTruthValues(ref Sentence sentence) {
        if (sentence is AtomicSentence) return;
        if(sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.NOT }) return;
        
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (!(lhs is AtomicSentence { IsTruthValue: true } || rhs is AtomicSentence { IsTruthValue: true })) {
            StepDown(ref sentence);
            return;
        }

        (AtomicSentence, Sentence) MapLhsRhs() {
            (AtomicSentence atomicTruthValue, Sentence other) result = (null, null);
            if (lhs is AtomicSentence { IsTruthValue: true } atomicLhs) {
                result = (atomicLhs, rhs);
            }

            if (rhs is AtomicSentence { IsTruthValue: true } atomicRhs) {
                result = (atomicRhs, lhs);
            }

            return result;
        }

        (AtomicSentence truthValueSide, Sentence otherSide) _ = MapLhsRhs();

        switch (((ComplexSentence)sentence).Operator) {
            case LogicalConstant.LSymbol.AND when _.truthValueSide.Verum:
                Replace(ref sentence, _.otherSide);
                break;
            case LogicalConstant.LSymbol.AND when _.truthValueSide.Falsum:
                Replace(ref sentence, _.truthValueSide);
                break;
            case LogicalConstant.LSymbol.OR when _.truthValueSide.Verum:
                Replace(ref sentence, _.truthValueSide);
                break;
            case LogicalConstant.LSymbol.OR when _.truthValueSide.Falsum:
                Replace(ref sentence, _.otherSide);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        void StepDown(ref Sentence sentence) {
            for (var i = 0; i < sentence.Children.Count; i++) {
                var childSentence = sentence.Children[i];
                SimplifyTruthValues(ref childSentence);
            }
        }

        void Replace(ref Sentence sentence, Sentence replaceWith) {
            replaceWith.Reparent(sentence);
            sentence = replaceWith;
        }

        StepDown(ref sentence);
    }

    private static void Absorption(ref Sentence sentence) {
        if(sentence is AtomicSentence) return;
        
        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsOpposite(complex.Operator, rhsComplex.Operator) && rhsComplex.Children.Contains(lhs)) {
            lhs.Reparent(sentence);
            sentence = lhs;
        }
               
       if (lhs is ComplexSentence lhsComplex && IsOpposite(complex.Operator, lhsComplex.Operator) && lhsComplex.Children.Contains(rhs)) { 
           rhs.Reparent(sentence);
           sentence = rhs;
       }

       bool IsOpposite(LogicalConstant.LSymbol o1, LogicalConstant.LSymbol o2) {
            switch (o1) {
                case LogicalConstant.LSymbol.AND when o2 == LogicalConstant.LSymbol.OR:
                case LogicalConstant.LSymbol.OR when o2 == LogicalConstant.LSymbol.AND:
                    return true;
                default:
                    return false;
            }
        }
    }
    
    private static void Absorption_Ish(ref Sentence sentence) {

        //(A AND (B AND A)) = (B AND A)
        //(A OR (B OR A)) = (B OR A)
        
        if(sentence is AtomicSentence) return;
        
        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsSame(complex.Operator, rhsComplex.Operator) && rhsComplex.Children.Contains(lhs)) {
            rhs.Reparent(sentence);
            sentence = rhs;
        }
               
        if (lhs is ComplexSentence lhsComplex && IsSame(complex.Operator, lhsComplex.Operator) && lhsComplex.Children.Contains(rhs)) { 
            lhs.Reparent(sentence);
            sentence = lhs;
        }

        bool IsSame(LogicalConstant.LSymbol o1, LogicalConstant.LSymbol o2) {
            switch (o1) {
                case LogicalConstant.LSymbol.AND when o2 == LogicalConstant.LSymbol.AND:
                case LogicalConstant.LSymbol.OR when o2 == LogicalConstant.LSymbol.OR:
                    return true;
                default:
                    return false;
            }
        }
    }
}