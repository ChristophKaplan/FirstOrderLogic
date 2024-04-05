namespace PropositionalLogic.Helpers;

public class SyntacticAnalysis : Section {
    public SyntacticAnalysis(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        var f1 = ("F_1", "a", "a");
        var f1b = ("F_{1b}", "NOT(a)", "a");
        
        var f2 = ("F_2", "a AND b", "a");
        var f2b = ("F_{2b}", "NOT(a AND b)", "a");
        var f3 = ("F_3", "a AND (c AND d)", "a");
        
        var f5 = ("F_5", "a OR b", "a");
        var f6 = ("F_6", "a OR (c OR d)", "a");
        
        var f7 = ("F_7", "a OR (c AND d)", "a");
        var f8 = ("F_8", "a AND (c OR d)", "a");
        
        var text = "In the previous section we gave examples of sentences with only one connective." +
                   "But what about more complex sentences. Does the\noutcome predominantly correlate with the top layer of connectives in the syntax tree?\n" +
                   "What is the effect or influence of deeper nested expressions on the final result?" +
                   "To take the investigation a step further, and also for the semantic analysis, we need to identify examples that are at least general enough to to make some valuable statements" +
                   "But how to find them, what is important ? First lets experiment with some more sentence and also deeper nesting and compare the results!\\\\";
        
        text += ForgetComparerLaTexWriter(f1,f1b, f2,f2b, f3, f5, f6, f7, f8);

        text += "Now here we can see... this and that";
        

        var topProp = MRKUPGen.Figurize(GetTableOfTreesFor(InputCreator.GeneratePropositionalSentences(3).ToArray()),
            @"$\top$ propagation",
            "topProp");
        var botProp = MRKUPGen.Figurize(
            GetTableOfTreesFor(InputCreator.GeneratePropositionalSentences(3, LogicalConstant.LSymbol.FALSE).ToArray()),
            @"$\bot$ propagation",
            "botProp");
        var topProp2 = MRKUPGen.Figurize(GetTableOfTreesFor(InputCreator.GeneratePropositionalSentences(4).ToArray()),
            @"$\top$ propagation",
            "topProp");
        //GetEquationsOfEquivalence(InputCreator.GeneratePropositionalSentences(3).ToArray());

        var sub1 = @"\subsection{Behavior of Truth Values in Abstract Syntax Trees}


                    The behavior of truth values presents notably insights when considering their interactions with logical connectives. 
                    We know that $\top \land \bot \equiv \bot$ and $\top \lor \bot \equiv \top$. 


                    To investigate the behaviour abstract syntax trees are a great way to visualize sentences.
To get a good overview we will try any permutation of connectives over a certain variable amount $n$ wich is $2^(n-1)$ possibilities. \\" +
                   topProp +
                   @"In figure \ref{fig:topProp} we can observe the propagation of truth values, in this case $\top$. Starting from the leafes 
                    @$\top$ effectively ""absorbs"" all siblings in conjunction with $\lor$ and stops propagating at $\land$.\\" +
                   botProp +
                   @"Figure \ref{fig:botProp} shows, dual to \ref{fig:topProp}, how $\bot$ ""annihilates"" all siblings when paired with a logical $\land$ and halts at $\lor$.
                    This dynamic interaction implies that truth values possess the capability to selectively ""eliminate"" 
                    variables and entire propositions." +
                   topProp2;
        return text + sub1;
    }

    string GetTableOfTreesFor(params string[] sentences) {
        int columns = 4; // Set the number of columns
        int rows = ((sentences.Length + columns - 1) / columns) * 2; // Calculate the number of rows
        string[][] array = new string[rows][]; // Create the 2D array

        for (var i = 0; i < sentences.Length; i++) {
            var parsed = (Sentence)_logic.TryParse(sentences[i]);
            var simplified = _logic.Simplify(parsed, out var steps);
            var tree = MRKUPGen.SentenceToForest(parsed);

            int row = (i / columns) * 2;
            int column = i % columns;

            array[row] ??= new string[columns];
            array[row + 1] ??= new string[columns];

            array[row][column] = tree;
            array[row + 1][column] = MRKUPGen.ReplaceUnicodeToLaTex($"\\tiny {parsed} $\\equiv$ {simplified}", true);
        }

        return MRKUPGen.ToLaTexTable((new string[columns], array), new MRKUPGen.TkizMarkerManager(), "white");
    }

    string GetEquationsOfEquivalence(params string[] input) {
        List<string[]> equations = new();
        input.ToList()
            .ForEach(x => {
                var unfoldEquivalence = _logic.UnfoldEquivalence((Sentence)_logic.TryParse(x), true);
                equations.Add(unfoldEquivalence.Select(eq => MRKUPGen.ReplaceUnicodeToLaTex(eq.ToString())).ToArray());
            });

        return MRKUPGen.LaTexEquations(equations);
    }
}