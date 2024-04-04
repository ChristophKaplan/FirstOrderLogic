namespace PropositionalLogic.Helpers;


public class SemanticInvestigation : Section {
    
    public SemanticInvestigation(PropositionalLogic logic):base(logic) { }

    protected override string Content() {
        
        var f1 = "(A OR B) AND C";
        var f2 = "(A AND B) OR C";
        
        string forgetVar = "A";
        ForgetCompare fc1 = new ForgetCompare(f1, forgetVar);
        ForgetCompare fc2 = new ForgetCompare(f2, forgetVar);

        forgetVar = "C";
        ForgetCompare fc3 = new ForgetCompare(f1, forgetVar);
        ForgetCompare fc4 = new ForgetCompare(f2, forgetVar);


        var m = $"$Mod(Forget(F, x)) = Mod(F) \\cup \\{{Switch(w, x) | w \\models F\\}}$ \\\\\n " +
                $"$Mod({BachelorThesis.SkepForgetName}(F, x)) = Mod(F) \\cap \\{{Switch(w, x) | w \\models F\\}}$";
        
        return fc1.ToLatex() + fc2.ToLatex() + fc3.ToLatex() + fc4.ToLatex() + m;
    }
}
