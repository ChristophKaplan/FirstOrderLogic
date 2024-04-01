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
        return $"F = {Formula} forgetting {forgetVar}<br> " +
               
               $"<table style=\"border: 1px solid black;\">" +
               
               $"<tr><td>Forget = {Forget} = {SimpForget}</td><td> SkepForget = {SkepForget} = {SimpSkepForget}</td></tr>" +
               $"<tr><td>{IntForget.ToHTML()}</td><td>{IntSkepForget.ToHTML()}</td></tr>" +
               $"<tr><td>{IntForget.Analyze()}</td><td>{IntSkepForget.Analyze()}</td></tr>" +
               
               $"</table>";
    }
    
}