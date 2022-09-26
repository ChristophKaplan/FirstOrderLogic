using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public class Absorbtion : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   F && (F || G) 
            //   F || (F && G) 

            if ((q is ComplexSentence) && v.GetOperator().AsConnective().IsOpposite(((ComplexSentence)q).GetOperator().AsConnective())) {
                if (((ComplexSentence)q).Contains(p)) return p;
            }
            if ((p is ComplexSentence) && v.GetOperator().AsConnective().IsOpposite(((ComplexSentence)p).GetOperator().AsConnective())) {
                if (((ComplexSentence)p).Contains(q)) return q;
            }

            return null;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   F && (F || G) 
            //   F || (F && G) 

            if ((q is ComplexSentence) && v.GetOperator().AsConnective().IsOpposite(((ComplexSentence)q).GetOperator().AsConnective())) {
                if (((ComplexSentence)q).Contains(p)) return true;
            }
            if ((p is ComplexSentence) && v.GetOperator().AsConnective().IsOpposite(((ComplexSentence)p).GetOperator().AsConnective())) {
                if (((ComplexSentence)p).Contains(q)) return true;
            }
            return false;
        }
    }

}