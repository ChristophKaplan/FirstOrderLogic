using System.Text;

namespace PropositionalLogic.Helpers;


public class ForgetCompare {
    private PropositionalLogic logic = new ();
    private Sentence Formula;
    private string forgetVar;
    private Sentence Forget;
    private Sentence SkepForget;
    private Sentence SimpForget;
    private Sentence SimpSkepForget;
    private InterpretationSet IntForget;
    private InterpretationSet IntSkepForget;  
    
    public ForgetCompare(string formula, string forgetVar) {
        this.forgetVar = forgetVar;
        Formula = (Sentence)logic.TryParse($"{formula}");
        Forget = (Sentence)logic.TryParse($"Forget({formula}, {forgetVar})");
        SkepForget = (Sentence)logic.TryParse($"SkepForget({formula}, {forgetVar})");
        SimpForget = (Sentence)logic.TryParse($"Simplify(Forget({formula}, {forgetVar}))");
        SimpSkepForget = (Sentence)logic.TryParse($"Simplify(SkepForget({formula}, {forgetVar}))");
        IntForget = (InterpretationSet)logic.TryParse($"Int({formula}, Forget({formula}, {forgetVar}))");
        IntSkepForget = (InterpretationSet)logic.TryParse($"Int({formula}, SkepForget({formula}, {forgetVar}))");
        
        IntForget.SortBy(forgetVar);
        IntSkepForget.SortBy(forgetVar);
    }
    
    public string ToHTML() {
        string[] cols = new []{$"{Formula} forgetting {forgetVar}"};
        
        string[][] rows = { 
            new []{$"Forget = {Forget} = {SimpForget}", $"SkepForget = {SkepForget} = {SimpSkepForget}"},
            new []{$"{IntForget.ToHTML()}",$"{IntSkepForget.ToHTML()}"},
            new []{$"{IntForget.Analyze()}",$"{IntSkepForget.Analyze()}"},
        };
        
        return MarkUpGenerator.ToHTMLTable((cols,rows));
    }
    
    public string ToLatex() {

        return MarkUpGenerator.FigureCompare(IntForget.ToLaTex(false), IntSkepForget.ToLaTex(false));
        
        string[] cols = new []{$"{Formula} forgetting {forgetVar}", "x"};
        
        string[][] rows = { 
            new []{$"Forget = {Forget} = {SimpForget}", $"SkepForget = {SkepForget} = {SimpSkepForget}"},
            new []{$"{IntForget.ToLaTex(false)}",$"{MarkUpGenerator.ReplaceUnicodeToLaTex(IntForget.Analyze())}"},
            new []{$"{IntSkepForget.ToLaTex(false)}",$"{MarkUpGenerator.ReplaceUnicodeToLaTex(IntSkepForget.Analyze())}"},
        };
        
        return MarkUpGenerator.ToLaTexTable((cols,rows));
    }
}