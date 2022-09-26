using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {
    public class DoubleNegation : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            ComplexSentence inside = v.GetP().AsComplex();
            return inside.GetP();
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            if (v.GetP().IsComplex()) {
                if (v.IsNegation() && v.GetP().AsComplex().IsNegation()) {
                    return true;
                }
            }
            return false;
        }
    }

}