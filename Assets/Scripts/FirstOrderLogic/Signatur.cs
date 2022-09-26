using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class Signature {
        private List<FunctionSymbol> FunctionSymbols;
        private List<PredicateSymbol> PredicateSymbols;
        public Signature(List<FunctionSymbol> Func, List<PredicateSymbol> Pred) {
            this.FunctionSymbols = Func;
            this.PredicateSymbols = Pred;
        }

        public List<FunctionSymbol> GetFunctionSymbols() {
            return this.FunctionSymbols;
        }
        public List<PredicateSymbol> GetPredicateSymbols() {
            return this.PredicateSymbols;
        }

        public PredicateSymbol GetPredicate(string name) {
            for (int i = 0; i < PredicateSymbols.Count; i++) {
                if (PredicateSymbols[i].GetName().Equals(name)) return PredicateSymbols[i];
            }
            Debug.LogError("Predicate symbol not found: " + name);
            return null;
        }
        public FunctionSymbol GetFunctionSymbol(string name) {
            for (int i = 0; i < FunctionSymbols.Count; i++) {
                if (FunctionSymbols[i].GetName().Equals(name)) return FunctionSymbols[i];
            }
            Debug.LogError("Function symbol not found: " + name);
            return null;
        }

        public void AddPredicateSymbol(PredicateSymbol ps) {
            PredicateSymbols.Add(ps);
        }
        public void AddFunctionSymbol(FunctionSymbol fs) {
            FunctionSymbols.Add(fs);
        }




        public Sentence TryParse(string stringSentence, bool checkSignature = true) {
            OldTokenizer oldTokenizer = new OldTokenizer();

            if (!oldTokenizer.BracketCheck(stringSentence)) throw new System.Exception("brackets not right!");
            
            Sentence sentence = oldTokenizer.FormularScan(stringSentence, 0);

            if (checkSignature && SignatureCheck(sentence)) return sentence;
            else if (checkSignature && !SignatureCheck(sentence)) throw new System.Exception("missing signature!");

            return sentence;
        }
        private bool SignatureCheck(Sentence sentence) {

            List<AtomicSentence> atoms = sentence.GetLeafs();
            for (int i = 0; i < atoms.Count; i++) {
                PredicateSymbol ps = this.GetPredicate(atoms[i].GetPredicate().ToString());
                if (ps == null) {
                    Debug.Log("ps is null! " + atoms[i].GetPredicate().ToString());
                    return false;
                }

                for (int j = 0; j < atoms[i].GetTerms().Length; j++) {
                    Term t = atoms[i].GetTerms()[j];
                    if (t is FunctionTerm) {
                        FunctionSymbol functionSymbol = (FunctionSymbol)((FunctionTerm)t).GetSymbol();
                        FunctionSymbol fs = this.GetFunctionSymbol(functionSymbol.ToString());
                        if (fs == null) {
                            Debug.Log("fs is null! " + functionSymbol.ToString());
                            return false;
                        }
                    }
                }
            }

            return true;
        }


    }


}