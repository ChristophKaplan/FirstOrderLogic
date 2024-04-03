using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace PropositionalLogic;

public enum Terminal {
    Function,
    Open,
    Comma,
    Close,
    Negation,
    AtomicSentence,
    Implication,
    Disjunction,
    Conjunction,
    TruthValue,
}

public enum NonTerminal {
    LangObject, Sentence, ComplexSentence, Ext, Connective
}

public class PropositionalLogic : Language<Terminal, NonTerminal> {
    public PropositionalLogic() {}

    protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions() {
        return new [] {
            new TokenDefinition<Terminal>(Terminal.Function, "Mod|Forget|SkepForget|MyForget|Int|Simplify|SwitchMany"),
            new TokenDefinition<Terminal>(Terminal.Open, "\\("),
            new TokenDefinition<Terminal>(Terminal.Comma, ","),
            new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
            new TokenDefinition<Terminal>(Terminal.Conjunction, "AND|&&"),
            new TokenDefinition<Terminal>(Terminal.Disjunction, "OR|\\|\\|"),
            new TokenDefinition<Terminal>(Terminal.Implication, "IMPLIES|=>"),
            new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!|-"),
            new TokenDefinition<Terminal>(Terminal.TruthValue, "TRUE|FALSE"),
            new TokenDefinition<Terminal>(Terminal.AtomicSentence, "[A-Z][a-z]*") 
        };
    }

