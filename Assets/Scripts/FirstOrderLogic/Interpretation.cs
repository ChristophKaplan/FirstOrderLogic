using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FirstOrderLogic {

    public class TruthValueUnderAssignment {
        private bool value;
        public bool GetValue() => this.value;

        private List<int[]> assignment = new List<int[]>();

        public List<int[]> GetAssignment() => this.assignment;

        public TruthValueUnderAssignment(bool value, params int[] args) {
            this.value = value;
            Add(args);
        }
        public TruthValueUnderAssignment(bool value, List<int[]> args) {
            this.value = value;
            Add(args);
        }
        public TruthValueUnderAssignment(bool value, List<int[]> args1, List<int[]> args2) {
            this.value = value;
            Add(args1);
            Add(args2);
        }
        public void Add(params int[] args) {
            assignment.Add(args);
        }
        public void Add(List<int[]> args) {
            for (int i = 0; i < args.Count; i++) assignment.Add(args[i]);
        }

        public override string ToString() {
            string s = this.value + " ( assignment: ";
            for (int i = 0; i < GetAssignment().Count; i++) {
                for (int j = 0; j < GetAssignment()[i].Length; j++) {
                    s += GetAssignment()[i][j];
                }
                s += ",";
            }
            return s + ")";
        }
    }

    public class Interpretation {
        private Structure structureRef;
        private Signature signaturRef;

        public Structure GetStructure() => this.structureRef;
        public Signature GetSignatur() => this.signaturRef;

        private Dictionary<PredicateSymbol, int> predicateMap = new Dictionary<PredicateSymbol, int>();
        private Dictionary<FunctionSymbol, int> functionMap = new Dictionary<FunctionSymbol, int>();

        public Dictionary<PredicateSymbol, int> GetPredicates() => this.predicateMap;
        public Dictionary<FunctionSymbol, int> GetFunctions() => this.functionMap;

        public int GetFunctionId(FunctionSymbol fs) => this.functionMap[fs];
        public int GetPredicateId(PredicateSymbol ps) => this.predicateMap[ps];

        public bool HasPredicateSymbol(PredicateSymbol ps) => predicateMap.ContainsKey(ps);

        public Interpretation(Structure structure, Signature signatur) {
            this.structureRef = structure;
            this.signaturRef = signatur;
        }

        public void AddToFunctionMap(string funcSymbolName, int id) {
            FunctionSymbol fs = signaturRef.GetFunctionSymbol(funcSymbolName);
            if (!functionMap.ContainsKey(fs)) functionMap.Add(fs, id);
        }
        public void ExtendAFunction(FunctionSymbol fs, int id) {
            functionMap.Add(fs, id);
        }
        public void AddToPredicateMap(string predSymbolName, int id) {
            PredicateSymbol ps = signaturRef.GetPredicate(predSymbolName);
            if (!predicateMap.ContainsKey(ps)) predicateMap.Add(ps, id);
        }


        public override bool Equals(object obj) {
            Interpretation other = (Interpretation)obj;

            //TEMP           
            return ToString().Equals(other.ToString());
        }

        public override int GetHashCode() => ToString().GetHashCode();
        

        public override string ToString() {
            string s = "Predicates\n";
            foreach (PredicateSymbol p in predicateMap.Keys) s += p + " -> id:" + predicateMap[p] + " pred:" + GetStructure().GetPredicateRelations()[predicateMap[p]].ToString(GetStructure().GetUniverse()) + "\n";
            s += "\nFunctions\n";
            foreach (FunctionSymbol f in functionMap.Keys) s += f + " -> id" + functionMap[f] + " func:" + GetStructure().GetFunctions()[functionMap[f]].ToString(GetStructure().GetUniverse()) + "\n";
            return s;
        }



        int[] Evaluate(Term[] parameter, List<(VariableSymbol, int)> quantifiedAssignment, VariableAssignment assigment) {
            int[] termauswertung = new int[parameter.Length];
            for (int i = 0; i < parameter.Length; i++) {
                termauswertung[i] = Evaluate(parameter[i], quantifiedAssignment, assigment);
            }
            return termauswertung;
        }

        private int Evaluate(Term t, List<(VariableSymbol, int)> quantifiedAssignment, VariableAssignment assignment) {
            if (t is VariableTerm) return Evaluation_Variable((VariableTerm)t, quantifiedAssignment, assignment);
            if (t is FunctionTerm) return Evaluation_Function((FunctionTerm)t, quantifiedAssignment, assignment);
            Debug.LogError("Unknown Term subtype");
            return -1;
        }

        private int Evaluation_Variable(VariableTerm t, List<(VariableSymbol, int)> quantifiedAssignment, VariableAssignment assignment) {
            VariableSymbol vs = (VariableSymbol)t.GetSymbol();


            //check free variable first
            int free = assignment.GetAssignmentFor(vs);
            if(free != -1) return free;


            //check bound variable first (?)
            for (int j = 0; j < quantifiedAssignment.Count; j++) {
                //beware, takes the first, if there is doubled assigned, the result is wrong
                if (vs.Equals(quantifiedAssignment[j].Item1)) {
                    return quantifiedAssignment[j].Item2;
                }
            }

            Debug.LogError("No Evaluation available ?");
            return -1;
        }

        private int Evaluation_Function(FunctionTerm t, List<(VariableSymbol, int)> quantifiedAssigment, VariableAssignment assigment) {
            int[] parameterForFunc = new int[t.GetArguments().Length];

            for (int i = 0; i < t.GetArguments().Length; i++) {
                int auswertung = Evaluate(t.GetArguments()[i], quantifiedAssigment, assigment);
                parameterForFunc[i] = auswertung;
            }
            FunctionSymbol fs = (FunctionSymbol)t.GetSymbol();
            FunctionBase f = GetStructure().GetFunctions()[GetFunctionId(fs)];

            int elem = f.Map(parameterForFunc);
            return elem;
        }


        public TruthValueUnderAssignment GetTruthValue(Sentence sentence, VariableAssignment assigment) {
            return GetTruthValue(sentence,new List<(VariableSymbol, int)>(), assigment);
        }



        private bool EvaluationMatchesPredicateRelation(PredicateSymbol ps, int[] evaluated) {
            if (!HasPredicateSymbol(ps)) {
                //Debug.Log("pred: " + ps.GetName() + " was not found!!");
                //trifft nicht zu
                return false;
            }

            int id = GetPredicateId(ps);
            PredicateRelationBase relation = GetStructure().GetPredicateRelations()[id];

            if (evaluated == null) throw new System.Exception("fix");
            if (ps.GetArity() != evaluated.Length) throw new System.Exception("fix");

            if (relation == null) return false; //predicaterelation gibts nicht
            if (ps.GetArity() == 0 && evaluated.Length == 0) return true; //relation mit leerem tupel                       

            if (relation.Contains(evaluated)) return true;

            return false;
        }

        private TruthValueUnderAssignment GetTruthValue(Sentence sentence, List<(VariableSymbol, int)> quantifiedAssigment, VariableAssignment assigment) {
            
            if (sentence is AtomicSentence) {
                
                AtomicSentence atom = (AtomicSentence)sentence;
                int[] eval = Evaluate(atom.GetTerms(), quantifiedAssigment, assigment);

                return new TruthValueUnderAssignment(EvaluationMatchesPredicateRelation(atom.GetPredicate(), eval), eval);
            }
            if (sentence.IsComplex()) {
                ComplexSentence c = (ComplexSentence)sentence;
                if (c.IsAffirmation()) {
                    return GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                }
                if (c.IsNegation()) {
                    TruthValueUnderAssignment p = GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                    return new TruthValueUnderAssignment(!p.GetValue(), p.GetAssignment());
                }
                if (c.IsConjunction()) {
                    TruthValueUnderAssignment p = GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                    TruthValueUnderAssignment q = GetTruthValue(c.GetQ(), quantifiedAssigment, assigment);
                    return new TruthValueUnderAssignment(p.GetValue() && q.GetValue(), p.GetAssignment(), q.GetAssignment());
                }
                if (c.IsDisjunction()) {
                    TruthValueUnderAssignment p = GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                    TruthValueUnderAssignment q = GetTruthValue(c.GetQ(), quantifiedAssigment, assigment);
                    return new TruthValueUnderAssignment(p.GetValue() || q.GetValue(), p.GetAssignment(), q.GetAssignment());
                }
                if (c.IsImplication()) {
                    TruthValueUnderAssignment p = GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                    TruthValueUnderAssignment q = GetTruthValue(c.GetQ(), quantifiedAssigment, assigment);
                    return new TruthValueUnderAssignment(!p.GetValue() || q.GetValue(), p.GetAssignment(), q.GetAssignment());
                }
                if (c.IsBiconditional()) {
                    TruthValueUnderAssignment p = GetTruthValue(c.GetP(), quantifiedAssigment, assigment);
                    TruthValueUnderAssignment q = GetTruthValue(c.GetQ(), quantifiedAssigment, assigment);
                    return new TruthValueUnderAssignment(p.GetValue() == q.GetValue(), p.GetAssignment(), q.GetAssignment());
                }
            }

            if (sentence.IsQuantifier()) {

                ComplexSentence q = (ComplexSentence)sentence;
                TruthValueUnderAssignment currentTruthValue = null;

                foreach (int uElement in GetStructure().GetUniverse().GetAllElementIDs()) {

                    VariableSymbol quantifiedVar = q.GetOperator().AsQuantifier().GetVariable();
                    //hier immer wieder eine neue zuweisung
                    for (int i = 0; i < quantifiedAssigment.Count; i++) {
                        if (quantifiedAssigment[i].Item1.Equals(quantifiedVar)) {
                            quantifiedAssigment.RemoveAt(i);
                        }
                    }
                    quantifiedAssigment.Add((quantifiedVar, uElement));

                    currentTruthValue = GetTruthValue(q.GetP(), quantifiedAssigment, assigment);

                    if (currentTruthValue.GetValue() && q.GetOperator().AsQuantifier().IsExistential()) return new TruthValueUnderAssignment(true, currentTruthValue.GetAssignment());
                    if (!currentTruthValue.GetValue() && q.GetOperator().AsQuantifier().IsUniversal()) return new TruthValueUnderAssignment(false, currentTruthValue.GetAssignment());
                }

                if (q.GetOperator().AsQuantifier().IsExistential()) return new TruthValueUnderAssignment(false, currentTruthValue.GetAssignment());
                if (q.GetOperator().AsQuantifier().IsUniversal()) return new TruthValueUnderAssignment(true, currentTruthValue.GetAssignment());
            }

            throw new System.Exception("error " + sentence.ToString());
        }


        public Dictionary<Term, Universe.Element> GetAssigmentMap(Sentence sentence, VariableAssignment variableAssigment) {

            TruthValueUnderAssignment tv = GetTruthValue(sentence, variableAssigment);
            List<int[]> trueUnder = tv.GetAssignment();
            List<AtomicSentence> atoms = sentence.GetLeafs();

            if (atoms.Count != trueUnder.Count) Debug.LogError("something is wrong");

            Dictionary<Term, Universe.Element> extract = new Dictionary<Term, Universe.Element>();

            for (int i = 0; i < atoms.Count; i++) {
                AtomicSentence curAtom = atoms[i];
                int[] curBelegung = trueUnder[i];

                for (int j = 0; j < curAtom.GetTerms().Length; j++) {
                    Term curTerm = curAtom.GetTerms()[j];
                    int elemInd = curBelegung[j];
                    Universe.Element elem = GetStructure().GetUniverse().GetElementById(elemInd);
                    if (!extract.ContainsKey(curTerm)) extract.Add(curTerm, elem);

                }
            }
            return extract;
        }


    }



}

