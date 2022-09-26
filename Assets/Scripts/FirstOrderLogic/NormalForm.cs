using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {
    public class NormalForm : TransformationBase {


        public bool IsNNF(Sentence sentence) {
            //no implication or bicondition, only literals

            if (sentence.IsLiteral()) return true;

            Queue<ComplexSentence> stack = new Queue<ComplexSentence>();
            stack.Enqueue(sentence.AsComplex());

            while (stack.Count > 0) {
                ComplexSentence cur = stack.Dequeue();
                if (cur.IsImplication() || cur.IsBiconditional() || (cur.IsNegation() && !cur.IsLiteral())) return false;
                for (int i = 0; i < cur.GetChildren().Length; i++) {
                    if (cur.GetChildren()[i].IsComplex()) stack.Enqueue(cur.GetChildren()[i].AsComplex());
                }
            }
            return true;
        }

        public bool IsCNF(Sentence s) {
            if (s.IsLiteral()) return true;
            Queue<ComplexSentence> stack = new Queue<ComplexSentence>();
            stack.Enqueue(s.AsComplex());

            while (stack.Count > 0) {
                ComplexSentence cur = stack.Dequeue();

                if (cur.IsImplication() || cur.IsBiconditional()) return false;
                if (cur.IsDisjunction()) {
                    if (!cur.IsDisjunctionOfLiteralsOrLiteral()) return false;
                }

                for (int i = 0; i < cur.GetChildren().Length; i++) {
                    if (cur.GetChildren()[i].IsComplex()) stack.Enqueue(cur.GetChildren()[i].AsComplex());
                }
            }
            return true;
        }

        public bool IsPNF(Sentence sentence) {
            if (sentence.GetLowerQuantifiers().Count <= 0) return true;
            if (!sentence.IsQuantifier()) return false;

            Queue<Sentence> stack = new Queue<Sentence>();
            stack.Enqueue(sentence);
            bool passedQuantifierSection = false;

            while (stack.Count > 0) {
                Sentence current = stack.Dequeue();
                if (current.IsAtom()) continue;

                for (int i = 0; i < current.AsComplex().GetChildren().Length; i++) {
                    Sentence child = current.AsComplex().GetChildren()[i];

                    if (!child.IsQuantifier()) passedQuantifierSection = true;
                    else if (child.IsQuantifier() && passedQuantifierSection) return false;

                    stack.Enqueue(child);
                }

            }
            return true;
        }



        public Sentence GetPrenexNNF(Sentence sentence) {
            Sentence copy = sentence.GetCopy();

            List<TransformationRule> first = new List<TransformationRule>() {
            new CleanedNormalform(),
        };

            List<TransformationRule> second = new List<TransformationRule>() {
            new Coimplikation(),
            new Implication(),
            //new CleanedNormalform(),
        };
            List<TransformationRule> third = new List<TransformationRule>() {
            new QuantifierNegation(),
            new DeMorganscheRegel(),
            new DoubleNegation(),
        };
            List<TransformationRule> fourth = new List<TransformationRule>() {
            new PullQuantifier(),
            new PullQuantifier_2(),
        };

            //1. clean
            ApplySetOfRulesUntilCantApplyAnymore(copy, first);
            //2. imp & coimp -> clean 
            ApplySetOfRulesUntilCantApplyAnymore(copy, second);
            //3. negation to the mid
            ApplySetOfRulesUntilCantApplyAnymore(copy, third);
            //4. quantifier out
            ApplySetOfRulesUntilCantApplyAnymore(copy, fourth);

            return copy;
        }


        public Sentence GetCNF(Sentence sentence) {
            Sentence copy = sentence.GetCopy();
            if (!IsPNF(copy)) copy = GetPrenexNNF(copy);

            //if there is no quantifiers PNF not detected

            while (!IsCNF(copy)) {
                PushDisjunctionUntilCNF(copy);
            }            
            //Debug.Log("CNF?:" + IsCNF(copy) + " -> " + copy.ToString());
            return copy;
        }

        private void PushDisjunctionUntilCNF(Sentence sentence) {
            if (sentence.IsAtom()) return;
            if (IsCNF(sentence)) return;
            
            if (sentence.AsComplex().IsDisjunction() && Transformation.distributivitaet.IsPossible(sentence)) {
                Sentence e = Transformation.distributivitaet.GetEquivalent(sentence);
                sentence.ReplaceWithEquivalent(e);

                PushDisjunctionUntilCNF(sentence);
            } else {
                //check for children
                for (int i = 0; i < sentence.AsComplex().GetChildren().Length; i++) {
                    PushDisjunctionUntilCNF(sentence.AsComplex().GetChildren()[i]);
                }
            }
        }


        public ClauseSet GetClauseSet(Sentence sentence) {
            if (!IsCNF(sentence)) {                
                sentence = GetCNF(GetPrenexNNF(sentence));
                if (!IsCNF(sentence)) throw new System.Exception("sentence is not in conjunctive normal form!");
            }

            List<Sentence> cons = sentence.GetConjunctedSentences();
            List<Clause> clauseList = new List<Clause>();

            if (sentence.IsDisjunctionOfLiteralsOrLiteral()) {
                Sentence[] split = SplitDisjunctions(sentence);
                Clause clause = new Clause(split);
                clauseList.Add(clause);
            } else {
                foreach (Sentence con in cons) {
                    Debug.Log("con: " + con.ToString());
                    Sentence[] split = SplitDisjunctions(con);
                    Clause clause = new Clause(split);
                    clauseList.Add(clause);
                }
            }

            ClauseSet clauseSet = new ClauseSet(clauseList);
            return clauseSet;
        }

        private Sentence[] SplitDisjunctions(Sentence con) {
            List<Sentence> literals = new List<Sentence>();
            Queue<Sentence> stack = new Queue<Sentence>();
            stack.Enqueue(con);
            while (stack.Count > 0) {
                Sentence cur = stack.Dequeue();
                if (cur.IsLiteral()) literals.Add(cur);
                if (!cur.IsLiteral()) {
                    for (int i = 0; i < cur.AsComplex().GetChildren().Length; i++) {
                        Sentence child = cur.AsComplex().GetChildren()[i];
                        stack.Enqueue(child);
                    }
                }

            }
            return literals.ToArray();
        }



        public Sentence Skolemization(Sentence sentence, Interpretation interpretation, VariableAssignment variablenbelegung, bool takeOutUniversalQuantifiers = true) {
            Sentence copy = sentence.GetCopy();
            if (!IsPNF(copy)) {
                throw new System.Exception(copy.ToString() + " input is not in prenex form!");
            }

            Dictionary<Term, Universe.Element> map = interpretation.GetAssigmentMap(copy, variablenbelegung);
            Dictionary<Symbol, List<Symbol>> dependenceOf = GetQuantifierScopeDependencies(copy);

            //other existential quantifiers need to be constants!

            foreach (Symbol quantifierVar in dependenceOf.Keys) {
                List<Symbol> symbols = dependenceOf[quantifierVar];
                if (symbols.Count == 0) {
                    Debug.Log("indy:" + quantifierVar);
                    continue;
                }

                (Universe.Element[], Universe.Element) fromto = GetFromToOfAssigmentMap(symbols, (VariableSymbol)quantifierVar, map);
                Substitution sub = BuildFunctionAndGetSubstitution("sf", symbols, (VariableSymbol)quantifierVar, fromto.Item1, fromto.Item2, interpretation);

                TakeOutQuantifier(copy, quantifierVar);
                sub.SubstituteFormular(copy);
            }

            if (takeOutUniversalQuantifiers) {
               copy = RemovePrenexQuantifiers(copy);
            }

            return copy;
        }


    }


}