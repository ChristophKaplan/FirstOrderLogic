namespace PropositionalLogic.Helpers;

public class SemanticInvestigation : Section {
    public SemanticInvestigation(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        var f1 = "(a AND b) AND (c AND d)";
        var f2 = "(a AND b) OR (c AND d)";
        var f3 = "(a OR b) AND (c AND d)";
        var f4 = "(a OR b) OR (c AND d)";
        var f5 = "(a AND b) AND (c OR d)";
        var f6 = "(a AND b) OR (c OR d)";
        var f7 = "(a OR b) AND (c OR d)";
        var f8 = "(a OR b) OR (c OR d)";

        string forgetVar = "a";
        ForgetCompare fc1 = new ForgetCompare(f1, forgetVar);
        ForgetCompare fc2 = new ForgetCompare(f2, forgetVar);
        ForgetCompare fc3 = new ForgetCompare(f3, forgetVar);
        ForgetCompare fc4 = new ForgetCompare(f4, forgetVar);
        ForgetCompare fc5 = new ForgetCompare(f5, forgetVar);
        ForgetCompare fc6 = new ForgetCompare(f6, forgetVar);
        ForgetCompare fc7 = new ForgetCompare(f7, forgetVar);
        ForgetCompare fc8 = new ForgetCompare(f8, forgetVar);
        forgetVar = "c";

        var compares = fc1.ForgetAndSkepCompareToLatex() +
                       fc2.ForgetAndSkepCompareToLatex() +
                       fc3.ForgetAndSkepCompareToLatex() +
                       fc4.ForgetAndSkepCompareToLatex() +
                       fc5.ForgetAndSkepCompareToLatex() +
                       fc6.ForgetAndSkepCompareToLatex() +
                       fc7.ForgetAndSkepCompareToLatex() +
                       fc8.ForgetAndSkepCompareToLatex();

        var compares2 = fc1.ForgetCompareToLatex() + fc2.ForgetCompareToLatex();

        var t =
            @"In \cite{lang2003propositional}, the authors denote two semantic functions, Force and Switch, as follows:
Given in interpretation w and a literal l, we let Force(w,l) denote the interpretation that gives the same truth value as w 
for all variables except the variable of l, and such that Force(w,l) \models l. Meaning Force(w,l) is the interpretation that 
satisfying l that is closest to w, If w \models l then Force(w,l)=w.\\ $Switch(w, l)$ denotes the interpretation that maintains 
the same truth value as $w$ for all variables except $x$, just like Force, but assigns to $x$ the opposite truth value given by $w$.";
        
        
        var mmodsachen = $"$Mod(Forget(F, x)) = Mod(F) \\cup \\{{Switch(w, x) | w \\models F\\}}$ \\\\\n " +
                         $"$Mod({BachelorThesis.SkepForgetName}(F, x)) = Mod(F) \\cap \\{{Switch(w, x) | w \\models F\\}}$" +
                         $"Proposition 14: The set of models of ForgetLit(\\Sigma,{{l}}) can be expressed as:\n\nMod(ForgetLit(\\Sigma,{{l}})) = Mod(\\Sigma) U {{Force(w,-l) | w|= \\Sigma}}\n= {{w | Force(w,l) |= \\Sigma}}";
        var text = @"$Int_{\Sigma_2}(Forget(F,a)) = Mod_{\Sigma_1}(a)$\\" +
                   @"$\forall w w \models a, w(Forget(F,a)) = w(F)$\\" +
                   @"$\forall w w \not\models a, w(Forget(F,a)) = Force(w,A)(F)$\\";
        
        
        InterpretationSet intab = (InterpretationSet)_logic.TryParse($"Int(a AND b)");
        InterpretationSet intabForce = intab.ForceAll(new AtomicSentence("a"));
        InterpretationSet intabSwitch = intab.SwitchAll(new AtomicSentence("a"));
        
        var tab = MRKUPGen.ToLaTexTable(intab.ToTable(), new MRKUPGen.TkizMarkerManager()) +@"\\"+
                  MRKUPGen.ToLaTexTable(intabForce.ToTable(), new MRKUPGen.TkizMarkerManager()) +@"\\"+
                  MRKUPGen.ToLaTexTable(intabSwitch.ToTable(), new MRKUPGen.TkizMarkerManager());
        
        return compares2 + mmodsachen + text +@"\\"+ tab;
    }
}