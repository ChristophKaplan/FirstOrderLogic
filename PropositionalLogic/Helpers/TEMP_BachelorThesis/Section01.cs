namespace PropositionalLogic.Helpers;


public class Section01 : Section {
    
    public Section01(PropositionalLogic logic):base(logic) { }

    protected override string Content() {
        var f1 = "(A OR B) AND C";
        var f2 = "(A AND B) OR C";
        
        string forgetVar = "A";
        ForgetCompare fc1 = new ForgetCompare(f1, forgetVar);
        ForgetCompare fc2 = new ForgetCompare(f2, forgetVar);

        forgetVar = "C";
        ForgetCompare fc3 = new ForgetCompare(f1, forgetVar);
        ForgetCompare fc4 = new ForgetCompare(f2, forgetVar);

        //HTMLGen.Export("forgetCompare.html", fc1.ToHTML() + "<br>" + fc2.ToHTML() + "<br>" + fc3.ToHTML() + "<br>" + fc4.ToHTML() );

        //_logic.Interpret($"Int({f1}, Forget({f1},{forgetVar}))");
        
        return MarkUpGenerator.SentenceToForest((Sentence)_logic.TryParse(f1));
    }
}
