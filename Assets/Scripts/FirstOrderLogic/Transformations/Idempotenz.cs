using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class Idempotenz : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            return f.AsComplex().GetP();
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();
            if (!v.IsConjunction() && !v.IsDisjunction()) return false;
            return p.Equals(q);
        }
    }

}