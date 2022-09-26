using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public class Contraposition : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            ComplexSentence equiv = new ComplexSentence(v.GetQ().GetNegation(), v.GetP().GetNegation(), OperatorType.implication);
            return equiv;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            return v.GetOperator().AsConnective().IsImplication();
        }
    }

}