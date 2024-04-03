namespace PropositionalLogic.Helpers;



public abstract class Section {
    public string Name;
    protected readonly PropositionalLogic _logic;

    protected Section(PropositionalLogic logic) {
        _logic = logic;
        Name = GetType().Name;
    }
    
    public string ToLaTex() {
        return $"\\section{{{Name}}}\n" + Content();
    }
    protected abstract string Content();
}

public class BachelorThesis {
    private PropositionalLogic logic = new ();
    private List<Section> Sections = new ();
    private void GeneratePDF() => MarkUpGenerator.ExportLaTex("AIG_bachelorthesis.tex");
    public BachelorThesis() {
        Sections.Add(new Preliminaries(logic));
        Sections.Add(new GoodExamples(logic));
        Sections.Add(new Section01(logic));
        
        WriteSections();
        WriteThesisContent();
        GeneratePDF();
    }

    private void WriteSections() {
        foreach (var section in Sections) {
            InputOutput.WriteFile($"{section.Name}.tex",section.ToLaTex(),MarkUpGenerator.latexPath);
        }
    }
    
    private void WriteThesisContent() {
        InputOutput.WriteFile("Thesis_Content.tex",GenThesisContent(),MarkUpGenerator.latexPath);
    }
    private string GenThesisContent() {
        string result = "";
        foreach (var section in Sections) {
            result += $"\\input{{{section.Name}}}";
        }
        return result;
    }
}
