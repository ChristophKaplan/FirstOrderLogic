using FirstOrderLogic;

public static class TransformationFOL {
    public enum EquivType { SimplifyConstants, DissolveImplication, PushNegation, DoubleNegation, Absorption, AssociationAndIdem, DissolveBiconditional }

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
            case EquivType.DissolveBiconditional:
                BottomUpTransformation(ref sentence, DissolveBiconditional);
                break;
            case EquivType.DissolveImplication:
                BottomUpTransformation(ref sentence, DissolveImplication);
                break;
            case EquivType.PushNegation:
                BottomUpTransformation(ref sentence, PushNegation);
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
        //add these to the other transformations
        //We take out Tautology and Contradiction
        if (sentence is not ComplexSentence complexSentence) {
            return;
        }
        
        if (complexSentence.IsLiteral) {
            if (complexSentence.Children[0] is AtomicSentence { IsConstant: true } constant) {
                constant.NegateNullary(); //push negation
                constant.SetParentToParentOf(sentence);
                sentence = constant;
            }

            return;
        }

        if (complexSentence._operator.Symbol != Connective.LogicSymbol.CONJUNCTION || 
            complexSentence._operator.Symbol != Connective.LogicSymbol.DISJUNCTION) {
            return;
        }
        
        foreach (var child in sentence.Children) {
            if (child is not AtomicSentence { IsConstant: true } atomicSentence) {
                continue;
            }

            switch (complexSentence._operator.Symbol) {
                case Connective.LogicSymbol.CONJUNCTION when atomicSentence.Tautology:
                case Connective.LogicSymbol.DISJUNCTION when atomicSentence.Contradiction:
                    var otherSide = complexSentence.GetOtherSide(atomicSentence);
                    otherSide.SetParentToParentOf(sentence);
                    sentence = otherSide;
                    break;
                case Connective.LogicSymbol.CONJUNCTION when atomicSentence.Contradiction:
                case Connective.LogicSymbol.DISJUNCTION when atomicSentence.Tautology:
                    atomicSentence.SetParentToParentOf(sentence);
                    sentence = atomicSentence;
                    break;
                default:
                    throw new Exception($"{complexSentence}, {complexSentence._operator}");
            }
        }
    }

    private static void DissolveBiconditional(ref Sentence sentence) {
        if (sentence is ComplexSentence { _operator.Symbol: Connective.LogicSymbol.BICONDITIONAL } implication) {
            var lhs = implication.Children[0];
            var rhs = implication.Children[1];
            var lhsImplication = new ComplexSentence(lhs, Connective.LogicSymbol.IMPLICATION, rhs);
            var rhsImplication = new ComplexSentence(rhs, Connective.LogicSymbol.IMPLICATION, lhs);
            var and = new ComplexSentence(lhsImplication, Connective.LogicSymbol.CONJUNCTION, rhsImplication);
            and.SetParentToParentOf(sentence);
            sentence = and;
        }
    }
    
    private static void DissolveImplication(ref Sentence sentence) {
        if (sentence is ComplexSentence { _operator.Symbol: Connective.LogicSymbol.IMPLICATION } implication) {
            var lhs = implication.Children[0];
            var rhs = implication.Children[1];
            var notLhs = new ComplexSentence(Connective.LogicSymbol.NEGATION, lhs);
            var or = new ComplexSentence(notLhs, Connective.LogicSymbol.DISJUNCTION, rhs);
            or.SetParentToParentOf(sentence);
            sentence = or;
        }
    }

    private static void PushNegation(ref Sentence sentence) {
        if (sentence is not ComplexSentence { IsNegation: true } negatedSentence) {
            return;
        }

        //negate quantifier
        if (negatedSentence.Children[0] is ComplexSentence { IsNegation: false, IsQuantifier: true } quantified) {
            quantified.FlipOperator();
            quantified.Children[0].Negate();
            quantified.SetParentToParentOf(sentence);
            sentence = quantified;
        }

        //deMorgan
        else if (negatedSentence.Children[0] is ComplexSentence { IsNegation: false, IsBinary: true } inner) {
            inner.FlipOperator();
            inner.Children[0].Negate();
            inner.Children[1].Negate();
            inner.SetParentToParentOf(sentence);
            sentence = inner;
        }
    }

    public static void DoubleNegation(ref Sentence sentence) {
        if (sentence is ComplexSentence { IsNegation: true } negation) {
            if (negation.Children[0] is ComplexSentence { IsNegation: true } doubleNegation) {
                var i = negation.Parent.Children.IndexOf(negation);
                negation.Parent.Children[i] = doubleNegation.Children[0];
            }
        }
    }

    private static void Absorption(ref Sentence sentence) {
        if (!sentence.IsBinary) return;

        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsDualOperator(complex._operator, rhsComplex._operator) && rhsComplex.Children.Contains(lhs)) {
            lhs.SetParentToParentOf(sentence);
            sentence = lhs;
        }

        if (lhs is ComplexSentence lhsComplex && IsDualOperator(complex._operator, lhsComplex._operator) && lhsComplex.Children.Contains(rhs)) {
            rhs.SetParentToParentOf(sentence);
            sentence = rhs;
        }

        bool IsDualOperator(Connective.LogicSymbol o1, Connective.LogicSymbol o2) {
            switch (o1) {
                case Connective.LogicSymbol.CONJUNCTION when o2 == Connective.LogicSymbol.DISJUNCTION:
                case Connective.LogicSymbol.DISJUNCTION when o2 == Connective.LogicSymbol.CONJUNCTION:
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

        if (!sentence.IsBinary) return;

        var complex = sentence as ComplexSentence;
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if (rhs is ComplexSentence rhsComplex && IsEquivOperator(complex._operator, rhsComplex._operator) && rhsComplex.Children.Contains(lhs)) {
            rhs.SetParentToParentOf(sentence);
            sentence = rhs;
        }

        if (lhs is ComplexSentence lhsComplex && IsEquivOperator(complex._operator, lhsComplex._operator) && lhsComplex.Children.Contains(rhs)) {
            lhs.SetParentToParentOf(sentence);
            sentence = lhs;
        }

        bool IsEquivOperator(Connective.LogicSymbol o1, Connective.LogicSymbol o2) {
            switch (o1) {
                case Connective.LogicSymbol.CONJUNCTION when o2 == Connective.LogicSymbol.CONJUNCTION:
                case Connective.LogicSymbol.DISJUNCTION when o2 == Connective.LogicSymbol.DISJUNCTION:
                    return true;
                default:
                    return false;
            }
        }
    }
}