using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {


    public class Inference : TransformationBase {
        public Inference() { }

        //ModusPonens
        public bool IsModusPonensPossible(params Sentence[] premise) {
            if (premise[1].IsComplex() && premise[1].AsComplex().IsImplication()) {
                if (premise[0].Equals(premise[1].AsComplex().GetP())) return true;
            }
            return false;
        }
        public Sentence GetModusPonens(params Sentence[] premise) {
            return premise[1].AsComplex().GetQ();
        }


        //ModusTolles
        public bool IsModusTollensPossible(params Sentence[] premise) {
            if (premise[0].IsComplex() && premise[0].AsComplex().IsImplication() && premise[1].IsNegation()) {
                ComplexSentence neg = premise[1].AsComplex();
                if (premise[0].AsComplex().GetQ().Equals(neg.GetP())) return true;
            }
            return false;
        }
        public Sentence GetModusTollens(params Sentence[] premise) {
            return premise[0].AsComplex().GetP().GetNegation();
        }


        //AndInduction
        public Sentence GetAndInductionConclusion(params Sentence[] premise) {
            return new ComplexSentence(premise[0], premise[1], OperatorType.conjunction);
        }

        //And Elimination
        public bool IsAndEliminationPossible(params Sentence[] premise) {
            return (premise[0].IsComplex() && premise[0].AsComplex().IsConjunction());
        }
        public Sentence GetAndEliminationConclusion(params Sentence[] premise) {
            ComplexSentence comp = premise[0].AsComplex();
            Sentence p = comp.GetP();
            Sentence q = comp.GetQ();
            return p; //or q 
        }




        //Existential Instance
        public Sentence GetExistentialInstance(Sentence premise, Interpretation interpretation, VariableAssignment variableAssignment) {

            Sentence copy = premise.GetCopy();
            Quantifier existentialQuantifier = (Quantifier)copy.AsComplex().GetOperator();

            if (!existentialQuantifier.IsExistential()) throw new System.Exception("quantifier is not existential");

            VariableSymbol quantifierVariable = existentialQuantifier.GetVariable();
            Sentence inside = copy.AsComplex().GetP();

            Dictionary<Term, Universe.Element> map = GetAssignmentMapWhereSentenceIsTrue(copy, interpretation, variableAssignment);
            Universe.Element to = GetElementOfAssigmentMap(map, quantifierVariable);

            Substitution sub = BuildFunctionAndGetSubstitution("c",new List<Symbol>(), quantifierVariable,new Universe.Element[0], to, interpretation);

            TakeOutQuantifier(inside, quantifierVariable);
            sub.SubstituteFormular(inside);
            return inside;

        }

        public bool IsExistentialInstancePossible(params Sentence[] premise) {
            return (premise[0].IsQuantifier() && premise[0].AsComplex().GetOperator().IsExistential());
        }

        //Universal Instance
        public Sentence GetUniversalInstanceConclusion(Sentence premise, Universe.Element whereElement, Interpretation interpretation) {

            Quantifier uni = (Quantifier)premise.AsComplex().GetOperator();
            Sentence inside = premise.AsComplex().GetP();
            Substitution sub =  BuildFunctionAndGetSubstitution("uni", new List<Symbol>(), uni.GetVariable(), new Universe.Element[0], whereElement, interpretation);

            TakeOutQuantifier(inside, uni.GetVariable());
            sub.SubstituteFormular(inside);
            return inside;
        }

        public bool IsUniversalInstancePossible(params Sentence[] premise) {
            return (premise[0].IsQuantifier() && premise[0].AsComplex().GetOperator().IsUniversal());
        }

        //
        public bool ResolutionProof(Sentence premise1, Sentence premise2, Sentence conclusion) {
            NormalForm normalForm = new NormalForm();

            ClauseSet cs1 = normalForm.GetClauseSet(premise1);
            ClauseSet cs2 = normalForm.GetClauseSet(premise2);
            ClauseSet cs3 = normalForm.GetClauseSet(conclusion.GetNegation());

            ClauseSet c = ClauseSet.Join(cs1, cs2, cs3);
            Debug.Log(c);
            Resolvent r = c.Resolution();
            Debug.Log(r.TraceResolution());


            return (r != null);
        }

    }


}