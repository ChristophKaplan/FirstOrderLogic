using System.Data;

namespace PropositionalLogic.Helpers;

public abstract class Section {
    public string Name;
    protected readonly PropositionalLogic _logic;
    private bool IsSubsection;


    protected Section(PropositionalLogic logic) {
        _logic = logic;
        Name = GetType().Name;
        IsSubsection = false;
    }

    public string ToLaTex() {
        if (IsSubsection) return $"\\subsection{{{Name}}}\n" + Content();
        return $"\\section{{{Name}}}\n" + Content();
    }

    protected abstract string Content();
    
    protected string ForgetComparerLaTexWriter(params (string sentenceName, string sentence, string variable)[] input) {
        
        List<string[]> e1 = new();
        List<string[]> e2 = new();
        List<string[]> e3 = new();
        for (var i = 0; i < input.Length; i++) {
            var s = (Sentence)_logic.TryParse(input[i].sentence);
            var l1 = _logic.UnfoldEquivalence((Sentence)_logic.TryParse($"Forget({input[i].sentence},{input[i].variable})"));
            var l2 = _logic.UnfoldEquivalence((Sentence)_logic.TryParse($"{BachelorThesis.SkepForgetName}({input[i].sentence},{input[i].variable})"));
            
            e1.Add(new []{MRKUPGen.ReplaceUnicodeToLaTex($"{input[i].sentenceName}"),MRKUPGen.ReplaceUnicodeToLaTex(s.ToString())});
            e2.Add(new []{MRKUPGen.ReplaceUnicodeToLaTex($"Forget({input[i].sentenceName},{input[i].variable})"), MRKUPGen.ReplaceUnicodeToLaTex(l1.Last().ToString())} );
            e3.Add(new []{MRKUPGen.ReplaceUnicodeToLaTex($"{BachelorThesis.SkepForgetName}({input[i].sentenceName},{input[i].variable})"),MRKUPGen.ReplaceUnicodeToLaTex(l2.Last().ToString())} );
        }
        
        string[][] row = new string[][] {
            new string[] {
                MRKUPGen.LaTexEquations(e1,true),
                MRKUPGen.LaTexEquations(e2,true),
                MRKUPGen.LaTexEquations(e3,true)
            }
        };
        
        var table =  MRKUPGen.ToLaTexTable((new string[3],row), new MRKUPGen.TkizMarkerManager(), "white");

        return "\\begin{adjustbox}{width=\\textwidth}\n" + table + "\\end{adjustbox}\n";
    }
    
    protected string ForgetLaTexWriter(bool isSkepForget, bool showOverview, bool completeSteps, bool center, params (string sentenceName, string sentence, string variable)[] input) {
        List<string[]> equations = new();

        var overview = string.Empty;
        if (showOverview)
            overview = MRKUPGen.ReplaceUnicodeToLaTex(input.Aggregate(string.Empty,
                    (current, i) => current + $"${i.sentenceName}$ = {i.sentence}\\\\\n"),
                true);

        foreach (var i in input) {
            var forget = isSkepForget ? $"{BachelorThesis.SkepForgetName}({i.sentenceName},{i.variable})" : $"Forget({i.sentenceName},{i.variable})";
            var replacer = $"{i.sentenceName}[{i.variable}/\\top] \\lor {i.sentenceName}[{i.variable}/\\bot]";
            var start = MRKUPGen.ReplaceUnicodeToLaTex($"{forget}&={replacer}\\\\");

            var unfoldEquivalence = isSkepForget ?
                _logic.UnfoldEquivalence((Sentence)_logic.TryParse($"{BachelorThesis.SkepForgetName}({i.sentence},{i.variable})"), completeSteps) :
                _logic.UnfoldEquivalence((Sentence)_logic.TryParse($"Forget({i.sentence},{i.variable})"), completeSteps);

            List<string> equivChain = new();
            if (!completeSteps) {
                equivChain.Add($"{forget}");
                //equivChain.AddRange(unfoldEquivalence.Select(eq => MarkUpGenerator.ReplaceUnicodeToLaTex(eq.ToString())));
                equivChain.Add(MRKUPGen.ReplaceUnicodeToLaTex(unfoldEquivalence.Last().ToString()));
            }
            else {
                equivChain.Add(start);
                equivChain.AddRange(unfoldEquivalence.Select(eq => MRKUPGen.ReplaceUnicodeToLaTex(eq.ToString())));
            }
            
            equations.Add(equivChain.ToArray());
        }

        if(center) return @"\begin{center}" + overview + MRKUPGen.LaTexEquations(equations,!completeSteps) + @"\end{center}";
        return overview + MRKUPGen.LaTexEquations(equations,!completeSteps);
    }
}

public class BachelorThesis {
    private PropositionalLogic logic = new();
    private List<Section> Sections = new();
    private void GeneratePDF() => MRKUPGen.ExportLaTex("AIG_bachelorthesis.tex");

    public static string SkepForgetName => "SkepForget";

    public BachelorThesis() {

        //logic.Interpret("Forget(a AND b, b)");


        Sections.Add(new Motivation(logic));
        Sections.Add(new SyntacticAnalysis(logic));
        Sections.Add(new SemanticInvestigation(logic));

        WriteSections();
        WriteThesisContent();
        GeneratePDF();
    }

    private void WriteSections() {
        foreach (var section in Sections) {
            InputOutput.WriteFile($"{section.Name}.tex", section.ToLaTex(), MRKUPGen.latexPath);
        }
    }

    private void WriteThesisContent() {
        InputOutput.WriteFile("Thesis_Content.tex", GenThesisContent(), MRKUPGen.latexPath);
    }

    private string GenThesisContent() {
        string result = "";
        foreach (var section in Sections) {
            result += $"\\input{{{section.Name}}}";
        }

        return result;
    }
}