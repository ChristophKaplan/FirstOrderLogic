using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {
    public class PullQuantifier_2 : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();

            ComplexSentence p = v.GetP().AsComplex();
            ComplexSentence q = v.GetQ().AsComplex();
            ComplexSentence v2 = new ComplexSentence(p.GetP(), q.GetP(), v.GetOperator().AsConnective());
            return new ComplexSentence(v2, p.GetOperator().AsQuantifier());
        }

        public override bool IsPossible(Sentence f) {
            if (!f.IsConnective() || f.IsLiteral()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            if (p.IsQuantifier() && q.IsQuantifier()) {
                Quantifier pq = p.AsComplex().GetOperator().AsQuantifier();
                Quantifier qq = q.AsComplex().GetOperator().AsQuantifier();
                if (pq.Equals(qq)) return true;
            }
            return false;
        }
    }
}