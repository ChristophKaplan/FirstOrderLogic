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

    protected string ForgetLaTexWriter(bool isSkepForget, bool showOverview, params (string sentenceName, string sentence, string variable)[] input) {
        List<string[]> equations = new();

        var overview = string.Empty;
        if (showOverview)
            overview = MarkUpGenerator.ReplaceUnicodeToLaTex(input.Aggregate(string.Empty,
                               (current, i) => current + $"${i.sentenceName}$ = {i.sentence}\\\\\n"), true);

        foreach (var i in input) {
            var forget = isSkepForget ? $"{BachelorThesis.SkepForgetName}({i.sentence},{i.variable})" : $"Forget({i.sentence},{i.variable})";
            var replacer = $"{i.sentenceName}[{i.variable}/\\top] \\lor {i.sentenceName}[{i.variable}/\\bot]";
            var start = MarkUpGenerator.ReplaceUnicodeToLaTex($"{forget}&={replacer}\\\\");

            var unfoldEquivalence = _logic.UnfoldEquivalence((Sentence)_logic.TryParse($"Forget({i.sentence},{i.variable})"), true);

            var equivChain = new List<string>() { start };
            equivChain.AddRange(unfoldEquivalence.Select(eq => MarkUpGenerator.ReplaceUnicodeToLaTex(eq.ToString())));
            equations.Add(equivChain.ToArray());
        }

        return overview + MarkUpGenerator.LaTexEquations(equations);
    }
}

public class BachelorThesis {
    private PropositionalLogic logic = new();
    private List<Section> Sections = new();
    private void GeneratePDF() => MarkUpGenerator.ExportLaTex("AIG_bachelorthesis.tex");

    public static string SkepForgetName => "SkepForget";

    public BachelorThesis() {
        logic.Interpret("a AND b");

        Sections.Add(new Motivation(logic));
        Sections.Add(new SyntacticAnalysis(logic));
        Sections.Add(new SemanticInvestigation(logic));

        WriteSections();
        WriteThesisContent();
        GeneratePDF();
    }

    private void WriteSections() {
        foreach (var section in Sections) {
            InputOutput.WriteFile($"{section.Name}.tex", section.ToLaTex(), MarkUpGenerator.latexPath);
        }
    }

    private void WriteThesisContent() {
        InputOutput.WriteFile("Thesis_Content.tex", GenThesisContent(), MarkUpGenerator.latexPath);
    }

    private string GenThesisContent() {
        string result = "";
        foreach (var section in Sections) {
            result += $"\\input{{{section.Name}}}";
        }

        return result;
    }
}