    protected override void SetUpGrammar() {

        var rule01 = AddProductionRule(SpecialNonTerminal.Start, NonTerminal.LangObject);
        
        var rule02 = AddProductionRule(NonTerminal.LangObject, NonTerminal.Sentence);
        var rule03 = AddProductionRule(NonTerminal.Sentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        var rule04 = AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        var rule05 = AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        
        var rule06 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, NonTerminal.Connective, NonTerminal.Sentence);
        var rule07 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, NonTerminal.Connective, NonTerminal.Sentence);
        var rule08 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);

        var rule09 = AddProductionRule(NonTerminal.LangObject, Terminal.Function, Terminal.Open, NonTerminal.LangObject, NonTerminal.Ext);
        var rule10 = AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.LangObject, NonTerminal.Ext);
        var rule11 = AddProductionRule(NonTerminal.Ext, Terminal.Close);
        
        var rule12 = AddProductionRule(NonTerminal.Connective, Terminal.Conjunction);
        var rule13 = AddProductionRule(NonTerminal.Connective, Terminal.Disjunction);
        var rule14 = AddProductionRule(NonTerminal.Connective, Terminal.Implication);
        
        //fix this in order
        var rule15 = AddProductionRule(NonTerminal.Sentence, Terminal.TruthValue); //weird?
        var rule16 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.TruthValue, NonTerminal.Connective, NonTerminal.Sentence);
        rule15.SetSemanticAction((lhs, rhs) => {
            var lc = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant();
            lhs.SyntheticAttribute = new AtomicSentence(lc.ToString());
        });
        rule16.SetSemanticAction((lhs, rhs) => {
            var lc = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant();
            lhs.SyntheticAttribute = new ComplexSentence(new AtomicSentence(lc.ToString()), (LogicalConstant)rhs[1].SyntheticAttribute, (Sentence)rhs[2].SyntheticAttribute);
        });
        
        
        
        rule01.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule02.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule03.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        rule04.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute); });

        rule05.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });

        rule06.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new ComplexSentence(new AtomicSentence((LexValue)rhs[0].SyntheticAttribute), (LogicalConstant)rhs[1].SyntheticAttribute, (Sentence)rhs[2].SyntheticAttribute);
        });

        rule07.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, (LogicalConstant)rhs[3].SyntheticAttribute, (Sentence)rhs[4].SyntheticAttribute);
        });

        rule08.SetSemanticAction((lhs, rhs) => lhs.SyntheticAttribute = new ComplexSentence(LogicalConstant.LSymbol.NOT, (Sentence)rhs[1].SyntheticAttribute));

        rule09.SetSemanticAction((lhs, rhs) => {
            var extArray = (ArrayValue)rhs[3].SyntheticAttribute;
            extArray.Insert(rhs[2].SyntheticAttribute,0);

            for (var i = 0; i < extArray.Value.Length; i++) {
                if (extArray.Value[i] is Function function) {
                    extArray.Value[i] = ExecuteFunction(function);
                }
            }

            lhs.SyntheticAttribute = new Function((LexValue)rhs[0].SyntheticAttribute, extArray.Value);
        });
        
        rule10.SetSemanticAction((lhs, rhs) => {
            var second = rhs[1].SyntheticAttribute;
            var ext = (ArrayValue)rhs[2].SyntheticAttribute;
            ext.Add(second);
            lhs.SyntheticAttribute = ext;
        });
        
        rule11.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new ArrayValue(Array.Empty<ILanguageObject>());
        });
        
        rule12.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant(); });
        rule13.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant(); });
        rule14.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = ((LexValue)rhs[0].SyntheticAttribute).ToLogicalConstant(); });
    }

    private ILanguageObject ExecuteFunction(Function function) {
        switch (function.Func) {
            case "Int": {
                Sentence[] a = new Sentence[function.Parameters.Length];
                for (var i = 0; i < function.Parameters.Length; i++) {
                    a[i] = (Sentence)function.Parameters[i];
                }

                return this.Int(a);
            }
            case "Mod": {
                return this.Mod((Sentence)function.Parameters[0]);
            }
            case "Simplify": {
                var result = this.Simplify((Sentence)function.Parameters[0]);
                //Console.WriteLine($"Simplify: {function.Parameters[0]} equals: {result}");
                return result;
            }
            case "Forget": {
                var result = this.Forget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
            case "SkepForget": {
                var result = this.SkepForget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
            case "SwitchMany": {
                var result = this.SwitchMany((InterpretationSet)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
        }

        Console.WriteLine($"No return value for: {function.Func}");
        return null;
    }

    public List<Interpretation> GenerateInterpretations(params Sentence[] sentences) {
        var interpretations = new List<Interpretation>();
        var cleanAtoms = GetAtoms(sentences);        
        var truthTable = GenerateTruthTable(cleanAtoms.Count);
        
        foreach (var truthValues in truthTable) {
            var interpretation = new Interpretation();
            var list = truthValues.ToArray();
            for (var i = 0; i < cleanAtoms.Count; i++) {
                interpretation.Add(cleanAtoms[i], list[i]);
            }

            if (!interpretations.Contains(interpretation)) {
                interpretations.Add(interpretation);
            }
        }

        return interpretations;

        List<AtomicSentence> GetAtoms(params Sentence[] sentences) {
            var reducedAtoms = new List<AtomicSentence>();
            var collectedAtoms = new List<AtomicSentence>();
            
            foreach (var s in sentences) {
                collectedAtoms.AddRange(s.GetAtoms());
            }
            
            foreach (var atom in collectedAtoms) {
                if (atom.Verum || atom.Falsum) {
                    continue;
                }
                if(!reducedAtoms.Contains(atom)) reducedAtoms.Add(atom);
            }

            return reducedAtoms;
        }
    }

    private IEnumerable<IEnumerable<bool>> GenerateTruthTable(int n) {
        switch (n) {
            case 0:
                return Enumerable.Empty<IEnumerable<bool>>();
            case 1:
                return new List<List<bool>> { new() { true }, new() { false } };
            default: {
                var subTables = GenerateTruthTable(n - 1);
                return subTables.SelectMany(row => new[] { row.Append(true), row.Append(false) });
            }
        }
    }

    public override ILanguageObject TryParse(string input) {
        var langObj = base.TryParse(input);
        if (langObj is Function function) {
            return ExecuteFunction(function);
        }

        return langObj;
    }
    
    public void Interpret(params string[] input) {
        var output = "";
        foreach (var s in input) {
            var lo = TryParse(s);
            output += lo + "\n";
        }
        Console.WriteLine(output);
    }
}