using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class ComplexSentence : Sentence {
        protected Operator op;
        private Sentence[] children;

        public ComplexSentence(Sentence p, Operator op) {
            this.SetChildren(p);
            this.op = op;
        }
        public ComplexSentence(Sentence p, Sentence q, Operator op) {
            this.SetChildren(p, q);
            this.op = op;
        }
        public ComplexSentence(Sentence[] children, Operator op) {
            this.SetChildren(children);
            this.op = op;
        }

        //connective
        public ComplexSentence(Sentence p, Sentence q, OperatorType opType) {
            this.SetChildren(p, q);
            this.op = new Connective(opType);
        }
        public ComplexSentence(Sentence p, OperatorType opType) {
            this.SetChildren(p);
            this.op = new Connective(opType);
        }

        //quantifier
        public ComplexSentence(Sentence sentence, OperatorType opType, string v) {
            this.SetChildren(sentence);
            this.op = new Quantifier(opType, v);
        }


        public override int GetArity() => children.Length;

        public Sentence[] GetChildren() => this.children;
        public void SetChildren(params Sentence[] children) {
            this.children = children;
            for (int i = 0; i < this.children.Length; i++) {
                this.children[i].SetParent(this);
                this.children[i].SetLevel(GetLevel() + 1);
            }
        }
        public Operator GetOperator() {
            return this.op;
        }
        public void SetOperator(Operator op) {
            this.op = op;
        }

        public Sentence GetP() {
            return GetChildren()[0];
        }
        public Sentence GetQ() {
            if (GetChildren().Length <= 1) return null;
            return GetChildren()[1];
        }

        public override bool IsConnective() => GetOperator().IsConnective();
        public override bool IsQuantifier() => GetOperator().IsQuantifier();
        public bool IsAffirmation() => GetOperator().IsAffirmation();
        public bool IsConjunction() => GetOperator().IsConjunction();
        public bool IsDisjunction() => GetOperator().IsDisjunction();
        public bool IsImplication() => GetOperator().IsImplication();
        public bool IsBiconditional() => GetOperator().IsBiconditional();

        public bool Contains(Sentence f) => (GetP().Equals(f) || GetQ().Equals(f));
        
        public List<Term> GetTermsInQuantifierScope() {
            List<AtomicSentence> leafs = GetLeafs();
            List<Term> collected = new List<Term>();
            for (int i = 0; i < leafs.Count; i++) 
                if (leafs[i].IsVariableInTerms(GetOperator().AsQuantifier().GetVariable()))
                    collected.AddRange(leafs[i].GetTerms());                             
            return collected;
        }

        public override string ToString() {

            bool withBrackets = true;
            if (IsQuantifier() || IsNegation()) withBrackets = false;

            string s = "";
            if (withBrackets) s += "(";

            if (this.GetChildren().Length == 1) {
                s += op.ToString() + " " + GetP().ToString();
            } else {
                s += GetP().ToString() + " " + op.ToString() + " " + GetQ().ToString();
            }
            if (withBrackets) s += ")";

            return s;
        }
        public override Sentence GetCopy() {
            Sentence[] children = GetChildren();
            Sentence[] newChildren = new Sentence[children.Length];
            for (int i = 0; i < children.Length; i++) {
                newChildren[i] = children[i].GetCopy();
            }
            return new ComplexSentence(newChildren, GetOperator());
        }



    }


}