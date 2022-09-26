using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {
    public class Assoziativitaet : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   (F && G) && H
            //   F && (G && H) 

            if ((p is ComplexSentence) && v.GetOperator().AsConnective().Equals(((ComplexSentence)p).GetOperator().AsConnective())) {
                Sentence F = ((ComplexSentence)p).GetP();
                Sentence G = ((ComplexSentence)p).GetQ();
                Sentence H = q;
                return new ComplexSentence(F, new ComplexSentence(G, H, v.GetOperator().AsConnective()), v.GetOperator().AsConnective());
            }

            if ((q is ComplexSentence) && v.GetOperator().AsConnective().Equals(((ComplexSentence)q).GetOperator().AsConnective())) {
                Sentence F = p;
                Sentence G = ((ComplexSentence)q).GetP();
                Sentence H = ((ComplexSentence)q).GetQ();
                return new ComplexSentence(new ComplexSentence(F, G, v.GetOperator().AsConnective()), H, v.GetOperator().AsConnective());
            }

            return null;
        }

        public override bool IsPossible(Sentence f) {
            if (!f.IsComplex()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   (F && G) && H
            //   F && (G && H) 

            if ((p is ComplexSentence) && v.GetOperator().AsConnective().Equals(((ComplexSentence)p).GetOperator().AsConnective())) {
                Sentence F = ((ComplexSentence)p).GetP();
                Sentence G = ((ComplexSentence)p).GetQ();
                Sentence H = q;
                if (!F.Equals(G) && !F.Equals(H) && !G.Equals(H)) return true;
            }

            if ((q is ComplexSentence) && v.GetOperator().AsConnective().Equals(((ComplexSentence)q).GetOperator().AsConnective())) {
                Sentence F = p;
                Sentence G = ((ComplexSentence)q).GetP();
                Sentence H = ((ComplexSentence)q).GetQ();
                if (!F.Equals(G) && !F.Equals(H) && !G.Equals(H)) return true;
            }

            return false;
        }
    }
}