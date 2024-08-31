using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace DebugLang;

public enum Terminal
{
    Open,
    Comma,
    Close,
    Identifier,
}

public enum NonTerminal
{
    LangObject,
    A,
    B,
    C
}

public class DebugLang : Language<Terminal, NonTerminal>
{
    protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
    {
        return new[]
        {
            new TokenDefinition<Terminal>(Terminal.Open, "\\("),
            new TokenDefinition<Terminal>(Terminal.Comma, ","),
            new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
            new TokenDefinition<Terminal>(Terminal.Identifier, "[a-zA-Z]+"),
        };
    }

    protected override void SetUpGrammar()
    {
        var ruleStart = AddProductionRule(SpecialNonTerminal.Start, NonTerminal.LangObject);
        var ruleLangObj = AddProductionRule(NonTerminal.LangObject, NonTerminal.A);
        
        //var rule1 = AddProductionRule(NonTerminal.A, NonTerminal.B);
        var rule2 = AddProductionRule(NonTerminal.A, NonTerminal.B, NonTerminal.C);
        var rule3 = AddProductionRule(NonTerminal.B, Terminal.Identifier);
        var rule4 = AddProductionRule(NonTerminal.C, Terminal.Open, Terminal.Close);
        var rule5 = AddProductionRule(NonTerminal.C, SpecialTerminal.Epsilon);
        
        ruleStart.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        ruleLangObj.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
    }
    
    public override ILanguageObject TryParse(string input)
    {
        var langObj = base.TryParse(input);
        return langObj;
    }
}