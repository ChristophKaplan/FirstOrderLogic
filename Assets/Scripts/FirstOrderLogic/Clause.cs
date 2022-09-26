using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public class ClauseSet {
        private List<Clause> clauses;
        public List<Clause> GetClauses() => this.clauses;
        public ClauseSet(List<Clause> clauses) {
            this.clauses = clauses;
        }

        public static ClauseSet Join(params ClauseSet[] sets) {
            List<Clause> joined = new List<Clause>();
            for (int i = 0; i < sets.Length; i++) {
                joined.AddRange(sets[i].GetClauses());
            }
            return new ClauseSet(joined);
        }

        public override string ToString() {
            string s = "{";
            for (int i = 0; i < clauses.Count; i++) {
                s += clauses[i].ToString();
                s += ", ";
            }
            s += "}";
            return s;
        }

        public bool IsSubsetOfClauses(List<Clause> other) {
            for (int i = 0; i < other.Count; i++) {
                for (int j = 0; j < clauses.Count; j++) {
                    if (!other.Equals(clauses[j])) return false;
                }
            }
            return true;
        }

        List<Resolvent> GetPossibleResolvents(Clause K1, Clause K2) {
            if (!IsResolvable(K1, K2)) {
                return null;
            }
            List<Resolvent> possibleResolvents = new List<Resolvent>();
            List<(Sentence, Sentence)> contradicting = new List<(Sentence, Sentence)>();

            //scan
            for (int i = 0; i < K1.GetLiterals().Count; i++) {
                for (int j = 0; j < K2.GetLiterals().Count; j++) {
                    if (LiteralsAreContradicting(K1.GetLiterals()[i], K2.GetLiterals()[j])) contradicting.Add(new(K1.GetLiterals()[i], K2.GetLiterals()[j]));
                }
            }

            for (int i = 0; i < contradicting.Count; i++) {
                Resolvent resolved = new Resolvent(K1, K2);
                for (int j = 0; j < K1.GetLiterals().Count; j++) if (!contradicting[i].Item1.Equals(K1.GetLiterals()[j])) resolved.AddLiteral(K1.GetLiterals()[j]);
                for (int j = 0; j < K2.GetLiterals().Count; j++) if (!contradicting[i].Item2.Equals(K2.GetLiterals()[j])) resolved.AddLiteral(K2.GetLiterals()[j]);
                possibleResolvents.Add(resolved);
            }

            return possibleResolvents;
        }
        bool IsResolvable(Clause K1, Clause K2) {
            for (int i = 0; i < K1.GetLiterals().Count; i++) {
                for (int j = 0; j < K2.GetLiterals().Count; j++) {
                    if (LiteralsAreContradicting(K1.GetLiterals()[i], K2.GetLiterals()[j])) return true;
                }
            }
            //Debug.Log("not resolveable: \n" + K1.ToString() + "\n" + K2.ToString());
            return false;
        }
        bool LiteralsAreContradicting(Sentence l1, Sentence l2) {

            PredicateSymbol p1 = null;
            PredicateSymbol p2 = null;
            ComplexSentence c1 = null;
            ComplexSentence c2 = null;

            if (l1.IsAtom()) {
                p1 = l1.AsAtom().GetPredicate();
                c1 = new ComplexSentence(l1, OperatorType.affirmation);
            }
            if (l1.IsComplex()) {
                p1 = l1.AsComplex().GetP().AsAtom().GetPredicate();
                c1 = l1.AsComplex();
            }

            if (l2.IsAtom()) {
                p2 = l2.AsAtom().GetPredicate();
                c2 = new ComplexSentence(l2, OperatorType.affirmation);
            }
            if (l2.IsComplex()) {
                p2 = l2.AsComplex().GetP().AsAtom().GetPredicate();
                c2 = l2.AsComplex();
            }

            if (p1.Equals(p2)) {
                if ((c1.IsAffirmation() && c2.IsNegation()) ||
                   (c1.IsNegation() && c2.IsAffirmation())) {
                    return true;
                }
            }
            return false;
        }



        public Resolvent Resolution() {
            List<Clause> neueResolventen = new List<Clause>();

            while (true) {
                for (int i = 0; i < clauses.Count; i++) {
                    for (int j = 0; j < clauses.Count; j++) {
                        if (!IsResolvable(clauses[i], clauses[j])) { continue; }
                        List<Resolvent> resolvents = GetPossibleResolvents(clauses[i], clauses[j]);
                        for (int k = 0; k < resolvents.Count; k++) {
                            if (resolvents[k].IsEmptyClause()) {
                                Debug.Log("success");
                                return resolvents[k];
                            }
                        }

                        neueResolventen.AddRange(resolvents);
                    }
                }

                if (IsSubsetOfClauses(neueResolventen)) {
                    Debug.Log("no success");
                    return null;
                }

                clauses.AddRange(neueResolventen);
            }

        }
    }


    public class Clause {
        private List<Sentence> literals;
        public Clause(params Sentence[] literals) {
            for (int i = 0; i < literals.Length; i++) {
                if (!literals[i].IsLiteral()) {
                    Debug.LogError("is not literal: " + literals[i]);
                    return;
                }
            }
            this.literals = new List<Sentence>(literals);
        }
        public List<Sentence> GetLiterals() {
            return this.literals;
        }
        public void AddLiteral(Sentence l) {
            if (literals == null) literals = new List<Sentence>();
            if (literals.Contains(l)) return;

            if (!l.IsLiteral()) {
                Debug.LogError("is not a literal");
                return;
            }
            literals.Add(l);
        }

        public override string ToString() {
            string s = "{";
            for (int j = 0; j < literals.Count; j++) {
                s += literals[j].ToString();
                if (j < literals.Count - 1) s += ", ";
            }
            s += "}";
            return s;
        }

        public override bool Equals(object obj) {
            Clause other = (Clause)obj;
            return other.ToString().Equals(this.ToString());
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
    }



    public class Resolvent : Clause {
        private Clause elternklausel1;
        private Clause elternklausel2;
        public Resolvent(Clause k1, Clause k2, params ComplexSentence[] literals) : base(literals) {
            this.elternklausel1 = k1;
            this.elternklausel2 = k2;
        }

        public string ResolventAsString() {
            string s = "";
            s += "k1: " + elternklausel1.ToString() + "\n";
            s += "k2: " + elternklausel2.ToString() + "\n";
            s += "-----------------------" + "\n";
            s += "res: " + this.ToString() + "\n";
            return s;
        }

        public bool IsEmptyClause() {
            if (this.GetLiterals() == null || this.GetLiterals().Count == 0) return true;
            return false;
        }

        public bool IsCircular(Clause k1, Clause k2) {
            if (this.elternklausel1.Equals(k1)) {
                if (this.elternklausel2.Equals(k2)) return true;
            }
            if ((this.elternklausel1 is Resolvent)) {
                return ((Resolvent)elternklausel1).IsCircular(elternklausel1, k2);
            }
            return false;
        }

        public string TraceResolution() {
            string s = "";

            if (elternklausel1 is Resolvent) {
                Resolvent parent1 = (Resolvent)elternklausel1;
                s += parent1.TraceResolution() + "\n";
            }
            if (elternklausel2 is Resolvent) {
                Resolvent parent2 = (Resolvent)elternklausel2;
                s += parent2.TraceResolution() + "\n";
            }

            if (this is Resolvent) {
                s += ResolventAsString() + "\n";
            } else {
                s += "literal:\n" + ToString() + "\n";
            }

            return s;
        }
    }

}
