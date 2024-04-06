using System.Numerics;

namespace PropositionalLogic.Helpers;


public class ForgetCompare {
    private PropositionalLogic logic = new ();

    private string formula;
    private string forgetVar;
    private string forgetFunc ;
    private string skepForgetFunc;
    
    private InterpretationSet IntForget;
    private InterpretationSet IntSkepForget;  
    
    public ForgetCompare(string formula, string forgetVar) {
        this.formula = formula;
        this.forgetVar = forgetVar;
        forgetFunc = $"Forget({formula}, {forgetVar})";
        skepForgetFunc = $"{BachelorThesis.SkepForgetName}({formula}, {forgetVar})";
        
        IntForget = (InterpretationSet)logic.TryParse($"Int({formula}, {forgetFunc})");
        IntSkepForget = (InterpretationSet)logic.TryParse($"Int({formula}, {skepForgetFunc})");
        
        IntForget.SortBy(forgetVar);
        IntSkepForget.SortBy(forgetVar);
    }
    
    public string ForgetCompareToLatex() {
        InterpretationSet IntForget2 = (InterpretationSet)logic.TryParse($"Int({forgetFunc})");
        var f = MRKUPGen.ReplaceUnicodeToLaTex(forgetFunc, true);
        var altSenCol1 = new List<string>() {MRKUPGen.ReplaceUnicodeToLaTex(formula, true), f };
        var altSenCol2 = new List<string>() { f };
        return MRKUPGen.FigureCompare(ForgetToLaTex(IntForget.ToTable(altSenCol1)), ForgetToLaTex(IntForget2.ToTable(altSenCol2)), $"{f}", $"{f}",forgetFunc);
    }
    
    public string ForgetAndSkepCompareToLatex() {
        var f = MRKUPGen.ReplaceUnicodeToLaTex(forgetFunc, true);
        var sf = MRKUPGen.ReplaceUnicodeToLaTex(skepForgetFunc, true);
        
        var altSenCol = new List<string>() {
            "A",
            f,
        };
        
        var altSenCol2 = new List<string>() {
            "A",
            sf,
        };
        
        return MRKUPGen.FigureCompare(ForgetToLaTex(IntForget.ToTable(altSenCol)), ForgetToLaTex(IntSkepForget.ToTable(altSenCol2),true), $"{f}", $"{sf}",forgetFunc);
    }
    
    private string ForgetToLaTex((string[] col, string[][] rows) table, bool isSkepForget = false){
        var length = table.rows[0].Length-1;
        var height = table.rows.Length-1;
        
        int forgetVarPos = IntForget.FindPosInSignature(forgetVar);
        int sentencePos = IntSkepForget.GetSignature().Count;
        int forgetSentencePos= IntSkepForget.GetSignature().Count+1;
        
        var manager = new MRKUPGen.TkizMarkerManager();
        manager.AddMarker(new Vector2(forgetVarPos, 0), new Vector2(forgetVarPos, height), "line", "red");
        
        if (isSkepForget) {
            manager.AddMarker(new Vector2(0, 0), new Vector2(0, MathF.Floor(height/2f)), "rectangle", "cyan", 0.25f);
            manager.AddMarker(new Vector2(sentencePos, 0), new Vector2(sentencePos, MathF.Floor(height/2f)), "rectangle", "cyan", 0.25f);
        }
        else {
            manager.AddMarker(new Vector2(0, MathF.Floor(height/2f)+1), new Vector2(0, height), "rectangle", "cyan", 0.75f);
            manager.AddMarker(new Vector2(sentencePos, MathF.Floor(height/2f)+1), new Vector2(sentencePos, height), "rectangle", "cyan", 0.25f);
        }
        
        manager.AddMarker(new Vector2(forgetSentencePos, MathF.Floor(height/2f)+1), new Vector2(forgetSentencePos, height), "rectangle", "cyan", 0.25f);
        manager.AddMarker(new Vector2(forgetSentencePos, 0), new Vector2(forgetSentencePos, MathF.Floor(height/2f)), "rectangle", "cyan", 0.25f);
        
        return MRKUPGen.ToLaTexTable(table, manager);
    }
}