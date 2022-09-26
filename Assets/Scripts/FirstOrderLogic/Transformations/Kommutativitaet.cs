using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public class Kommutativitaet : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            Sentence p = v.GetP();
            Sentence q = v.GetQ();
            return new ComplexSentence(q, p, v.GetOperator().AsConnective());
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();
            if (v.IsConjunction() || v.IsDisjunction()) return true;
            return false;
        }
    }

}