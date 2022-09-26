using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {
    public class Implication : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            ComplexSentence equiv = new ComplexSentence(v.GetP().GetNegation(), v.GetQ(), OperatorType.disjunction);
            return equiv;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            return f.AsComplex().IsImplication();
        }
    }

}