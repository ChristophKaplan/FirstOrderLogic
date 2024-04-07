namespace PropositionalLogic.Helpers;

public class SemanticInvestigation : Section {
    public SemanticInvestigation(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        var intro = @"In \cite{lang2003propositional}, the authors denote two semantic functions, Force and Switch, as follows:
Given in interpretation w and a literal l, we let Force(w,l) denote the interpretation that gives the same truth value as w 
for all variables except the variable of l, and such that Force(w,l) $\models$ l. Meaning Force(w,l) is the interpretation that 
satisfying l that is closest to w, If w $\models$ l then Force(w,l)=w.\\ $Switch(w, l)$ denotes the interpretation that maintains 
the same truth value as $w$ for all variables except $x$, just like Force, but assigns to $x$ the opposite truth value given by $w$.";

        return intro + PropertiesOfForget() + PropertiesOfSkepForget();
    }

    string PropertiesOfForget() {
        var linreiter = @"In \cite{lin1994forget} the authors state the following properties of Forget:\\" +
                        MRKUPGen.Itemizer(@"$T \models forget(T,p)$",
                            @"For any sentence $\phi$, $T \models \phi \downarrow p iff forget(T;p) \models \phi \downarrow p$ (works for FOL, not pl ???)");

        var isberner = @"\cite{eiter2019brief}\\" + MRKUPGen.Itemizer(@"$M_1 \sim_p M_2 , M_1 and M_2$ agree on all variables except possibly $p$");

        var lang2003 = @"In \cite{lang2003propositional} the authors state the following properties of Forget:\\" +
                       MRKUPGen.Itemizer(
                           @"Proposition 14: The set of models of $ForgetLit(\\Sigma,\{ l \})$ " +
                           @"can be expressed as:\\" +
                           @"Corollary 5: $Mod(ForgetLit(\Sigma,\{l\})) = Mod(\Sigma) \cup \{Force(w,-l) | w \models \Sigma\} = \{w | Force(w,l) \models \Sigma\}$",
                           @"$Mod(Forget(F, x)) = Mod(F) \cup \{Switch(w, x) | w \models F\}$\\");


        var mine = @"$Int_{\Sigma_2}(Forget(F,a)) = Mod_{\Sigma_1}(a)$\\" +
                   @"$\forall w : w \models a, w(Forget(F,a)) = w(F)$\\" +
                   @"$\forall w : w \not\models a, w(Forget(F,a)) = Force(w,A)(F)$\\";

        return @"\\\subsection{Properties of Forget}" + linreiter + isberner + lang2003 + mine;
    }

    string GetSignatureLatex(Sentence F, string name) {
        string result = $"\\Sigma_{{{name}}}\\{{";
        var list = F.GenerateSignature();
        for (var i = 0; i < list.Count-1; i++) {
            result += $"{list[i]}, ";
        }
        return result +list[^1]+ @"\}";
    }

    string PropertiesOfSkepForget() {
        InterpretationSet overview = (InterpretationSet)_logic.TryParse("Int(NOT(a), a OR b, a AND b, SkepForget(NOT(a),a), SkepForget(a OR b,a), SkepForget(a AND b,a))");

        var baseCase = SwitchProofer("a", "a","1");
        var negationCase = SwitchProofer("NOT(a)", "a","1");
        var orCase = SwitchProofer("a OR b", "a","2");
        var andCase = SwitchProofer("a AND b", "a","2");

        
        var switchPropos =
            $"Proposition: $Mod({{{BachelorThesis.SkepForgetName}}}(F, x)) = Mod(F) \\cap \\{{Switch(w, x) | w \\models F\\}}$ \\\\\\\\\n ";
        var switchProof =
            @"Let F be a propositional sentence, over a signature $\Sigma=\{a,b\}$, a propositional variable $x$ $\in \Sigma$ and w be an interpretation.\\" +
            @"\\Using structural induction, we can prove that the proposition holds. We consider the following cases:\\
            \begin{enumerate}
        \item \textbf{Base Case:} F is a propositional atomic variable.\\" +
            baseCase +
            @"\item \textbf{Negation Case:}\\" +
            negationCase +
            @"\item \textbf{$\lor$ Case:}\\" +
            orCase +
            @"\item \textbf{$\land$ Case:}\\" +
            andCase +
            @"\end{enumerate}" +
            
            MRKUPGen.SideBySideTables((MRKUPGen.ToLaTexTable(overview.ToTable(), new MRKUPGen.TkizMarkerManager()), "reference"));

        return @"\\\subsection{Properties of SkepForget}" + MRKUPGen.ReplaceUnicodeToLaTex(switchPropos) + switchProof;
    }

    string SwitchProofer(string sen, string forgetMe, string sigName) {
        Sentence F = (Sentence)_logic.TryParse(sen);
        var Fstring = MRKUPGen.ReplaceUnicodeToLaTex(F.ToString());
        var sig = F.GenerateSignature();
        var signature1 = GetSignatureLatex(F, sigName);
        Sentence ForgetFa = (Sentence)_logic.TryParse($"{{{BachelorThesis.SkepForgetName}}}({sen},{forgetMe})");
        InterpretationSet intF = _logic.Int(sig, F);
        InterpretationSet modForgetFa = _logic.Mod(sig, ForgetFa);
        InterpretationSet modF = _logic.Mod(sig, F);
        InterpretationSet switchModFa = _logic.SwitchAll(modF, new AtomicSentence(forgetMe));
        InterpretationSet interModFSwitch = _logic.Intersection(modF, switchModFa);
        
        return MRKUPGen.SemanticEquation(
            ($"Mod_{{\\Sigma_{sigName}}}({Fstring})", modF),
            ("Switch(w," + forgetMe + $@") | w \models {Fstring}",switchModFa),
            ($"Mod_{{\\Sigma_{sigName}}}({{{BachelorThesis.SkepForgetName}}}({Fstring}))",modForgetFa),
            ($"Mod(F)_{{\\Sigma_{sigName}}} \\cap \\{{Switch(w, "+forgetMe+$") | w \\models {Fstring}\\}}",interModFSwitch)
            ) + $"\\\\with ${signature1}$";
    }

    string ComparisionForgetSkeptForget() {
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

        return @"\subsection{Comparing Forget and SkeptForget}" + compares;
    }
}