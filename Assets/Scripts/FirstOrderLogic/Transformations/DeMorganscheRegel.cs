using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public class DeMorganscheRegel : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {

            ComplexSentence inside = f.AsComplex().GetP().AsComplex();

            Connective upsidedown = null;
            if (inside.IsDisjunction()) upsidedown = new Connective(OperatorType.conjunction);
            if (inside.IsConjunction()) upsidedown = new Connective(OperatorType.disjunction);

            ComplexSentence equiv = new ComplexSentence(inside.GetP().GetNegation(), inside.GetQ().GetNegation(), upsidedown);
            return equiv;
        }

        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            if (f.AsComplex().IsNegation()) {
                //!(F && G)
                //!(F || G)
                Sentence p = f.AsComplex().GetP();
                return p.IsComplex() && (p.AsComplex().IsConjunction() || p.AsComplex().IsDisjunction());
            }
            return false;
        }
    }

}