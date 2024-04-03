namespace PropositionalLogic.Helpers;

public class GoodExamples: Section{
    public GoodExamples(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {

        var text = "what are good examples to start of with the semantical analysis ? \n " +
                   "When does deeper nesting affect the result of the semantical analysis ? \n" + @"\\";
        
       
        var c = InputCreator.GeneratePropositionalSentences(2);

        var t = "";
        c.ForEach(x => {
            t += Eq((Sentence)_logic.TryParse(x));
        });

        string Eq(Sentence sentence) {
            var simplified = _logic.Simplify(sentence);
            return MarkUpGenerator.SentenceToLaTex(sentence) + @"$\equiv$" +MarkUpGenerator.SentenceToLaTex(simplified) + @"\\";
        }
        
        var f1 = "A AND B";
        var f2 = "(A OR B) AND (C OR D)";

        return text + t;
    }
}
