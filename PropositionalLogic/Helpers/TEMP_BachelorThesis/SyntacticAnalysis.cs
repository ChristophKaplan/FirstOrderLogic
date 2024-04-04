namespace PropositionalLogic.Helpers;

public class SyntacticAnalysis : Section {
    public SyntacticAnalysis(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        var f0 = ("F_1", "a", "a");
        var f1 = ("F_2", "a AND b", "a");
        var f2 = ("F_3", "(x OR y) AND (w OR z)", "x");

        var text = "what are good examples to start of with the semantical analysis ? \n " +
                   "When does deeper nesting affect the result of the semantical analysis ? \n" +
                   ForgetLaTexWriter(false, true, f0, f1, f2);


        var allTrees = MarkUpGenerator.Figurize(GetTableOfTreesFor(InputCreator.GeneratePropositionalSentences(3).ToArray()));
        var allTrees2 = MarkUpGenerator.Figurize(GetTableOfTreesFor(InputCreator.GeneratePropositionalSentences(3, LogicalConstant.LSymbol.FALSE).ToArray()));

        //GetEquationsOfEquivalence(InputCreator.GeneratePropositionalSentences(3).ToArray());

        var sub1 = @"\subsection{Behavior of Truth Values in Abstract Syntax Trees}
                    The behavior of truth values presents notably insights when considering their interactions with logical connectives. 
                    We know that $\top \land \bot \equiv \bot$ and $\top \lor \bot \equiv \top$. 
                    To investigate the behaviour abstract syntax trees are a great way to visualize sentences.\\" + allTrees +
                    @"In these examples we can observe the propagation of truth values, in this case $\top$. Starting from the leafes 
                    @$\top$ effectively ""absorbs"" all siblings in conjunction with $\lor$ and stops propagating at $\land$.\\" + allTrees2 +
                    @"Dual to that, $\bot$ ""annihilates"" all siblings when paired with a logical $\land$ and halts at $\lor$.
                    This dynamic interaction implies that truth values possess the capability to selectively ""eliminate"" 
                    variables and entire propositions.";
        return text + sub1;
    }

    string GetTableOfTreesFor(params string[] sentences) {
        int columns = 4; // Set the number of columns
        int rows = ((sentences.Length + columns - 1) / columns) * 2; // Calculate the number of rows
        string[][] array = new string[rows][]; // Create the 2D array

        for (var i = 0; i < sentences.Length; i++) {
            var parsed = (Sentence)_logic.TryParse(sentences[i]);
            var simplified = _logic.Simplify(parsed, out var steps);
            var tree = MarkUpGenerator.SentenceToForest(parsed);

            int row = (i / columns) * 2;
            int column = i % columns;

            array[row] ??= new string[columns];
            array[row + 1] ??= new string[columns];

            array[row][column] = tree;
            array[row + 1][column] = MarkUpGenerator.ReplaceUnicodeToLaTex($"\\tiny {parsed} $\\equiv$ {simplified}", true);
        }

        return MarkUpGenerator.ToLaTexTable((new string[columns], array), new MarkUpGenerator.TkizMarkerManager(), "white");
    }

    string GetEquationsOfEquivalence(params string[] input) {
        List<string[]> equations = new();
        input.ToList()
            .ForEach(x => {
                var unfoldEquivalence = _logic.UnfoldEquivalence((Sentence)_logic.TryParse(x), true);
                equations.Add(unfoldEquivalence.Select(eq => MarkUpGenerator.ReplaceUnicodeToLaTex(eq.ToString())).ToArray());
            });

        return MarkUpGenerator.LaTexEquations(equations);
    }
}