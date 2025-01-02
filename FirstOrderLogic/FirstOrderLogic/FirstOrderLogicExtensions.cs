using System;
using System.Collections.Generic;
using LogHelper;
using LRParser.Language;

namespace FirstOrderLogic {
    public static class FirstOrderLogicExtensions
    {
        public static Connective.LogicSymbol ToLogicalConstant(this LexValue lexValue) {
            switch (lexValue.Value) {
                case "OR":
                case "||":
                    return Connective.LogicSymbol.DISJUNCTION;
                case "AND":
                case "&&":
                    return Connective.LogicSymbol.CONJUNCTION;
                case "NOT":
                case "!":
                case "-":
                case "~":
                case "\u00ac":
                    return Connective.LogicSymbol.NEGATION;
                case "IFF":
                case "<=>":
                    return Connective.LogicSymbol.BICONDITIONAL;
                case "IMPLIES":
                case "=>":
                    return Connective.LogicSymbol.IMPLICATION;
                case "TRUE":
                    return Connective.LogicSymbol.TRUE;
                case "FALSE":
                    return Connective.LogicSymbol.FALSE;
                case "FORALL":
                    return Connective.LogicSymbol.UNIVERSAL;
                case "EXISTS":    
                    return Connective.LogicSymbol.EXISTENTIAL;
                
                default:
                    throw new Exception($"Unknown Logic Symbol: {lexValue}");
            }
        }
    
        public static ISentence ConnectSentences(this FirstOrderLogic logic, List<ISentence> sentences, Connective.LogicSymbol connective = Connective.LogicSymbol.CONJUNCTION) {
            switch (sentences.Count) {
                case 0:
                    throw new Exception("No sentences to connect.");
                case 1:
                    return sentences[0];
            }

            var conjunct = new ComplexSentence(sentences[0], connective, sentences[1]);
            for (var i = 2; i < sentences.Count; i++) {
                conjunct = new ComplexSentence(conjunct,connective, sentences[i]);
            }

            return conjunct;
        }
    
        private delegate void TransformationDelegate(ref ISentence sentence);
        public static ISentence ToPrenexForm(this FirstOrderLogic logic, ISentence sentence, out List<ISentence> steps) {
            steps = new List<ISentence>();
            var clone = sentence.Clone();
        
            var transformations = new List<TransformationDelegate> {
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.SimplifyConstants, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveBiconditional, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DissolveImplication, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.PushNegation, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DoubleNegation, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.Absorption, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.AssociationAndIdem, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.PullQuantifier, ref s),
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.RemoveDuplicateQuantifier, ref s),
            };

            while (true) {
                var start = clone.Clone();
                foreach (var transform in transformations) {
                    transform(ref clone);
                    steps.Add(clone.Clone());
                }
                if (start.Equals(clone)) {
                    break;
                }
            }

            Logger.Log("pnf done.");
            return clone;
        }

        public static ISentence ToConjunctiveNormalForm(this FirstOrderLogic logic, ISentence sentence, out List<ISentence> steps) {
            var pnf = ToPrenexForm(logic, sentence, out steps);
        
            var clone = pnf.Clone();
            var transformations = new List<TransformationDelegate> {
                (ref ISentence s) => TransformationFOL.Transform(TransformationFOL.EquivType.DistributionOfDisjunction, ref s)
            };

            while (true) {
                var start = clone.Clone();
                foreach (var transform in transformations) {
                    transform(ref clone);
                    steps.Add(clone.Clone());
                }
                if (start.Equals(clone)) {
                    break;
                }
            }
        
            if(!clone.IsCNF()) { throw new Exception("Sentence is not in CNF"); }
            return clone;
        }

        public static ISentence SkolemForm(this FirstOrderLogic logic, ISentence sentence) {
            var clone = sentence.Clone();
        
            //1. is PNF ?
            var complexSentence = (IComplexSentence) clone;
            var  universalQantifiers = complexSentence.GetQuantifiers(Connective.LogicSymbol.UNIVERSAL);
            var  existentialQuantifiers = complexSentence.GetQuantifiers(Connective.LogicSymbol.EXISTENTIAL);
       
            Dictionary<Variable, Function> substitution = new();

            foreach (var exist in existentialQuantifiers) {
                var args = new List<Term>();
                foreach (var universal in universalQantifiers) {
                    args.Add(universal.Variable);
                }

                //TODO: semantics of skolem function ??
                var skolemFunction = new Function("sk", args.ToArray());
                substitution.Add(exist.Variable, skolemFunction);
            }
        
            foreach (var var in substitution.Keys) {
                clone.SubstituteTerm(var, substitution[var]);
            }
        
            //remove quantifiers
            TransformationFOL.Transform(TransformationFOL.EquivType.RemoveQuantifier, ref clone);
        
            return clone;
        }
    
        public static List<Clause> GetClauseSet(this ISentence sentence, List<Clause> clauseSet = null) {
            if (!sentence.IsCNF()) { throw new Exception("Sentence is not in CNF"); }
        
            clauseSet ??= new List<Clause>();
            var clone = sentence.Clone(); 
        
            if(clone.IsDisjunctionOfLiterals())
            {
                var clauseList = clone.GetLiterals();
                clauseSet.Add(new Clause(clauseList.ToArray()));
                return clauseSet;
            }
        
            foreach (var child in sentence.Children)
            {
                child.GetClauseSet(clauseSet);
            }
        
            return clauseSet;
        }

        public static List<ISentence> GetInstancesOverTime(this ISentence sentence, int from, int to) {
            var sentences = new List<ISentence>();
            for (var i = from; i < to; i++) {
                var clone = sentence.Clone();
                clone.AddTime(i);
                sentences.Add(clone);
            }

            return sentences;
        }
    }
}