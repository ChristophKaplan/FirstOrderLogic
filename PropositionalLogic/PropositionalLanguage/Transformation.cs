namespace PropositionalLogic;

public static class Transformation {
    public enum EquivType { SimplifyConstants, DissolveImplication, DeMorgan, DoubleNegation, Absorption, AssociationAndIdem }

    private delegate void TransformAction<Sentence>(ref Sentence sentence);

    private static void BottomUpTransformation(ref Sentence sentence, TransformAction<Sentence> transformAction) {
        for (var i = 0; i < sentence.Children.Count; i++) {
            var childSentence = sentence.Children[i];
            BottomUpTransformation(ref childSentence, transformAction);
        }

        transformAction(ref sentence);
    }

    private static void TopDownTransformation(ref Sentence sentence, TransformAction<Sentence> transformAction) {
        transformAction(ref sentence);

        for (var i = 0; i < sentence.Children.Count; i++) {
            var childSentence = sentence.Children[i];
            TopDownTransformation(ref childSentence, transformAction);
        }
    }

    public static void Transform(EquivType equivType, ref Sentence sentence) {
        switch (equivType) {
            case EquivType.SimplifyConstants:
                BottomUpTransformation(ref sentence, SimplifyConstants);
                break;
            case EquivType.DissolveImplication:
                BottomUpTransformation(ref sentence, DissolveImplication);
                break;
            case EquivType.DeMorgan:
                BottomUpTransformation(ref sentence, DeMorgan);
                break;
            case EquivType.DoubleNegation:
                BottomUpTransformation(ref sentence, DoubleNegation);
                break;
            case EquivType.Absorption:
                BottomUpTransformation(ref sentence, Absorption);
                break;
            case EquivType.AssociationAndIdem:
                BottomUpTransformation(ref sentence, AssociationAndIdem);
                break;
        }
    }

    private static void SimplifyConstants(ref Sentence sentence) {
        //We take out Tautology and Contradiction
        if (sentence is not ComplexSentence complexSentence) {
            return;
        }

        if (complexSentence.IsLiteral) {
            if (complexSentence.Children[0] is AtomicSentence { IsConstant: true } constant) {
                constant.FlipTruthValue(); //push negation
                constant.Reparent(sentence);
                sentence = constant;
            }

            return;
        }

        foreach (var child in sentence.Children) {
            if (child is not AtomicSentence { IsConstant: true } atomicSentence) {
                continue;
            }

            switch (complexSentence.Operator) {
                case LogicalConstant.LSymbol.AND when atomicSentence.Tautology:
                case LogicalConstant.LSymbol.OR when atomicSentence.Contradiction:
                    var otherSide = complexSentence.GetOtherSide(atomicSentence);
                    otherSide.Reparent(sentence);
                    sentence = otherSide;
                    break;
                case LogicalConstant.LSymbol.AND when atomicSentence.Contradiction:
                case LogicalConstant.LSymbol.OR when atomicSentence.Tautology:
                    atomicSentence.Reparent(sentence);
                    sentence = atomicSentence;
                    break;
                default:
                    throw new Exception(complexSentence.ToString() + complexSentence.Operator);
            }
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
    }

    private static void DeMorgan(ref Sentence sentence) {
        if (sentence is ComplexSentence { IsNegation: true } negatedSentence) {
            if (negatedSentence.Children[0] is ComplexSentence { IsNegation: false } inner) {
                inner.FlipOperator(); //deMorgan
                var p = new ComplexSentence(LogicalConstant.LSymbol.NOT, inner.Children[0]);
                var q = new ComplexSentence(LogicalConstant.LSymbol.NOT, inner.Children[1]);
                var pq = new ComplexSentence(p, inner.Operator, q);
                pq.Reparent(sentence);
                sentence = pq;
            }
        }
    }

    public static void DoubleNegation(ref Sentence sentence) {
        if (sentence is ComplexSentence { IsNegation: true } negation) {
            if (negation.Children[0] is ComplexSentence { IsNegation: true } doubleNegation) {
                int i = negation.Parent.Children.IndexOf(negation);
                negation.Parent.Children[i] = doubleNegation.Children[0];
            }
        }
    }

    private static void Absorption(ref Sentence sentence) {
        if (sentence is AtomicSentence || sentence is ComplexSentence { IsNegation: true }) return;

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

    private static void AssociationAndIdem(ref Sentence sentence) {
//A AND A)
//A OR A)
        //(A AND (B AND A)) = (B AND A)
        //(A OR (B OR A)) = (B OR A)

        if (sentence is AtomicSentence || sentence is ComplexSentence { IsNegation: true }) return;

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