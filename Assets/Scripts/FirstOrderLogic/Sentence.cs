
using System.Collections;
using System.Collections.Generic;
using Helpers;
using UnityEngine;



namespace FirstOrderLogic {

    public abstract class Sentence : IEqualityComparer<Sentence> {

        private int level;
        private ComplexSentence parent;

        public abstract override string ToString();

        public abstract int GetArity();
        public abstract Sentence GetCopy();

        public ComplexSentence GetNegation() => new ComplexSentence(this, OperatorType.negation);

        public override bool Equals(object obj) {
            Sentence other = (Sentence)obj;
            if (this.ToString().Equals(other.ToString())) return true;
            return false;
        }
        public override int GetHashCode() => ToString().GetHashCode();
        public bool Equals(Sentence x, Sentence y) => x.Equals(y);
        public int GetHashCode(Sentence obj) => GetHashCode();

        public int GetLevel() => this.level;
        public void SetLevel(int lvl) {
            this.level = lvl;
            if (IsComplex() && AsComplex().GetChildren().Length > 0) {
                for (int i = 0; i < AsComplex().GetChildren().Length; i++) AsComplex().GetChildren()[i].SetLevel(GetLevel() + 1);
            }

        }
        public Sentence GetRoot() {
            if (GetParent() == null) return this;
            return GetParent().GetRoot();
        }

        public ComplexSentence GetParent() => this.parent;
        public void SetParent(ComplexSentence parent) {
            this.parent = parent;
        }

        public int GetDeepesLevel() {
            if (IsAtom()) return GetLevel();
            int max = GetLevel();
            for (int i = 0; i < AsComplex().GetChildren().Length; i++) {
                int cur = AsComplex().GetChildren()[i].GetDeepesLevel();
                if (cur > max) max = cur;
            }
            return max;
        }

        public bool IsRoot() => GetParent() == null;
        public bool IsComplex() => this is ComplexSentence;
        public bool IsAtom() => this is AtomicSentence;
        public ComplexSentence AsComplex() => (ComplexSentence)this;
        public AtomicSentence AsAtom() => (AtomicSentence)this;

        public virtual bool IsConnective() => IsComplex() && AsComplex().GetOperator().IsConnective();
        public virtual bool IsNegation() => IsComplex() && AsComplex().GetOperator().IsNegation();
        public virtual bool IsQuantifier() => IsComplex() && AsComplex().GetOperator().IsQuantifier();
        public bool IsLiteral() => ((IsAtom() || IsComplex() && (AsComplex().IsNegation() || AsComplex().IsAffirmation())));

        public bool IsDisjunctionOfLiteralsOrLiteral() {
            if (IsLiteral()) return true;
            if (IsComplex() && AsComplex().IsDisjunction()) {
                return AsComplex().GetP().IsDisjunctionOfLiteralsOrLiteral() && AsComplex().GetQ().IsDisjunctionOfLiteralsOrLiteral();
            }
            return false;
        }

        public bool IsVariableInTerms(VariableSymbol var) {
            Term[] terms = GetAllTerms().ToArray();
            for (int i = 0; i < terms.Length; i++) if (terms[i].IsVariableInTerm(var)) return true;
            return false;
        }
        public bool IsVariableFree(VariableSymbol var) => !IsVariableBound(var) && IsVariableInTerms(var);

        public bool IsVariableBound(VariableSymbol var) {
            if (!IsVariableInTerms(var)) return false;
            List<Quantifier> upperQUantifiers = GetUpperQuantifierOperators();
            for (int i = 0; i < upperQUantifiers.Count; i++) {
                VariableSymbol quantifierVariable = upperQUantifiers[i].GetVariable();
                if (quantifierVariable.Equals(var)) return true;
            }
            return false;
        }

        public void ReplaceWithEquivalent(Sentence sentence) {
            //want to change this

            /*    example replace this with an atom: (¬ (¬ C(x))) with: C(x)
             *    musst replace the attributes of "this"-reference,
             *    we cant change the class type, so we use an affirmation as an atomic sentence
             */

            sentence.SetParent(this.GetParent());
            sentence.SetLevel(GetLevel());

            ComplexSentence newOther = null;
            ComplexSentence me = this.AsComplex();

            if (sentence.IsAtom()) newOther = new ComplexSentence(sentence, OperatorType.affirmation);
            else newOther = sentence.AsComplex();
            me.SetChildren(newOther.GetChildren());
            me.SetOperator(newOther.GetOperator());
        }

