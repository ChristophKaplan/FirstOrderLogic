using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace FirstOrderLogic {
    public enum Terminal
    {
        Open,
        Comma,
        Close,
        Identifier,
        Conjunction,
        Disjunction,
        Implication,
        Negation,
        Boolean,
        Quantifier,
        Biconditional,
        TimeAttribute
    }

    public enum NonTerminal
    {
        LangObject,
        AtomicSentence,
        Term,
        TermList,
        TermListExt,
        TermExt,
        Sentence,
        AtomicSentenceExt,
        ComplexSentence,
        LogicalOperator,
        ComplexSentenceUnary
    }

    public class FirstOrderLogic : Language<Terminal, NonTerminal>
    {
        protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
        {
            return new[]
            {
                new TokenDefinition<Terminal>(Terminal.Open, "\\("),
                new TokenDefinition<Terminal>(Terminal.Comma, ","),
                new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
                new TokenDefinition<Terminal>(Terminal.Conjunction, "AND|&&"),
                new TokenDefinition<Terminal>(Terminal.Disjunction, "OR|\\|\\|"),
                new TokenDefinition<Terminal>(Terminal.Implication, "IMPLIES|=>"),
                new TokenDefinition<Terminal>(Terminal.Biconditional, "IFF|<=>"),
                new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!|-|~|¬"),
                new TokenDefinition<Terminal>(Terminal.Boolean, "TRUE|FALSE"),
                new TokenDefinition<Terminal>(Terminal.Quantifier, "FORALL|EXISTS"),
                new TokenDefinition<Terminal>(Terminal.TimeAttribute, "\\^[0-9]"),
                new TokenDefinition<Terminal>(Terminal.Identifier, "[a-zA-Z0-9]+"),
            };
        }

        protected override void SetUpGrammar()
        {
            AddRule(rhs => rhs[0].Attribute, NonTerminal.LangObject, NonTerminal.Sentence);

            AddRule(rhs => rhs[1].Attribute, NonTerminal.Sentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
            AddRule(rhs => rhs[0].Attribute, NonTerminal.Sentence, NonTerminal.ComplexSentence);
            AddRule(rhs => rhs[0].Attribute, NonTerminal.Sentence, NonTerminal.AtomicSentence);
            AddRule(rhs =>
            {
                var boolean = ((LexValue)rhs[0].Attribute).ToLogicalConstant();
                return new Proposition(boolean.ToString());
            }, NonTerminal.Sentence, Terminal.Boolean);

            AddRule(rhs =>
            {
                var quantifierSymbol = ((LexValue)rhs[0].Attribute).ToLogicalConstant();
                var variableString = ((LexValue)rhs[1].Attribute).Value;
                var sentence = (Sentence)rhs[2].Attribute;
                return new ComplexSentence(new Quantifier(quantifierSymbol, new Variable(variableString)), sentence);
            }, NonTerminal.ComplexSentence, Terminal.Quantifier, Terminal.Identifier, NonTerminal.Sentence);

            AddRule(rhs =>
            {
                var atomic = (Sentence)rhs[0].Attribute;
                var extArray = (ArrayValue)rhs[1].Attribute;
                var connective = (Connective)extArray.Value[0];
                var sentence = (Sentence)extArray.Value[1];
                return new ComplexSentence(atomic, connective, sentence);
            }, NonTerminal.ComplexSentence, NonTerminal.AtomicSentence, NonTerminal.ComplexSentenceUnary);

            AddRule(rhs =>
            {
                var atomic = (Sentence)rhs[1].Attribute;
                var extArray = (ArrayValue)rhs[3].Attribute;
                var connective = (Connective)extArray.Value[0];
                var sentence = (Sentence)extArray.Value[1];
                return new ComplexSentence(atomic, connective, sentence);
            }, NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, NonTerminal.ComplexSentenceUnary);

            AddRule(rhs =>
            {
                var extArray = (ArrayValue)rhs[0].Attribute;
                var negation = (Connective)extArray.Value[0];
                var sentence = (Sentence)extArray.Value[1];
                return new ComplexSentence(negation, sentence);
            }, NonTerminal.ComplexSentence, NonTerminal.ComplexSentenceUnary);

            AddRule(rhs =>
            {
                var connective = (Connective)rhs[0].Attribute;
                var sentences = (Sentence)rhs[1].Attribute;
                return new ArrayValue(connective, sentences);
            }, NonTerminal.ComplexSentenceUnary, NonTerminal.LogicalOperator, NonTerminal.Sentence);

            AddRule(GetAtomicSentence, NonTerminal.AtomicSentence, Terminal.Identifier, NonTerminal.AtomicSentenceExt);
            AddRule(GetAtomicSentence, NonTerminal.AtomicSentence, Terminal.Identifier, NonTerminal.AtomicSentenceExt, Terminal.TimeAttribute);
            AddRule(rhs => rhs[1].Attribute, NonTerminal.AtomicSentenceExt, Terminal.Open, NonTerminal.TermList, Terminal.Close);
            AddRule(rhs => rhs[0].Attribute, NonTerminal.AtomicSentenceExt, InternalSymbol.Epsilon);

            AddRule(rhs =>
            {
                var firstTerm = rhs[0].Attribute;

                if (rhs[1].Attribute == null)
                {
                    return new ArrayValue(firstTerm);
                }

                var ext = (ArrayValue)rhs[1].Attribute;
                ext.Insert(firstTerm, 0);
                return ext;
            }, NonTerminal.TermList, NonTerminal.Term, NonTerminal.TermListExt);

            AddRule(rhs =>
            {
                var firstTerm = rhs[1].Attribute;

                if (rhs[2].Attribute == null)
                {
                    return new ArrayValue(firstTerm);
                }

                var ext = (ArrayValue)rhs[2].Attribute;
                ext.Insert(firstTerm, 0);
                return ext;
            }, NonTerminal.TermListExt, Terminal.Comma, NonTerminal.Term, NonTerminal.TermListExt);

            AddRule(rhs => new ArrayValue(Array.Empty<ILanguageObject>()), NonTerminal.TermListExt, InternalSymbol.Epsilon);

            AddRule(rhs =>
            {
                var symbol = ((LexValue)rhs[0].Attribute).Value;

                ILanguageObject term;

                if (rhs[1].Attribute != null)
                {
                    var extArray = (ArrayValue)rhs[1].Attribute;
                    var terms = extArray.Value.Select(lexValue => (Term)lexValue).ToArray();
                    term = new Function(symbol, terms);
                }
                else
                {
                    var variableList = new[] { "x", "y", "z", "w" };
                    var isVariable = variableList.Contains(symbol);
                    term = isVariable ? (ILanguageObject)new Variable(symbol) : new Constant(symbol);
                }

                return term;
            }, NonTerminal.Term, Terminal.Identifier, NonTerminal.TermExt);

            AddRule(rhs => rhs[1].Attribute, NonTerminal.TermExt, Terminal.Open, NonTerminal.TermList, Terminal.Close);
            AddRule(rhs => rhs[0].Attribute, NonTerminal.TermExt, InternalSymbol.Epsilon);

            AddRule(GetConnective, NonTerminal.LogicalOperator, Terminal.Conjunction);
            AddRule(GetConnective, NonTerminal.LogicalOperator, Terminal.Disjunction);
            AddRule(GetConnective, NonTerminal.LogicalOperator, Terminal.Implication);
            AddRule(GetConnective, NonTerminal.LogicalOperator, Terminal.Biconditional);
            AddRule(GetConnective, NonTerminal.LogicalOperator, Terminal.Negation);
        }

        ILanguageObject GetConnective(Symbol[] rhs) => new Connective(((LexValue)rhs[0].Attribute).ToLogicalConstant());

        ILanguageObject GetAtomicSentence(Symbol[] rhs) {
            var symbol = ((LexValue)rhs[0].Attribute).Value;

            int? timeValue = null;
            if (rhs.Length > 2)
            {
                var timeAttribute = ((LexValue)rhs[2].Attribute).Value;
                timeValue = int.Parse(timeAttribute[1..]);
            }
            
            if (rhs[1].Attribute != null)
            {
                var extArray = (ArrayValue)rhs[1].Attribute;
                var terms = extArray.Value.Select(lexValue => (Term)lexValue).ToArray();
                return timeValue.HasValue ? new Predicate(symbol, terms, (int)timeValue) : new Predicate(symbol, terms);
            }
            
            return timeValue.HasValue ? new Proposition(symbol, (int)timeValue) : new Proposition(symbol);
        }
    
        public override ILanguageObject TryParse(string input)
        {
            var langObj = base.TryParse(input);
            return langObj;
        }
    
        public List<ILanguageObject> TryParse(List<string> inputList)
        {
            var langObjList = new List<ILanguageObject>();
            foreach (var input in inputList) {
                langObjList.Add(TryParse(input));
            }
            return langObjList;
        }
    }
}