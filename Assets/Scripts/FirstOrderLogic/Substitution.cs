using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {



    public class Substitution {
        private Dictionary<VariableTerm, Term> mapping = new Dictionary<VariableTerm, Term>();
        public Substitution() {

        }
        public Substitution(VariableTerm from, Term to) {
            Add(from, to);
        }

        public override string ToString() {
            string s = "(";
            foreach (VariableTerm var in mapping.Keys) {
                s += "[" + var + "/" + mapping[var] + "], ";
            }
            s += ")";
            return s;
        }

        public void Add(VariableTerm domain, Term mapping) {
            this.mapping.Add(domain, mapping);
        }

        public bool DomainContains(VariableTerm term) {
            return mapping.ContainsKey(term);
        }

        public Term Substitute(Term t) {
            if (t is VariableTerm) {
                VariableTerm v = (VariableTerm)t;
                if (mapping.ContainsKey(v)) return mapping[v];
                //Debug.Log("variable not in domain: " + v);
                //Debug.Log(GetSubstitution());
                return null;
            }
            if (t is FunctionTerm) {
                FunctionTerm f = (FunctionTerm)t;
                Term[] replaced = new Term[f.GetArguments().Length];
                for (int i = 0; i < f.GetArguments().Length; i++) {
                    Term mapping = Substitute(f.GetArguments()[i]);
                    if (mapping == null) {
                        mapping = f.GetArguments()[i]; //stay same
                    }
                    replaced[i] = mapping;
                }
                return new FunctionTerm((FunctionSymbol)f.GetSymbol(), replaced);
            }
            throw new System.Exception("unknown term type");
        }

        public void SubstituteFormular(Sentence sentence) {
            List<AtomicSentence> leafs = sentence.GetLeafs();
            for (int i = 0; i < leafs.Count; i++) {
                AtomicSentence atom = leafs[i];
                Term[] terms = new Term[atom.GetTerms().Length];

                for (int j = 0; j < atom.GetTerms().Length; j++) {
                    Term sub = Substitute(atom.GetTerms()[j]);
                    if (sub == null) sub = atom.GetTerms()[j];
                    //Debug.Log(atom.GetTerms()[j].ToString() + " -> " + sub.ToString());
                    terms[j] = sub;
                }
                atom.SetTerms(terms);
            }

            if (sentence.IsQuantifier()) {
                //this is to replace Quantifier variables with other variables, but not functions/constants
                ComplexSentence q = sentence.AsComplex();
                VariableSymbol qvs = q.GetOperator().AsQuantifier().GetVariable();
                VariableTerm qv = new VariableTerm(qvs);

                if (!mapping.ContainsKey(qv)) {
                    return;
                }
                Term t = mapping[qv];

                VariableTerm v = (VariableTerm)t;
                VariableSymbol vs = (VariableSymbol)v.GetSymbol();
                q.GetOperator().AsQuantifier().SetVariable(vs);
            }
        }
    }

    public class Unificator {
        private List<Substitution> substitutions = new List<Substitution>();
        public List<Substitution> GetSubstitutions() => this.substitutions;

        public Unificator(Clause c1, Clause c2) {
            if (IsUnifyable(c1, c2)) this.substitutions = GetSubstitutions(c1, c2);
            else Debug.LogError("not unifyable!");
        }
        public Unificator(Sentence l1, Sentence l2) {
            if (IsUnifyable(l1, l2)) this.substitutions = GetSubstitutions(l1, l2);
            else Debug.LogError("not unifyable!");
        }

        private bool IsUnifyable(Term t1, Term t2) {
            if ((t1 is VariableTerm) || (t2 is VariableTerm)) {
                if (t1 is VariableTerm) {
                    VariableTerm vt1 = (VariableTerm)t1;
                    VariableSymbol vs1 = (VariableSymbol)vt1.GetSymbol();
                    if (!t2.IsVariableInTerm(vs1)) {
                        return true;
                    }
                }
                if (t2 is VariableTerm) {
                    VariableTerm vt2 = (VariableTerm)t2;
                    VariableSymbol vs2 = (VariableSymbol)vt2.GetSymbol();
                    if (!t1.IsVariableInTerm(vs2)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsUnifyable(Sentence l1, Sentence l2) {
            if (!l1.IsLiteral() || !l2.IsLiteral()) {
                Debug.Log("must be literals");
                return false;
            }
            if (l1.GetArity() != l2.GetArity()) return false;

            List<Term> terms1 = l1.GetAllTerms();
            List<Term> terms2 = l2.GetAllTerms();

            for (int i = 0; i < terms1.Count; i++) {
                if (!terms1[i].Equals(terms2[i])) {
                    if (!IsUnifyable(terms1[i], terms2[i])) return false;
                }
            }
            return true;
        }

        public bool IsUnifyable(Clause c1, Clause c2) {
            for (int i = 0; i < c1.GetLiterals().Count; i++) {
                for (int j = 0; j < c2.GetLiterals().Count; j++) {
                    if (!IsUnifyable(c1.GetLiterals()[i], c2.GetLiterals()[j])) return false;
                }
            }
            return true;
        }

        private Substitution GetSubstitution(Term t1, Term t2) {
            if (t1 is VariableTerm) {
                return new Substitution((VariableTerm)t1, t2);
            }
            if (t2 is VariableTerm) {
                return new Substitution((VariableTerm)t2, t1);
            }
            throw new System.Exception("unification error!");
        }
        public List<Substitution> GetSubstitutions(Sentence l1, Sentence l2) {
            List<Term> terms1 = l1.GetAllTerms();
            List<Term> terms2 = l2.GetAllTerms();
            List<Substitution> subs = new List<Substitution>();
            for (int i = 0; i < terms1.Count; i++) {
                if (IsUnifyable(terms1[i], terms2[i])) {
                    Substitution s = GetSubstitution(terms1[i], terms2[i]);
                    subs.Add(s);
                }
            }
            return subs;
        }
        public List<Substitution> GetSubstitutions(Clause c1, Clause c2) {
            List<Substitution> subs = new List<Substitution>();
            for (int i = 0; i < c1.GetLiterals().Count; i++) {
                for (int j = 0; j < c2.GetLiterals().Count; j++) {
                    if (IsUnifyable(c1.GetLiterals()[i], c2.GetLiterals()[j])) {
                        List<Substitution> s = GetSubstitutions(c1.GetLiterals()[i], c2.GetLiterals()[j]);
                        subs.AddRange(s);
                    }
                }
            }
            return subs;
        }

        public void Unify(Sentence sentence) {
            for (int i = 0; i < substitutions.Count; i++) {
                substitutions[i].SubstituteFormular(sentence);
            }

        }

        public override string ToString() {
            string s = "Unificator: ";
            for (int i = 0; i < substitutions.Count; i++) {
                s += substitutions[i] + ", ";
            }
            return s;
        }
    }
}
