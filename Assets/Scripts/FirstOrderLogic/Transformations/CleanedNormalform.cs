using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {
    public class CleanedNormalform : TransformationRule {
        //there should not be free and bound variables with the same name -> substitute

        public override bool IsPossible(Sentence f) {
            if (FindDoubleQuantified(f).Item1 != null) return true;
            return false;
        }

        public override Sentence GetEquivalent(Sentence f) {
            (Sentence, Quantifier) tuple = FindDoubleQuantified(f);
            Sentence r = tuple.Item1;
            Quantifier qo = tuple.Item2;
            Rename(r, qo);
            return f;
        }

        private (Sentence, Quantifier) FindDoubleQuantified(Sentence f) {
            List<Quantifier> collected = new List<Quantifier>();
            Queue<Sentence> stack = new Queue<Sentence>();
            stack.Enqueue(f);

            while (stack.Count > 0) {
                Sentence current = stack.Dequeue();
                if (current.IsAtom()) continue;
                if (current.IsQuantifier()) {
                    Quantifier qo = current.AsComplex().GetOperator().AsQuantifier();
                    if (collected.Contains(qo)) return (current, qo);
                    collected.Add(qo);
                }
                for (int i = 0; i < current.AsComplex().GetChildren().Length; i++) stack.Enqueue(current.AsComplex().GetChildren()[i]);
            }
            return (null, null);
        }

        private void Rename(Sentence renameMe, Quantifier qo) {
            //choose a different var?
            Substitution sub = new Substitution(new VariableTerm(qo.GetVariable()), new VariableTerm("sub" + Random.Range(0, 1000)));
            sub.SubstituteFormular(renameMe);

        }

    }


}