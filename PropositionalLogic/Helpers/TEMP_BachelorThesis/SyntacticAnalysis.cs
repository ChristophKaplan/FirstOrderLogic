namespace PropositionalLogic.Helpers;

public class SyntacticAnalysis : Section {
    public SyntacticAnalysis(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        var fa1 = ("F_{\\alpha1}", "a", "a");
        var fa2 = ("F_{\\alpha2}", "NOT(a)", "a");
        
        var fb1 = ("F_{\\beta1}", "a AND b", "a");
        var fb2 = ("F_{\\beta2}", "NOT(a AND b)", "a");
        var fb3 = ("F_{\\beta3}", "a OR b", "a");
        var fb4 = ("F_{\\beta4}", "NOT(a OR b)", "a");
        
        var fc1 = ("F_{\\gamma1}", "a AND b", "a");
        var fc2 = ("F_{\\gamma2}", "a OR b", "a");
        
        var fd1 = ("F_{\\delta1}", "a AND (c AND d)", "a");
        var fd2 = ("F_{\\delta2}", "a AND (c OR d)", "a");
        var fd3 = ("F_{\\delta3}", "a OR (c AND d)", "a");
        var fd4 = ("F_{\\delta4}", "a OR (c OR d)", "a");

        var fe1 = ("F_{\\theta1}", "(a AND b) AND (c AND d)", "a");
        var fe2 = ("F_{\\theta2}", "(a AND b) AND (c OR d)", "a");
        var fe3 = ("F_{\\theta3}", "(a AND b) OR (c AND d)", "a");
        var fe4 = ("F_{\\theta4}", "(a AND b) OR (c OR d)", "a");
        var fe5 = ("F_{\\theta5}", "(a OR b) AND (c AND d)", "a");
        var fe6 = ("F_{\\theta6}", "(a OR b) AND (c OR d)", "a");
        var fe7 = ("F_{\\theta7}", "(a OR b) OR (c AND d)", "a");
        var fe8 = ("F_{\\theta8}", "(a OR b) OR (c OR d)", "a");
        
        var text = $"To explore the properties of our ${BachelorThesis.SkepForgetName}$ operation, we need to identify the space or dimensions in which the function operates. " +
                   "While introductory examples provided in the previous section shed light on the primary issue, " +
                   $"it remains crucial to investigate the behavior of ${BachelorThesis.SkepForgetName}$ with more complex sentences. Do the outcomes predominantly correlate " +
                   "with the top layer of connectives in the syntax tree? What influence do deeper nested expressions exert on the final result?\\\\" +
                   "To draw valuable conclusions it is necessary to select examples that can proof generality while beeing specific enough to not loose any valuable properties and characteristics." +
                   "How to identify such examples ? What criteria are essential for their selection?\\\\From the syntactical perspective we have sketched several dimensions for comparison: " +
                   "We need to examine the behavior of the two forget operations in relation — are they dual in nature or not? " +
                   "We need to consider complexity of sentences such as negation and multiple binary connectives with deeper nesting" +
                   "Comparing the forgetting of variables at different depths, for instance, for a sentence such as (a or b or c) we have three options.\\\\" +
                   "Lets start our investigation with the following sentences:\\\\";

        
        text += ForgetComparerLaTexWriter(fa1,fa2);
        text += "In $F_{\\alpha1}$ and $F_{\\alpha2}$ we forget the whole sentence itself where $F_{\\alpha2}$ is negation of $F_{\\alpha1}$." + 
                $"In both cases the result is $\\top$ for $Forget$ and $\\bot$ for ${BachelorThesis.SkepForgetName}$ as if it would characterize the function fundamentally."+
                "Lets see what happens with more complex sentences.\\\\";
        
        text += ForgetComparerLaTexWriter(fb1,fb2,fb3,fb4);
        text += @"Here we can see that $Forget(F_{\beta1},a) \equiv \neg(SkepForget(F_{\beta2},a$))\\";
        text += @"With $F_{\beta1} \equiv \neg F_{\beta2}$ we get $Forget(F_{\beta1},a) \equiv \neg SkepForget(\neg F_{\beta1},a)$"+
                @"$\neg F_{\beta1} \equiv F_{\beta2}$ and $Forget(\neg F_{\beta2},a) \equiv \neg SkepForget(F_{\beta2},a)$\\";
        text += @"More generaly we can establish a relation that: $Forget(F,a) \equiv \neg SkepForget(\neg F,a)$ and $Forget(\neg F,a) \equiv \neg SkepForget(F,a)$\\"+
                "Lets focus more on the logical connectives:\\\\";
        
        text += ForgetComparerLaTexWriter(fc1,fc2);
        text += "In $F_{\\beta1}$ and $F_{\\beta2}$ we forget only one variable of a binary connective. Since logical connectives are commutative, forgetting the other variable would not give us new information." + 
                $"We observe a dualistic behavior. Our forget operations deliver either truth values or the remaining variable $b$ of the original sentence." +
                $"A necessary condition seems to be the connective itself. We can observe that Forget with $\\land$ gives us $\\bot$ where in combination with $\\lor$ we receive $b$. " +
                $"For {BachelorThesis.SkepForgetName} its the other way arround, also the truth value is $\\top$ wich is the opposite"+
                "Bottom line, with the 4 results, we loose exactly the variable, wich we expect to loose. And in the other two cases we loose more information thatn we expect" +
                "Lets see if this holds true if we look are more complex/longer sentences:\\\\";
        
        text += ForgetComparerLaTexWriter( fd1, fd2, fd3, fd4);
        text += "If we add more complexity via one additional connective." +
                "It seems that, due to the inductive or recursive structure of propositional logic we still get the same dualistic result as in the previous example. " +
                "As in the previous example, in half of the cases we loose exactly what we expect, and in the other half we loose more information." +
                "Just to be on the safe side, lets see if it still holds when we add one more variable\\\\";
        
        text += ForgetComparerLaTexWriter( fe1, fe2, fe3, fe4, fe5, fe6, fe7, fe8);
        text += "Surprisingly we dont get exactly the same behaviour as before. The dualistic overall picture remains." +
                "But for $F_{\\theta5}$ and $F_{\\theta6}$ on the Forget side and $F_{\\theta3}$ and $F_{\\theta4}$ on the " +
                "SkepForget side the result is structurally different from what we expected compared to the previous example." +
                "Because, as in the previous example, in half of the cases we loose exactly what we want to loose but in the other half it is more differentiated. " +
                "We loose some but we dont loos as much as in the previous. But we made a little mistake here:"+
                "Imagine $a$ from the previous example is now replaced with with $(a \\circ b)$, we now forget only $a$ and not $(a \\circ b)$ wich we should in order to make the same move as before" +
                "but we can see that within the half of the cases in wich we loose information again we can divide this in half and observe that we only lost half of that in drastic way, meaning reuslt is only truth values"+
                "As we can only forget atomic variables, we cant make this comparison as we would need to forget $(a \\circ b)$." +
                "We now would need to look at forgetting different variables in same sentences:" +
                "note: also a is now deeper nested vs before it was top layer\\\\";


        text += @"For a sentence $a \land (b \lor c)$ we have 3 options, but only 2 layers in depth, considering commutativity of logical connectives we can reduce this to 2 options.\\";
        
        var f1a = ("F_{\\phi1}", "a AND (b AND c)", "a");
        var f2a = ("F_{\\phi2}", "a AND (b OR c)", "a");
        var f3a = ("F_{\\phi3}", "a OR (b AND c)", "a");
        var f4a = ("F_{\\phi4}", "a OR (b OR c)", "a");
        text += ForgetComparerLaTexWriter( f1a, f2a, f3a, f4a);
        var f1c = ("F_{\\phi1}", "a AND (b AND c)", "c");
        var f2c = ("F_{\\phi2}", "a AND (b OR c)", "c");
        var f3c = ("F_{\\phi3}", "a OR (b AND c)", "c");
        var f4c = ("F_{\\phi4}", "a OR (b OR c)", "c");

        text += ForgetComparerLaTexWriter( f1c, f2c, f3c, f4c);

        text += @"taking on the perspective from the variable to forget we can see that half of the cases we loose only what we expect.\\" +
                "$a$ is in the top layer, here will get themost drastic results where the deeper nested variable $c$ is were we get the less drastic results, we loose less information\\\\" +
                "It seems that we loose information.";
                
                
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

        var t1 = "Forget(a AND (b AND c),a)";
        var t2 = "Forget(a AND (b OR c),a)";
        var t3 = "Forget(a OR (b AND c),a)";
        var t4 = "Forget(a OR (b OR c),a)";
        var t5 = "Forget(a AND (b AND c),c)";
        var t6 = "Forget(a AND (b OR c),c)";
        var t7 = "Forget(a OR (b AND c),c)";
        var t8 = "Forget(a OR (b OR c),c)";
        var treesss = MRKUPGen.Figurize(GetTableOfTreesFor(t1,t2,t3,t4,t5,t6,t7,t8),"ddddd","ssss");
        
        
        var sub1 = @"\subsection{Behavior of Truth Values in Abstract Syntax Trees}

                    The behavior of truth values presents notably insights when considering their interactions with logical connectives. 
                    We know that $\top \land \bot \equiv \bot$ and $\top \lor \bot \equiv \top$. 

                    To investigate the behaviour abstract syntax trees are a great way to visualize sentences.To get a good overview we will try any permutation of connectives over a certain variable amount $n$ wich is $2^(n-1)$ possibilities. \\" +
                   topProp +
                   @"In figure \ref{fig:topProp} we can observe the propagation of truth values, in this case $\top$. Starting from the leafes 
                    @$\top$ effectively ""absorbs"" all siblings in conjunction with $\lor$ and stops propagating at $\land$.\\" +
                   botProp +
                   @"Figure \ref{fig:botProp} shows, dual to \ref{fig:topProp}, how $\bot$ ""annihilates"" all siblings when paired with a logical $\land$ and halts at $\lor$.
                    This dynamic interaction implies that truth values possess the capability to selectively ""eliminate"" 
                    variables and entire propositions." +
                   topProp2;

        var text2 =  @"Lets say $V$ is the Set of variables in a Sentence, and  $L \in \mathbb{N} \geq 0$ is the layer in the syntax tree. 
                We can denote the possible amount of truth values for any sentence over all possible connectives as $ TVA = 2^{|V|-1}/2^L$,\\ the partially lost information as $PL = (2^{|V|-1}/2)-TVA$\\ and the fully kept information as $K = (2^{|V|-1}/2)$";

        return text + sub1 +text2 +treesss;
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