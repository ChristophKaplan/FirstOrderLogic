using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public class AtomicSentence : Sentence {

        private PredicateSymbol predicate;
        private Term[] terms;

        public override int GetArity() => terms.Length;
        public PredicateSymbol GetPredicate() => this.predicate;
        public Term[] GetTerms() => this.terms;

        public AtomicSentence(PredicateSymbol pred) {
            this.predicate = pred;
            if (this.predicate.GetArity() != 0) throw new System.Exception("arity not 0!");
            this.terms = new Term[0];
        }

        public AtomicSentence(PredicateSymbol pred, params Term[] terms) {
            this.predicate = pred;
            this.terms = terms;
        }
        public AtomicSentence(PredicateSymbol pred, params VariableSymbol[] vars) {
            this.predicate = pred;
            Term[] p = new Term[vars.Length];
            for (int i = 0; i < vars.Length; i++) {
                p[i] = new VariableTerm(vars[i]);
            }
            this.terms = p;
        }
        public AtomicSentence(PredicateSymbol pred, params string[] varsAsString) {
            this.predicate = pred;
            Term[] p = new Term[varsAsString.Length];
            for (int i = 0; i < varsAsString.Length; i++) {
                p[i] = new VariableTerm(new VariableSymbol(varsAsString[i]));
            }
            this.terms = p;
        }

        
        public void SetTerms(Term[] terms) {
            this.terms = terms;
        }

        public override string ToString() {
            string s = "";
            s += GetPredicate().GetName() + "(";
            for (int i = 0; i < terms.Length; i++) {
                if (i > 0) s += ", ";
                s += terms[i].ToString();
            }
            s += ")";
            return s;
        }

        public override Sentence GetCopy() {
            Term[] newTerms = new Term[this.terms.Length];
            for (int i = 0; i < terms.Length; i++) {
                newTerms[i] = terms[i].GetCopy();
            }
            return new AtomicSentence(this.predicate, newTerms);
        }
    }


}