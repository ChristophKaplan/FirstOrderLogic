using System.Text;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace FirstOrderLogic;

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
    public FirstOrderLogic() 
    {
        Console.OutputEncoding = Encoding.UTF8;
    }
    
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
            new TokenDefinition<Terminal>(Terminal.Identifier, "[a-zA-Z]+"),
        };
    }

    protected override void SetUpGrammar()
    {
        var ruleStart = AddProductionRule(SpecialNonTerminal.Start, NonTerminal.LangObject);
        var ruleLangObj = AddProductionRule(NonTerminal.LangObject, NonTerminal.Sentence);

        var ruleSentence = AddProductionRule(NonTerminal.Sentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        var ruleSentenceComp = AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        var ruleSentenceAtom = AddProductionRule(NonTerminal.Sentence, NonTerminal.AtomicSentence);
        var ruleSentenceBoolean = AddProductionRule(NonTerminal.Sentence, Terminal.Boolean);
        
        var ruleSentenceQuantifier = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Quantifier, Terminal.Identifier, NonTerminal.Sentence);
        var ruleComplexSentenceAtomic = AddProductionRule(NonTerminal.ComplexSentence, NonTerminal.AtomicSentence, NonTerminal.ComplexSentenceUnary);
        var ruleComplexSentenceBraces = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, NonTerminal.ComplexSentenceUnary);
        var ruleComplexSentenceNegation = AddProductionRule(NonTerminal.ComplexSentence, NonTerminal.ComplexSentenceUnary);
        var ruleComplexSentenceExt = AddProductionRule(NonTerminal.ComplexSentenceUnary, NonTerminal.LogicalOperator, NonTerminal.Sentence);

        var ruleAtomicSentence = AddProductionRule(NonTerminal.AtomicSentence, Terminal.Identifier, NonTerminal.AtomicSentenceExt);
        var ruleAtomicSentenceT = AddProductionRule(NonTerminal.AtomicSentence, Terminal.Identifier, NonTerminal.AtomicSentenceExt, Terminal.TimeAttribute);
        var ruleAtomicSentenceExtPred = AddProductionRule(NonTerminal.AtomicSentenceExt, Terminal.Open, NonTerminal.TermList, Terminal.Close);
        var ruleAtomicSentenceExtProp = AddProductionRule(NonTerminal.AtomicSentenceExt, SpecialTerminal.Epsilon);

        
        var ruleTermList = AddProductionRule(NonTerminal.TermList, NonTerminal.Term, NonTerminal.TermListExt);
        var ruleTermListExt = AddProductionRule(NonTerminal.TermListExt, Terminal.Comma, NonTerminal.Term, NonTerminal.TermListExt);
        var ruleTermListExtEnd = AddProductionRule(NonTerminal.TermListExt, SpecialTerminal.Epsilon);

        var ruleTermVarOrConstOrFunc = AddProductionRule(NonTerminal.Term, Terminal.Identifier, NonTerminal.TermExt);
        var ruleTermExt = AddProductionRule(NonTerminal.TermExt, Terminal.Open, NonTerminal.TermList, Terminal.Close);
        var ruleTermExtEnd = AddProductionRule(NonTerminal.TermExt, SpecialTerminal.Epsilon);

        var ruleCon = AddProductionRule(NonTerminal.LogicalOperator, Terminal.Conjunction);
        var ruleDis = AddProductionRule(NonTerminal.LogicalOperator, Terminal.Disjunction);
        var ruleImp = AddProductionRule(NonTerminal.LogicalOperator, Terminal.Implication);
        var ruleIFF = AddProductionRule(NonTerminal.LogicalOperator, Terminal.Biconditional);
        var ruleNeg = AddProductionRule(NonTerminal.LogicalOperator, Terminal.Negation);

        ruleStart.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleLangObj.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleSentence.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });
        ruleSentenceComp.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleSentenceAtom.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleSentenceBoolean.SetSemanticAction((lhs, rhs) =>
        {
            var boolean = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant();
            lhs.SyntheticAttribute = new Proposition(boolean.ToString());
        });
            
        ruleSentenceQuantifier.SetSemanticAction((lhs, rhs) =>
        {
            var quantifierSymbol = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant();
            var variableString = ((LexValue)rhs[1].SyntheticAttribute).Value;
            var sentence = (Sentence)rhs[2].SyntheticAttribute;
            lhs.SyntheticAttribute = new ComplexSentence(new Quantifier(quantifierSymbol, new Variable(variableString)), sentence);
        });

        ruleComplexSentenceAtomic.SetSemanticAction((lhs, rhs) =>
        {
            var atomic = (Sentence)rhs[0].SyntheticAttribute;
            var extArray = (ArrayValue)rhs[1].SyntheticAttribute;
            var connective = (Connective)extArray.Value[0];
            var sentence = (Sentence)extArray.Value[1];
            lhs.SyntheticAttribute = new ComplexSentence(atomic, connective, sentence);
        });

        ruleComplexSentenceBraces.SetSemanticAction((lhs, rhs) =>
        {
            var atomic = (Sentence)rhs[1].SyntheticAttribute;
            var extArray = (ArrayValue)rhs[3].SyntheticAttribute;
            var connective = (Connective)extArray.Value[0];
            var sentence = (Sentence)extArray.Value[1];
            lhs.SyntheticAttribute = new ComplexSentence(atomic, connective, sentence);
        });

        ruleComplexSentenceExt.SetSemanticAction((lhs, rhs) =>
        {
            var connective = (Connective)rhs[0].SyntheticAttribute;
            var sentences = (Sentence)rhs[1].SyntheticAttribute;
            lhs.SyntheticAttribute = new ArrayValue(connective, sentences);
        });

        ruleComplexSentenceNegation.SetSemanticAction((lhs, rhs) =>
        {
            var extArray = (ArrayValue)rhs[0].SyntheticAttribute;
            var negation = (Connective)extArray.Value[0];
            var sentence = (Sentence)extArray.Value[1];
            lhs.SyntheticAttribute = new ComplexSentence(negation, sentence);
        });

        ruleAtomicSentence.SetSemanticAction((lhs, rhs) =>
        {
            lhs.SyntheticAttribute = GetAtomicSentence(rhs);
        });
        
        ruleAtomicSentenceT.SetSemanticAction((lhs, rhs) =>
        {
            lhs.SyntheticAttribute = GetAtomicSentence(rhs);
        });


        
        ruleAtomicSentenceExtPred.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });
        ruleAtomicSentenceExtProp.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleTermExt.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        ruleTermList.SetSemanticAction((lhs, rhs) =>
        {
            var firstTerm = rhs[0].SyntheticAttribute;

            if (rhs[1].SyntheticAttribute == null)
            {
                lhs.SyntheticAttribute = new ArrayValue(firstTerm);
                ;
                return;
            }

            var ext = (ArrayValue)rhs[1].SyntheticAttribute;
            ext.Add(firstTerm);
            lhs.SyntheticAttribute = ext;
        });

        ruleTermListExt.SetSemanticAction((lhs, rhs) =>
        {
            var firstTerm = rhs[1].SyntheticAttribute;

            if (rhs[2].SyntheticAttribute == null)
            {
                lhs.SyntheticAttribute = new ArrayValue(firstTerm);
                return;
            }

            var ext = (ArrayValue)rhs[2].SyntheticAttribute;
            ext.Add(firstTerm);
            lhs.SyntheticAttribute = ext;
        });

        ruleTermListExtEnd.SetSemanticAction((lhs, rhs) =>
        {
            lhs.SyntheticAttribute = new ArrayValue(Array.Empty<ILanguageObject>());
        });

        ruleTermExtEnd.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });

        ruleTermVarOrConstOrFunc.SetSemanticAction((lhs, rhs) =>
        {
            var symbol = ((LexValue)rhs[0].SyntheticAttribute).Value;

            var term = default(ILanguageObject);

            if (rhs[1].SyntheticAttribute != null)
            {
                var extArray = (ArrayValue)rhs[1].SyntheticAttribute;
                var terms = extArray.Value.Select(lexValue => (Term)lexValue).ToArray();
                term = new Function(symbol, terms);
            }
            else
            {
                var variableList = new[] { "x", "y", "z", "w" };
                var isVariable = variableList.Contains(symbol);
                term = isVariable ? new Variable(symbol) : new Constant(symbol);
            }

            lhs.SyntheticAttribute = term;
        });

        ruleCon.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = GetConnective(rhs); });
        ruleDis.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = GetConnective(rhs); });
        ruleImp.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = GetConnective(rhs); });
        ruleIFF.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = GetConnective(rhs); });
        ruleNeg.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = GetConnective(rhs); });
    }

    ILanguageObject GetConnective(Symbol[] rhs) => new Connective(((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant()); 

    ILanguageObject GetAtomicSentence(Symbol[] rhs) {
        var symbol = ((LexValue)rhs[0].SyntheticAttribute).Value;

        int? timeValue = null;
        if (rhs.Length > 2)
        {
            var timeAttribute = ((LexValue)rhs[2].SyntheticAttribute).Value;
            timeValue = int.Parse(timeAttribute[1..]);
        }
            
        if (rhs[1].SyntheticAttribute != null)
        {
            var extArray = (ArrayValue)rhs[1].SyntheticAttribute;
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