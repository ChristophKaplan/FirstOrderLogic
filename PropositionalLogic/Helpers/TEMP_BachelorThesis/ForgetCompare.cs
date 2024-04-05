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
    
    public string ToLatex() {
        var f = MRKUPGen.ReplaceUnicodeToLaTex(forgetFunc, true);
        var sf = MRKUPGen.ReplaceUnicodeToLaTex(skepForgetFunc, true);
        return MRKUPGen.FigureCompare(ForgetToLaTex(), SkepForgetToLaTex(), $"{f}", $"{sf}",forgetFunc);
    }
    
    private string ForgetToLaTex(){
        var table = IntForget.ToTable();
        var length = table.rows[0].Length-1;
        var height = table.rows.Length-1;
        
        int forgetVarPos = IntForget.FindPosInSignature(forgetVar);
        var manager = new MRKUPGen.TkizMarkerManager();
        manager.AddMarker(new Vector2(forgetVarPos, 0), new Vector2(forgetVarPos, height), "line", "red");
        manager.AddMarker(new Vector2(0, MathF.Floor(height/2f)+1), new Vector2(length, height), "rectangle", "yellow", 0.25f);
        return MRKUPGen.ToLaTexTable(table, manager);
    }
    
    private string SkepForgetToLaTex(){
        var table = IntSkepForget.ToTable();
        var length = table.rows[0].Length-1;
        var height = table.rows.Length-1;

        int forgetVarPos = IntSkepForget.FindPosInSignature(forgetVar);
        var manager = new MRKUPGen.TkizMarkerManager();
        manager.AddMarker(new Vector2(forgetVarPos, 0), new Vector2(forgetVarPos, height), "line", "red");
        manager.AddMarker(new Vector2(0, 0), new Vector2(length, MathF.Floor(height/2f)), "rectangle", "yellow", 0.25f);
        return MRKUPGen.ToLaTexTable(table, manager);
    }
}