        public ComplexSentence GetLowestQuantifier() {
            if (this.IsAtom()) return null;
            for (int i = 0; i < AsComplex().GetChildren().Length; i++) {
                Sentence child = AsComplex().GetChildren()[i];
                ComplexSentence lowestQuantifier = child.GetLowestQuantifier();
                if (this.IsQuantifier()) if (lowestQuantifier == null) return this.AsComplex();
                return lowestQuantifier;
            }
            throw new System.Exception("GetLowestQuantifier() - no quantifier?");
        }

        public List<ComplexSentence> GetLowerQuantifiers() {
            List<ComplexSentence> collected = new List<ComplexSentence>();
            Queue<ComplexSentence> stack = new Queue<ComplexSentence>();
            stack.Enqueue(AsComplex());
            while (stack.Count > 0) {
                ComplexSentence current = stack.Dequeue();
                if (current.IsQuantifier()) collected.Add(current);
                for (int i = 0; i < current.GetChildren().Length; i++) {
                    Sentence child = current.GetChildren()[i];
                    if (child.IsComplex()) stack.Enqueue(child.AsComplex());
                }
            }
            return collected;
        }

        public List<Quantifier> GetUpperQuantifierOperators() {
            List<Quantifier> collected;
            if (this.GetLevel() == 0) collected = new List<Quantifier>();
            else collected = GetParent().GetUpperQuantifierOperators();
            if (this is ComplexSentence) collected.Add(((ComplexSentence)this).GetOperator().AsQuantifier());
            return collected;
        }

        public List<Sentence> GetAllOnLevel(int level) {
            List<Sentence> collected = new List<Sentence>();
            if (GetLevel() == level) collected.Add(this);
            if (!IsAtom()) {
                for (int i = 0; i < AsComplex().GetChildren().Length; i++) {
                    List<Sentence> collectedChild = AsComplex().GetChildren()[i].GetAllOnLevel(level);
                    for (int j = 0; j < collectedChild.Count; j++) collected.Add(collectedChild[j]);
                }
            }
            return collected;
        }

        public List<Term> GetAllTerms() {
            List<AtomicSentence> leafs = GetLeafs();
            List<Term> collected = new List<Term>();
            for (int i = 0; i < leafs.Count; i++) {
                AtomicSentence a = leafs[i];
                for (int j = 0; j < a.GetTerms().Length; j++) collected.Add(a.GetTerms()[j]);
            }
            return collected;
        }

        public List<AtomicSentence> GetLeafs() {
            List<AtomicSentence> collected = new List<AtomicSentence>();
            if (this.IsAtom()) collected.Add(AsAtom());
            else for (int i = 0; i < AsComplex().GetChildren().Length; i++) collected.AddRange(AsComplex().GetChildren()[i].GetLeafs());
            return collected;
        }
        public List<Sentence> GetLiterals() {
            List<Sentence> collected = new List<Sentence>();
            if (this.IsLiteral()) collected.Add(this);
            else for (int i = 0; i < AsComplex().GetChildren().Length; i++) collected.AddRange(AsComplex().GetChildren()[i].GetLeafs());
            return collected;
        }

        public List<Sentence> GetConjunctedSentences() {
            List<AtomicSentence> leafs = GetLeafs();
            List<Sentence> collected = new List<Sentence>();

            foreach (AtomicSentence leaf in leafs) {
                Sentence cur = leaf;
                ComplexSentence parent = cur.GetParent();

                while (parent != null && (cur.GetLevel() >= this.GetLevel())) {
                    if (parent.IsConnective() && parent.IsConjunction()) {
                        if (!collected.Contains(cur)) collected.Add(cur);
                        parent = null;
                    } else {
                        cur = parent;
                        parent = cur.GetParent();
                    }
                }
            }
            return collected;
        }

        public void PrintSyntaxTree() {
            StringTree stringTree = new StringTree();
            Debug.Log(stringTree.GetSyntaxTree(this));
        }

    }





}