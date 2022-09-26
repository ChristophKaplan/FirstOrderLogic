using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace FirstOrderLogic {

    public abstract class TransformationRule {
        public abstract bool IsPossible(Sentence f);
        public abstract Sentence GetEquivalent(Sentence f);
    }


    public abstract class TransformationBase {

        protected void ApplySetOfRulesUntilCantApplyAnymore(Sentence sentence, List<TransformationRule> rules) {
            if (sentence.IsAtom()) return;

            if (IsARulePossible(sentence, rules)) {

                //apply
                foreach (TransformationRule rule in rules) {
                    if (rule.IsPossible(sentence)) {

                        Sentence e = rule.GetEquivalent(sentence);

                        //Debug.Log("replace: " + sentence.ToString() + " with:" + e.ToString()); 
                        sentence.ReplaceWithEquivalent(e);
                        //Debug.Log("replaced: " + sentence.ToString());
                    }
                }

                //try again after changes, but from root !
                ApplySetOfRulesUntilCantApplyAnymore(sentence.GetRoot(), rules);
            } else {
                //check for children
                for (int i = 0; i < sentence.AsComplex().GetChildren().Length; i++) {
                    ApplySetOfRulesUntilCantApplyAnymore(sentence.AsComplex().GetChildren()[i], rules);
                }
            }

        }
        private bool IsARulePossible(Sentence sentence, List<TransformationRule> rules) {
            foreach (TransformationRule rule in rules) if (rule.IsPossible(sentence)) return true;
            return false;
        }


        protected Dictionary<Term, Universe.Element> GetAssignmentMapWhereSentenceIsTrue(Sentence sentence, Interpretation interpretation, VariableAssignment varAssignment) {
            return interpretation.GetAssigmentMap(sentence, varAssignment);
        }

        protected Universe.Element GetElementOfAssigmentMap(Dictionary<Term, Universe.Element> map, VariableSymbol varOfTerm) {
            foreach (Term t in map.Keys)
                if (t.IsVariableInTerm(varOfTerm)) return map[t];
            return null;
        }

        protected (Universe.Element[], Universe.Element) GetFromToOfAssigmentMap(List<Symbol> FromSymbols, VariableSymbol ToSymbol, Dictionary<Term, Universe.Element> assigmentMap) {
            Universe.Element[] from = new Universe.Element[FromSymbols.Count];
            for (int i = 0; i < FromSymbols.Count; i++) from[i] = GetElementOfAssigmentMap(assigmentMap, (VariableSymbol)FromSymbols[i]);
            Universe.Element to = GetElementOfAssigmentMap(assigmentMap, (VariableSymbol)ToSymbol);
            return new(from, to);
        }

        protected Substitution BuildFunctionAndGetSubstitution(string name, List<Symbol> symbols, VariableSymbol quantifierVar, Universe.Element[] from, Universe.Element to, Interpretation interpretation) {
            FunctionSymbol functionSymbol = new FunctionSymbol(name + Random.Range(0, 99), symbols.Count);            
            int id = interpretation.GetStructure().AddFunction(new Function(from,to));
            interpretation.ExtendAFunction(functionSymbol,id);
            FunctionTerm functionAsTerm = new FunctionTerm(functionSymbol, new string[0]);
            return new Substitution(new VariableTerm(quantifierVar), functionAsTerm);
        }

        protected Dictionary<Symbol, List<Symbol>> GetQuantifierScopeDependencies(Sentence sentence) {
            List<ComplexSentence> prenexQuantifier = sentence.GetLowerQuantifiers();
            Dictionary<Symbol, List<Symbol>> scopeDependency = new Dictionary<Symbol, List<Symbol>>();

            for (int i = prenexQuantifier.Count - 1; i >= 0; i--) {
                Quantifier currentQuantifier = prenexQuantifier[i].GetOperator().AsQuantifier();
                if (currentQuantifier.IsUniversal()) continue;

                //collect "independent" quantifiers as well
                if (!scopeDependency.ContainsKey(currentQuantifier.GetVariable())) scopeDependency.Add(currentQuantifier.GetVariable(), new List<Symbol>());

                for (int j = i - 1; j >= 0; j--) {
                    //scan left side of the current quantifier
                    if (IsQuantifiedSentenceInScopeOf(prenexQuantifier[j],prenexQuantifier[i])) {
                        scopeDependency[currentQuantifier.GetVariable()].Add(prenexQuantifier[j].GetOperator().AsQuantifier().GetVariable());
                    }
                }
            }
            return scopeDependency;
        }

        protected bool IsQuantifiedSentenceInScopeOf(ComplexSentence quantifiedSentenceHigher, ComplexSentence quantifiedSentenceLower) {
            List<Term> lower = quantifiedSentenceLower.GetTermsInQuantifierScope();
            List<Term> higher = quantifiedSentenceHigher.GetTermsInQuantifierScope();
            for (int i = 0; i < lower.Count; i++) {
                for (int j = 0; j < higher.Count; j++) if (higher[j].HasVariableIntersection(lower[i])) return true;
            }
            return false;
        }

        protected Sentence RemovePrenexQuantifiers(Sentence sentence) {            
            if (sentence.IsQuantifier()) {                
                return RemovePrenexQuantifiers(sentence.AsComplex().GetChildren()[0]);
            }
            return sentence.GetCopy();
        }

        protected void TakeOutQuantifier(Sentence sentence, Symbol quantifierVar) {
            List<ComplexSentence> lower = sentence.GetLowerQuantifiers();
            for (int i = 0; i < lower.Count; i++) {
                Quantifier low = (Quantifier)lower[i].GetOperator();
                if (low.GetVariable().Equals(quantifierVar)) {
                    if (lower[i].IsRoot()) {
                        sentence = lower[i].GetP().GetCopy();
                        return;
                    }

                    ComplexSentence parent = lower[i].GetParent();
                    for (int j = 0; j < parent.GetChildren().Length; j++) {
                        Sentence child = parent.GetChildren()[j];
                        if (child.Equals(lower[i])) {
                            parent.GetChildren()[j] = lower[i].GetP();
                            return;
                        }
                    }

                }
            }
            throw new System.Exception("TakeOutQuantifier() error");
        }

    }
    public class Transformation : TransformationBase {

        protected List<TransformationRule> tRules;

        public static Idempotenz idempotenz = new Idempotenz();
        public static Kommutativitaet Kommutativitaet = new Kommutativitaet();
        public static Assoziativitaet assoziativitaet = new Assoziativitaet();
        public static Absorbtion absorbtion = new Absorbtion();
        public static Distributivitaet distributivitaet = new Distributivitaet();
        public static DoubleNegation doubleNegation = new DoubleNegation();
        public static DeMorganscheRegel deMorganscheRegel = new DeMorganscheRegel();
        public static TautologyRule tautologieregeln = new TautologyRule();
        public static Unerfuellbarkeitsregeln unerfuellbarkeitsregeln = new Unerfuellbarkeitsregeln();
        public static Contraposition contraposition = new Contraposition();
        public static Implication implication = new Implication();
        public static Coimplikation coimplikation = new Coimplikation();
        public static QuantifierNegation quantifierNegation = new QuantifierNegation();
        public static PullQuantifier pullQuantifier = new PullQuantifier();
        public static PullQuantifier_2 pullQuantifier_2 = new PullQuantifier_2();
        public static CleanedNormalform cleanedNormalform = new CleanedNormalform();

        public Transformation() {
                tRules = new List<TransformationRule>(){
                new Idempotenz(),
                new Kommutativitaet(),
                new Assoziativitaet(),
                new Absorbtion(),
                new Distributivitaet(),
                new DoubleNegation(),
                new DeMorganscheRegel(),
                new TautologyRule(),
                new Unerfuellbarkeitsregeln(),
                new Contraposition(),
                new Implication(),
                new Coimplikation(),
                new QuantifierNegation(),
                new PullQuantifier(),
                new PullQuantifier_2(),
                new CleanedNormalform(),
            };


        }



        protected string ShowPossible(Sentence formel) {
            ComplexSentence v = formel.AsComplex();
            string s = "";
            s += "Count:" + GetEquivs(v).Count + " " + v.ToString() + "\n";
            for (int i = 0; i < GetEquivs(v).Count; i++) if (GetEquivs(v)[i] != null) s += GetEquivs(v)[i].ToString() + "\n";
            for (int i = 0; i < tRules.Count; i++) s += "" + tRules[i] + ":" + tRules[i].IsPossible(v) + "\n";            
            return s;
        }

        protected List<Sentence> GetEquivs(ComplexSentence v) {
            List<Sentence> eq = new List<Sentence>();
            for (int i = 0; i < tRules.Count; i++) {
                if (tRules[i].IsPossible(v)) eq.Add(tRules[i].GetEquivalent(v));                
            }
            return eq;
        }



    }






}













