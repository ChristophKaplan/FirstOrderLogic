using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using UnityEngine;


namespace FirstOrderLogic {

    public class LogicSystem {
        private Structure structure = new Structure();
        public Structure GetStructure() => this.structure;

        private Signature signature = new Signature(new List<FunctionSymbol>() { }, new List<PredicateSymbol>() { });
        public Signature GetSignature() => this.signature;

        private List<Interpretation> interpretations = new List<Interpretation>();
        public List<Interpretation> GetInterpretations() => this.interpretations;

        private VariableAssignment variablenbelegung = new VariableAssignment(new Dictionary<VariableSymbol, int>() { });
        public VariableAssignment GetVariableAssignment() => this.variablenbelegung;

        private List<Sentence> sentences = new List<Sentence>();
        public List<Sentence> GetSentences() => this.sentences;

        public LogicSystem() {

        }

        public virtual void AddSentence(Sentence f) {
            this.sentences.Add(f);
        }
        public void PrintSentencesWithTruthValue() {
            for (int i = 0; i < this.sentences.Count; i++) Debug.Log(PrintSentenceWithTruthValue(sentences[i]));
        }
        public string PrintSentenceWithTruthValue(Sentence sentence) {
            string s = "";
            for (int j = 0; j < GetInterpretations().Count; j++) {
                s += "Int[" + j + "]: Sentence: " + sentence.ToString() + " = " + this.interpretations[j].GetTruthValue(sentence, variablenbelegung).ToString() + "\n";
            }
            return s;
        }
        public void PrintInterpretations() {
            for (int i = 0; i < this.interpretations.Count; i++) Debug.Log(interpretations[i]);
        }

        public Interpretation GetNewInterpretation() {
            Interpretation i = new Interpretation(this.structure, this.signature);
            interpretations.Add(i);
            return i;
        }

    }



    public class LogicSystemInterface : LogicSystem {

        public LogicSystemInterface() {

        }



        //Get
        public Universe.Element GetElementById(int id) {
            return GetStructure().GetUniverse().GetElementById(id);
        }
        public Dictionary<VariableSymbol, int> GetAssignment() {
            return GetVariableAssignment().GetAssignment();
        }
        public Dictionary<PredicateSymbol, int> GetPredicates(Interpretation interpretation) {
            return interpretation.GetPredicates();
        }
        public Dictionary<FunctionSymbol, int> GetFunctions(Interpretation interpretation) {
            return interpretation.GetFunctions();
        }
        public List<FunctionSymbol> GetFunctionSymbols() {
            return GetSignature().GetFunctionSymbols();
        }
        public List<PredicateSymbol> GetPredicateSymbols() {
            return GetSignature().GetPredicateSymbols();
        }

        //Add
        public void AddElement(Universe.Element Element) {
            GetStructure().GetUniverse().AddElement(Element);
        }
        public void AddElements(List<Universe.Element> Elements) {
            GetStructure().GetUniverse().AddElements(Elements);
        }
        public void AddFreeVariable(string a, Universe.Element b) {
            GetVariableAssignment().AddAssignment(a, b);
        }
        public void AddPredicateSymbol(PredicateSymbol ps) {
            GetSignature().AddPredicateSymbol(ps);
        }
        public void AddFunctionSymbol(FunctionSymbol fs) {
            GetSignature().AddFunctionSymbol(fs);
        }
        public override void AddSentence(Sentence sentence) {
            base.AddSentence(sentence);
        }

        public void AddUniverse(Universe universe) {
            GetStructure().AddUniverse(universe);
        }

        public void AddPredicateRelation(Interpretation interpretation, string predName, List<Universe.Element[]> args) {
            int id = GetStructure().AddPredicateRelation(new PredicateRelation(args));
            interpretation.AddToPredicateMap(predName, id);
        }
        public void AddPredicateRelation(Interpretation interpretation, string predName, PredicateRelationBase prb) {
            int id = GetStructure().AddPredicateRelation(prb);
            interpretation.AddToPredicateMap(predName, id);
        }

        public void AddPredicateRelation(Interpretation interpretation, string predName, List<Universe.Element> args) { // 1 Stellig
            int id = GetStructure().AddPredicateRelation(new PredicateRelation(args));
            interpretation.AddToPredicateMap(predName, id);
        }

        public void AddFunction(Interpretation interpretation, string f, List<Universe.Element[]> input, Universe.Element returnValue) {
            int id = GetStructure().AddFunction(new Function(input, returnValue));
            interpretation.AddToFunctionMap(f, id);
        }

