using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class Coimplikation : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            ComplexSentence imp1 = new ComplexSentence(v.GetP(), v.GetQ(), OperatorType.implication);
            ComplexSentence imp2 = new ComplexSentence(v.GetQ(), v.GetP(), OperatorType.implication);

            ComplexSentence equiv = new ComplexSentence(imp1, imp2, OperatorType.conjunction);
            return equiv;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            return f.AsComplex().IsBiconditional();
        }
    }

}