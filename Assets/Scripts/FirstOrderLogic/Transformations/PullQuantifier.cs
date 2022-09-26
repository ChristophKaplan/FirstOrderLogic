using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public class PullQuantifier : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            if (p.IsQuantifier()) return E1(p.AsComplex(), q, v.GetOperator().AsConnective());
            if (q.IsQuantifier()) return E1(q.AsComplex(), p, v.GetOperator().AsConnective());
            return null;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsConnective() || f.IsLiteral()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            return ((p.IsQuantifier() && CheckIfVariableIsFree(p.AsComplex(), q)) || (q.IsQuantifier() && CheckIfVariableIsFree(q.AsComplex(), p)));
        }

        private bool CheckIfVariableIsFree(ComplexSentence p, Sentence q) {
            Quantifier localQuantifier = p.GetOperator().AsQuantifier();
            if (!q.IsVariableFree(localQuantifier.GetVariable())) return true;
            return false;
        }

        private Sentence E1(ComplexSentence p, Sentence q, Connective c) {
            Quantifier localQ = p.GetOperator().AsQuantifier();
            ComplexSentence v = new ComplexSentence(p.GetP(), q, c);
            return new ComplexSentence(v, localQ);
        }
    }
}