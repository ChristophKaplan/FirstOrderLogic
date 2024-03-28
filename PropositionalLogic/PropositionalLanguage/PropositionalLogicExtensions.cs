using LRParser.Language;

namespace PropositionalLogic;

public static class PropositionalLogicExtensions {
    
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

    public static InterpretationSet Mod(this PropositionalLogic logic, Sentence sentence) {
        var interpretations = logic.GenerateInterpretations(sentence);
        var models = new List<Interpretation>();

        foreach (var interpretation in interpretations) {
            var mod = interpretation.Evaluate(sentence);
            if (mod) { models.Add(interpretation); }
        }

        return new InterpretationSet(models, sentence);
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

    public static Sentence Simplify(this PropositionalLogic logic, Sentence sentence) {
        var old = sentence;
        var copy = sentence.GetCopy();
        var changed = true;

        while (changed) {
            SimplifyTruthValues(ref copy);
            changed = !old.Equals(copy);
            old = copy.GetCopy();
        }

        Absorption(ref copy);
        Absorption2(ref copy);
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
            case LogicalConstant.LSymbol.AND when _.truthValueSide.Tautology:
                Replace(ref sentence, _.otherSide);
                break;
            case LogicalConstant.LSymbol.AND when _.truthValueSide.Falsum:
                Replace(ref sentence, _.truthValueSide);
                break;
            case LogicalConstant.LSymbol.OR when _.truthValueSide.Tautology:
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
        if (!sentence.IsAtomComplexRelation(out var atomicSentence, out var complex)) {
            return;
        }

        if (complex.Children.Contains(atomicSentence) &&
            ((sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.OR } && complex.Operator == LogicalConstant.LSymbol.AND) ||
             (sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.AND } && complex.Operator == LogicalConstant.LSymbol.OR))) {
            atomicSentence.Reparent(sentence);
            sentence = atomicSentence;
        }
    }
    
    private static void Absorption2(ref Sentence sentence) {
        if (!sentence.IsAtomComplexRelation(out var atomicSentence, out var complex)) {
            return;
        }

        //(A AND (B AND A)) = (B AND A)
        //(A OR (B OR A)) = (B OR A)
        
        if (complex.Children.Contains(atomicSentence) &&
            ((sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.AND } && complex.Operator == LogicalConstant.LSymbol.AND) ||
             (sentence is ComplexSentence { Operator: LogicalConstant.LSymbol.OR } && complex.Operator == LogicalConstant.LSymbol.OR))) {
            complex.Reparent(sentence);
            sentence = complex;
        }
    }
}