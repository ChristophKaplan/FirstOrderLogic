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
        
        while (changed) {
            SimplifyTruthValues(ref copy);
            changed = !old.Equals(copy);
            if(changed) steps.Add(old);
            old = copy.GetCopy();
        }
        
        steps.Add(copy.GetCopy());
        DissolveImplication(ref copy);
        steps.Add(copy.GetCopy());
        PushNegation(ref copy);
        steps.Add(copy.GetCopy());
        DoubleNegation(ref copy);
        steps.Add(copy.GetCopy());
        Absorption(ref copy);
        steps.Add(copy.GetCopy());
        Absorption_Ish(ref copy);
        
        //temp
        changed = true;
        while (changed) {
            SimplifyTruthValues(ref copy);
            changed = !old.Equals(copy);
            if(changed) steps.Add(old);
            old = copy.GetCopy();
        }
        Console.WriteLine("done here");
        return copy;
    }

    private static void SimplifyTruthValues(ref Sentence sentence) {
        if (sentence is AtomicSentence) return;
        if (sentence is ComplexSentence { IsNegation: true } c) {
            if (c.Children[0] is AtomicSentence {IsTruthValue: true } truthValue) {
                truthValue.FlipTruthValue();
                Replace(ref sentence, truthValue);
            }
            return;
        }
        
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

        (AtomicSentence truthValueSide, Sentence otherSide) mapping = MapLhsRhs();

        switch (((ComplexSentence)sentence).Operator) {
            case LogicalConstant.LSymbol.AND when mapping.truthValueSide.Verum:
                Replace(ref sentence, mapping.otherSide);
                break;
            case LogicalConstant.LSymbol.AND when mapping.truthValueSide.Falsum:
                Replace(ref sentence, mapping.truthValueSide);
                break;
            case LogicalConstant.LSymbol.OR when mapping.truthValueSide.Verum:
                Replace(ref sentence, mapping.truthValueSide);
                break;
            case LogicalConstant.LSymbol.OR when mapping.truthValueSide.Falsum:
                Replace(ref sentence, mapping.otherSide);
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

    private static void SimplifyTruthValues_BETTER(ref Sentence sentence) {
        if (sentence is not ComplexSentence complexSentence) {
            return;
        }
        
        if (complexSentence.IsLiteral) {
            if (complexSentence.Children[0] is AtomicSentence {IsTruthValue: true } truthValue) {
                truthValue.FlipTruthValue();
                truthValue.Reparent(sentence);
                sentence = truthValue;
            }
            return;
        }
        
        for (var i = 0; i < sentence.Children.Count; i++) {
            var childSentence = sentence.Children[i];
            SimplifyTruthValues(ref childSentence);
        }
        
        foreach (var child in sentence.Children) {
            if (child is not AtomicSentence { IsTruthValue: true } atomicSentence) {
                continue;
            }
            
            switch (complexSentence.Operator) {
                case LogicalConstant.LSymbol.AND when atomicSentence.Verum:
                case LogicalConstant.LSymbol.OR when atomicSentence.Falsum:
                    var otherSide = complexSentence.GetOtherSide(atomicSentence);
                    otherSide.Reparent(sentence);
                    sentence = otherSide;
                    break;
                case LogicalConstant.LSymbol.AND when atomicSentence.Falsum:
                case LogicalConstant.LSymbol.OR when atomicSentence.Verum:
                    atomicSentence.Reparent(sentence);
                    sentence = atomicSentence;
                    break;
                default:
                    throw new Exception(complexSentence.ToString() +complexSentence.Operator);
            }
            break;
        }
    }
    
    private static void DissolveImplication(ref Sentence sentence) {
        if (sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.IMPLIES } implication) {
            var lhs = implication.Children[0];
            var rhs = implication.Children[1];
            var notLhs = new ComplexSentence(LogicalConstant.LSymbol.NOT, lhs);
            var or = new ComplexSentence(notLhs, LogicalConstant.LSymbol.OR, rhs);
            or.Reparent(sentence);
            sentence = or;
        }
        
        for (var i = 0; i < sentence.Children.Count; i++) {
            var c = sentence.Children[i];
            DissolveImplication(ref c);
        }
    }
    
    private static void PushNegation(ref Sentence sentence) {
        if (sentence is ComplexSentence { IsNegation: true } negatedSentence) {
            
            if (negatedSentence.Children[0] is AtomicSentence {IsTruthValue: true } truthValue) {
                truthValue.FlipTruthValue();
                truthValue.Reparent(sentence);
                sentence = truthValue;
                return;
            }
            
            if (negatedSentence.Children[0] is ComplexSentence { IsNegation: false } inner) {
                inner.FlipOperator(); //deMorgan
                var p = new ComplexSentence(LogicalConstant.LSymbol.NOT,inner.Children[0]);
                var q = new ComplexSentence(LogicalConstant.LSymbol.NOT,inner.Children[1]);
                var pq = new ComplexSentence(p, inner.Operator, q);
                pq.Reparent(sentence);
                sentence = pq;
            }
        }
        
        for (var i = 0; i < sentence.Children.Count; i++) {
            var c = sentence.Children[i];
            PushNegation(ref c);
        }
    }

    private static void DoubleNegation(ref Sentence sentence) {
        if (sentence is ComplexSentence { IsNegation: true } negation) {
            if (negation.Children[0] is ComplexSentence { IsNegation: true } doubleNegation) {

                int i = negation.Parent.Children.IndexOf(negation);
                negation.Parent.Children[i] = doubleNegation.Children[0];
            }
        }
        
        for (var i = 0; i < sentence.Children.Count; i++) {
            var c = sentence.Children[i];
            DoubleNegation(ref c);
        }
    }

    private static void Absorption(ref Sentence sentence) {
        if(sentence is AtomicSentence || sentence is ComplexSentence { IsNegation: true }) return;
        
        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsDualOperator(complex.Operator, rhsComplex.Operator) && rhsComplex.Children.Contains(lhs)) {
            lhs.Reparent(sentence);
            sentence = lhs;
        }
               
       if (lhs is ComplexSentence lhsComplex && IsDualOperator(complex.Operator, lhsComplex.Operator) && lhsComplex.Children.Contains(rhs)) { 
           rhs.Reparent(sentence);
           sentence = rhs;
       }

       bool IsDualOperator(LogicalConstant.LSymbol o1, LogicalConstant.LSymbol o2) {
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
//A AND A)
//A OR A)
        //(A AND (B AND A)) = (B AND A)
        //(A OR (B OR A)) = (B OR A)
        
        if(sentence is AtomicSentence || sentence is ComplexSentence { IsNegation: true }) return;
        
        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsEquivOperator(complex.Operator, rhsComplex.Operator) && rhsComplex.Children.Contains(lhs)) {
            rhs.Reparent(sentence);
            sentence = rhs;
        }
               
        if (lhs is ComplexSentence lhsComplex && IsEquivOperator(complex.Operator, lhsComplex.Operator) && lhsComplex.Children.Contains(rhs)) { 
            lhs.Reparent(sentence);
            sentence = lhs;
        }

        bool IsEquivOperator(LogicalConstant.LSymbol o1, LogicalConstant.LSymbol o2) {
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