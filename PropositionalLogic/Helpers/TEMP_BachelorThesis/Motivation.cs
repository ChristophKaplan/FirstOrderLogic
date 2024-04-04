namespace PropositionalLogic.Helpers;

public class Motivation : Section {
    public Motivation(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        
        var F1 = ("F_1", "a AND b", "b");
        var F2 = ("F_2", "a OR b", "b");
        
        var intro1 =
            @"Propositional logic deals, among other things, with the satisfiability of sentences or the inference of sentences from a set of premises. 
                A knowledge base may contain many different statements in various respects. 
                To assert or entail sentences about specific phenomena or interest, it would be advantageous to work only with relevant knowledge to reduce computational complexity. 
                In other words, non-relevant or irrelevant information in relation to the current interest could be ""forgotten"". 
                Furthermore, knowledge bases change over time, and old knowledge might need to be contracted. 
                To this end, an operation exists for eliminating variables within a proposition. 
                As stated in \cite{lin1994forget} or \cite{lang2003propositional}, we define variable forgetting inductively as follows:" +
            SkepForgetDefinition() +
            @"For some formulas $F$, this operation of variable elimination can lead to a notable reductive or drastic results. 
                We start by considering an example where $\mathit{Forget}$ behaves quite appropriate. 
                Let a sentence $F_1$ be $a \land b$ and we choose to forget about $b$:" +
            ForgetLaTexWriter(false,false,F1)+
            @"We can see, as shown above, that $\mathit{Forget(F_1,b)}$ is equivalent to $a$ which seems to be the intuitive result when thinking of forgetting about $b$ in $F_1$. 
                    Next, we consider forgetting of $b$ in the formula $F_2 = a \lor b$" +
            ForgetLaTexWriter(false, false,F2)+
            @"We observe that $\mathit{Forget(F_2,b)}$ is \emph{tautological}, which can be seen as a rather drastic result when we choose to forget $b$ only.
                A variation of a forget operation could be defined as follows:" +
            SkepForgetDefinition() +
            @"The idea is to use conjunction instead of disjunction. The consequences for our previous examples are the following:" +
            ForgetLaTexWriter(true,false,F1) + ForgetLaTexWriter(true,false,F2) +
            @"We observe that our alternative operation, at least in these examples, doesn't necessarily entail less drastic results. 
            We obtain a contradiction in the first case and in the second case, our equivalent to $a$. 
            It suggests a duality between the two operations.";

        return intro1;
    }

    string ForgetDefinition() {
        return $"\\begin{{definition}}[Variable Forgetting]\n" +
               $"Let F be a formula, p an atomic variable and S as Set of Variables. " +
               $"The \\emph{{forgetting of p in F $\\mathit{{Forget}}(F,p)$}}, respectively, the \\emph{{forgetting of S in F}} $Forget(F,S)$, is given inductively by:\n" +
               $"\\end{{definition}}\n\n " +
               $"\\begin{{itemize}}\n \\" +
               $"item $\\mathit{{Forget(F,\\emptyset)}} = F$ \n \\" +
               $"item $\\mathit{{Forget(F,\\{{p\\}})}} = F[p/\\top] \\lor F[p/\\bot]$\n \\" +
               $"item $\\mathit{{Forget}}(F,p) = \\mathit{{Forget}}(F,\\{{p\\}})$\n \\" +
               $"item $\\mathit{{Forget(F,S \\cup \\{{p\\}})}} = \\mathit{{Forget(Forget(F,\\{{p\\}}), S)}}$\n \\" +
               $"end{{itemize}}";
    }

    string SkepForgetDefinition() {
        return $"\\begin{{definition}}[Skeptical Variable Forgetting]\n" +
               $"Let F be a formula, p an atomic variable and S as Set of Variables. " +
               $"The \\emph{{skeptical forgetting of p in F $\\mathit{{{BachelorThesis.SkepForgetName}}}(F,p)$}}, respectively, the \\emph{{skeptical forgetting of S in F}} $\\mathit{{{BachelorThesis.SkepForgetName}(F,S)}}$, is given inductively by:\n" +
               $"\\end{{definition}}\n \n " +
               $"\\begin{{itemize}}\n \\" +
               $"item $\\mathit{{{BachelorThesis.SkepForgetName}(F,\\emptyset)}} = F$\n \\" +
               $"item $\\mathit{{{BachelorThesis.SkepForgetName}(F,\\{{p\\}})}} = F[p/\\top] \\land F[p/\\bot]$\n \\" +
               $"item $\\mathit{{{BachelorThesis.SkepForgetName}}}(F,p) = \\mathit{{{BachelorThesis.SkepForgetName}}}(F,\\{{p\\}})$\n \\" +
               $"item $\\mathit{{{BachelorThesis.SkepForgetName}(F,S \\cup \\{{p\\}})}} = \\mathit{{{BachelorThesis.SkepForgetName}({BachelorThesis.SkepForgetName}(F,\\{{p\\}}), S)}}$\n \\" +
               $"end{{itemize}}";
    }
}