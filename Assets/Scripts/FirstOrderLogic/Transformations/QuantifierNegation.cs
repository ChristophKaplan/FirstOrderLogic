using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public class QuantifierNegation : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            ComplexSentence q = v.GetP().AsComplex();

            ComplexSentence v2 = new ComplexSentence(q.GetP(), v.GetOperator().AsConnective());
            ComplexSentence q2 = new ComplexSentence(v2, q.GetOperator().AsQuantifier().GetAsOpposite());
            return q2;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();
            return v.IsNegation() && v.GetP().IsQuantifier();
        }
    }
}