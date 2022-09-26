using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class Distributivitaet : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   F && (G || H) 
            //   F || (G && H)
            //   

            ComplexSentence d1 = null;
            ComplexSentence d2 = null;
            ComplexSentence d3 = null;
            if ((p.IsComplex() && v.GetOperator().AsConnective().IsOpposite(p.AsComplex().GetOperator().AsConnective()))) {
                Sentence F = p.AsComplex().GetP();
                Sentence G = p.AsComplex().GetQ();
                Sentence H = q;

                d1 = new ComplexSentence(H, F, v.GetOperator().AsConnective());
                d2 = new ComplexSentence(H, G, v.GetOperator().AsConnective());
                d3 = new ComplexSentence(d1, d2, ((ComplexSentence)p).GetOperator().AsConnective());
                return d3; //FIX
            }

            if ((q.IsComplex() && v.GetOperator().AsConnective().IsOpposite(q.AsComplex().GetOperator().AsConnective()))) {
                Sentence F = p;
                Sentence G = q.AsComplex().GetP();
                Sentence H = q.AsComplex().GetQ();

                d1 = new ComplexSentence(F, G, v.GetOperator().AsConnective());
                d2 = new ComplexSentence(F, H, v.GetOperator().AsConnective());
                d3 = new ComplexSentence(d1, d2, ((ComplexSentence)q).GetOperator().AsConnective());
            }

            return d3;
        }
        public override bool IsPossible(Sentence f) {
            if (!f.IsConnective() || f.IsLiteral()) return false;
            ComplexSentence v = f.AsComplex();

            Sentence p = v.GetP();
            Sentence q = v.GetQ();

            //   F && (G || H) 
            //   F || (G && H) 

            if (p.IsComplex() && v.GetOperator().AsConnective().IsOpposite(p.AsComplex().GetOperator().AsConnective())) {
                Sentence F = p.AsComplex().GetP();
                Sentence G = p.AsComplex().GetQ();
                Sentence H = q;
                if (!F.Equals(G) && !F.Equals(H) && !G.Equals(H)) return true;
            }

            if (q.IsComplex() && v.GetOperator().AsConnective().IsOpposite(q.AsComplex().GetOperator().AsConnective())) {
                Sentence F = p;
                Sentence G = q.AsComplex().GetP();
                Sentence H = q.AsComplex().GetQ();
                if (!F.Equals(G) && !F.Equals(H) && !G.Equals(H)) return true;
            }

            return false;
        }
    }



}