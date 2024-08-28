using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace FirstOrderLogic;

public enum Terminal {
    Open, Comma, Close, Identifier
}

public enum NonTerminal {
    LangObject, AtomicSentence, Term, TermList, TermListExt, TermExt
}


public class FirstOrderLogic: Language<Terminal, NonTerminal>
{
    protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
    {
        return new [] {
            new TokenDefinition<Terminal>(Terminal.Open, "\\("),
            new TokenDefinition<Terminal>(Terminal.Comma, ","),
            new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
            new TokenDefinition<Terminal>(Terminal.Identifier, "[A-Z][a-z]*|[a-z]*"),
        };
    }

    protected override void SetUpGrammar()
    {
        var ruleStart = AddProductionRule(SpecialNonTerminal.Start, NonTerminal.LangObject);
        var ruleLangObj = AddProductionRule(NonTerminal.LangObject, NonTerminal.AtomicSentence);
        
        var ruleAtomicSentence = AddProductionRule(NonTerminal.AtomicSentence, Terminal.Identifier, Terminal.Open, NonTerminal.TermList, Terminal.Close);
        
        var ruleTermList = AddProductionRule(NonTerminal.TermList, NonTerminal.Term, NonTerminal.TermListExt);
        var ruleTermListExt = AddProductionRule(NonTerminal.TermListExt, Terminal.Comma, NonTerminal.Term, NonTerminal.TermListExt);
        var ruleTermListExtEnd = AddProductionRule(NonTerminal.TermListExt, SpecialTerminal.Epsilon);
        
        var ruleTermVarOrConstOrFunc = AddProductionRule(NonTerminal.Term, Terminal.Identifier, NonTerminal.TermExt);
        var ruleTermExt = AddProductionRule(NonTerminal.TermExt, Terminal.Open, NonTerminal.TermList, Terminal.Close);
        var ruleTermExtEnd = AddProductionRule(NonTerminal.TermExt, SpecialTerminal.Epsilon);

        //
        ruleStart.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleLangObj.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        
        ruleAtomicSentence.SetSemanticAction((lhs, rhs) => {
            var predicateSymbol = ((LexValue)rhs[0].SyntheticAttribute).Value;
            var extArray = (ArrayValue)rhs[2].SyntheticAttribute;
            var terms = extArray.Value.Select(lexValue => (Term)lexValue).ToArray();
            lhs.SyntheticAttribute = new AtomicSentence(predicateSymbol, terms);
        });
        
        ruleTermExt.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });
        
        ruleTermList.SetSemanticAction((lhs, rhs) => {
            var firstTerm = rhs[0].SyntheticAttribute;

            if (rhs[1].SyntheticAttribute == null)
            {
                lhs.SyntheticAttribute = new ArrayValue(firstTerm);;
                return;
            }
            
            var ext = (ArrayValue)rhs[1].SyntheticAttribute;
            ext.Add(firstTerm);
            lhs.SyntheticAttribute = ext;
        });
        
        ruleTermListExt.SetSemanticAction((lhs, rhs) => {
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
        
        ruleTermListExtEnd.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new ArrayValue(Array.Empty<ILanguageObject>());
        });
        
        ruleTermExtEnd.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        
        ruleTermVarOrConstOrFunc.SetSemanticAction((lhs, rhs) => {
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
                var isVariable = true;
                if (isVariable)
                {
                    term = new Variable(symbol);
                }
                else
                {
                    term = new Constant(symbol);
                }   
            }
            
            lhs.SyntheticAttribute = term;
        });
    }
    
    public override ILanguageObject TryParse(string input) {
        var langObj = base.TryParse(input);
        return langObj;
    }
}