        public void AddInterpretation(Interpretation interpretation) {
            GetInterpretations().Add(interpretation);
        }


        //Remove
        public void RemoveElement(Universe.Element Element) {
            GetStructure().GetUniverse().RemoveElement(Element);
        }
        public void RemoveFreeVariable(VariableSymbol var) {
            GetVariableAssignment().RemoveAssignment(var);
        }
        public void RemovePredicateSymbol(PredicateSymbol ps) {
            GetPredicateSymbols().Remove(ps);
        }
        public void RemoveFunctionSymbol(FunctionSymbol fs) {
            GetFunctionSymbols().Remove(fs);
        }
        public void RemoveSentence(Sentence sentence) {
            base.GetSentences().Remove(sentence);
        }
        public void RemovePredicte(Interpretation interpretation, PredicateSymbol ps) {
            interpretation.GetPredicates().Remove(ps);
        }
        public void RemoveFunction(Interpretation interpretation, FunctionSymbol fs) {
            interpretation.GetFunctions().Remove(fs);
        }
        public void RemoveInterpretation(Interpretation interpretation) {
            GetInterpretations().Remove(interpretation);
        }


        //NormalForm
        public Sentence GetPrenexNormalForm(Sentence sentence) {
            NormalForm normalForm = new NormalForm();
            Sentence prenex = normalForm.GetPrenexNNF(sentence);
            return prenex;
        }

        public Sentence GetConjunktiveNormalForm(Sentence sentence) { //PNF recommendet
            NormalForm normalForm = new NormalForm();
            Sentence cnf = normalForm.GetCNF(sentence);
            return cnf;
        }

        public Sentence GetSkolemForm(Sentence sentenceMustBePNF, Interpretation interpretation, VariableAssignment variableAssignment) {
            NormalForm normalForm = new NormalForm();
            Sentence skolemform = normalForm.Skolemization(sentenceMustBePNF, interpretation, variableAssignment);
            return skolemform;
        }
        public ClauseSet GetClauseSet(Sentence sentenceMustBeCNF) {
            NormalForm normalForm = new NormalForm();
            ClauseSet cs = normalForm.GetClauseSet(sentenceMustBeCNF);
            return cs;
        }



        //Inference
        //ModusPonens
        public Sentence GetModusPonens(params Sentence[] premise) {
            Inference inference = new Inference();
            if (!inference.IsModusPonensPossible(premise)) return null;
            return inference.GetModusPonens(premise);
        }

        //ModusTolles
        public Sentence GetModusTollens(params Sentence[] premise) {
            Inference inference = new Inference();
            if (!inference.IsModusTollensPossible(premise)) return null;
            return inference.GetModusTollens(premise);
        }

        //AndInduction
        public Sentence GetAndInductionConclusion(params Sentence[] premise) {
            Inference inference = new Inference();
            return inference.GetAndInductionConclusion(premise);
        }

        //And Elimination
        public Sentence GetAndEliminationConclusion(params Sentence[] premise) {
            Inference inference = new Inference();
            if (inference.IsAndEliminationPossible(premise)) return null;
            return inference.GetAndEliminationConclusion(premise);
        }

        //Existential Instance
        public Sentence GetExistentialInstance(Sentence premise, Interpretation interpretation, VariableAssignment variableAssignment) {
            Inference inference = new Inference();
            if (inference.IsExistentialInstancePossible(premise)) return null;
            return inference.GetExistentialInstance(premise, interpretation, variableAssignment);

        }

        //Universal Instance
        public Sentence GetUniversalInstanceConclusion(Sentence premise, Universe.Element whereElement, Interpretation interpretation) {
            Inference inference = new Inference();
            if (inference.IsUniversalInstancePossible(premise)) return null;
            return inference.GetUniversalInstanceConclusion(premise, whereElement, interpretation);
        }

        public bool ResolutionProof(Sentence premise1, Sentence premise2, Sentence conclusion) {
            Inference inference = new Inference();
            return inference.ResolutionProof(premise1, premise2, conclusion);
        }

        //String
        public Sentence SringToSentence(string stringSentence, bool checkSignature = true) {
            Sentence s = GetSignature().TryParse(stringSentence, checkSignature);
            return s;
        }

        public string GetStringSyntaxTree(Sentence sentence) {
            StringTree stringTree = new StringTree();
            return stringTree.GetSyntaxTree(sentence);
        }

    }

}


