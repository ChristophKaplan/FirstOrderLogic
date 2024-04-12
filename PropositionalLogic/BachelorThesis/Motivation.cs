namespace PropositionalLogic.Helpers;

public class Motivation : Section {
    public Motivation(PropositionalLogic logic) : base(logic) {
    }

    protected override string Content() {
        
        var F1 = ("F_1", "a AND b", "b");
        var F2 = ("F_2", "a OR b", "b");

        var motivation = $$"""
                         Why ougth an agent forget aquired knowledge at all? Wouldn't that be fatal? Should we just forget that the Earth is a sphere? Is the result a step forward or backward?
                         We presuppose that Truth or what is socially valid as Truth changes in the context of time and history. Practically, knowledge or facts are indeed compelling but not dogmatic 
                         and can be falsified by the progress of scientific practice. Knowledge changes, can be replaced by other knowledge, or become irrelevant. Whether it's about knowledge representation, 
                         reasoning, belief change, or multi-agent systems, in Artificial Intelligence, there are many examples where knowledge modeling is a crucial component. Thus, it's clear that it's 
                         advantageous not only to acquire knowledge but also to be able to eliminate it. We don't just want to model strict knowledge but also views, values or opinions. As is well known, 
                         these change in the context of time and the social circumstance in which they are carried. Certain views become irrelevant and simply forgotten.
                         Furthermore, there are purely material difficulties, as computing complex queries to a knowledge base or drawing conclusions by artificial agents requires high computational power. 
                         Filtering out irrelevant knowledge also means reducing complexity, from which many areas of AI would technically benefit. Another perspective is that of focus. Knowledge changes 
                         depending on what is considered. If certain connections are not seen, the whole shape changes. What is deemed relevant also determines knowledge. Here we have a rather indirect or 
                         presupposed perspective for our topic. What is irrelevant is consequently also forgotten. 
                         (Transition to logic) 
                         To this end there are a few approaches to forgetting in logic yet it is not a well researched topic. One can come up with many ways of what forgetting even means and how it should work.
                         We will concentrate of the variable forgetting in propositional logic. In the laws of thought \cite{boole2021investigation} George Boole denotes the so called 
                         "elimination of the middle terms" wich can be seen as an early version of variable forget in the field.
                         The Forget operation that we want to build upon, goes back to the paper Forget It! \cite{lin1994forget} by Lin and Reiter in 1994 wich takes the approach to make the variable irrelevant. 
                         Their notion is defined by having two interpretations that totally agree about anything except the truthvalues of variable to forget, hence it is irrelevant.
                         We will define a variation of this operation, which we call the {{BachelorThesis.SkepForgetName}} operation and that we want to investigate in this thesis.
                         """;
        
        var forgetIntro = $$"""
            Now we can the classical forget operation as stated in \cite{lin1994forget} or \cite{lang2003propositional}, we define variable forgetting inductively as follows:
            {{SkepForgetDefinition()}}
            For some formulas $F$, this operation of variable elimination can lead to a notable reductive or drastic results. 
                We start by considering an example where $\mathit{Forget}$ behaves quite appropriate. 
                Let a sentence $F_1$ be $a \land b$ and we choose to forget about $b$:
            {{ForgetLaTexWriter(false,false,true, true,F1)}}
            We can see, as shown above, that $\mathit{Forget(F_1,b)}$ is equivalent to $a$ which seems to be the intuitive result when thinking of forgetting about $b$ in $F_1$. 
                    Next, we consider forgetting of $b$ in the formula $F_2 = a \lor b$
            {{ForgetLaTexWriter(false, false,true, true,F2)}}
            We observe that $\mathit{Forget(F_2,b)}$ is \emph{tautological}, which can be seen as a rather drastic result when we choose to forget $b$ only.
                A variation of a forget operation could be defined as follows:
            {{SkepForgetDefinition()}}
            The idea is to use conjunction instead of disjunction. The consequences for our previous examples are the following:
            {{ForgetLaTexWriter(true,false,true, true,F1) + ForgetLaTexWriter(true,false,true, true,F2)}}
            We observe that our alternative operation, at least in these examples, doesn't necessarily entail less drastic results. 
            We obtain a contradiction in the first case and in the second case, our equivalent to $a$. 
            It suggests a duality between the two operations.
            """;

        return motivation +Preliminaries()+ forgetIntro;
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

    string Preliminaries() {
        var cont = """
                   \subsection{Preliminaries}
                   Before we jump now into the topic of forgetting, we need to clarify some basic concepts of propositional logic.
                   Let $L$ be a propositional language. We denote set of propositional variables by $V$. Sentences or Formulas in $L$ over $V$
                   inductively defined over the common logical connectives ($\neg$, $\land$, $\lor$ , $\rightarrow$, $\leftrightarrow$) as well as the logical constants $true$ and $false$.
                   For every propositional variable a $\in$ V, a is sentence. If F is a sentence, then so is $\neg$ F. If F and G are sentences,
                   then so are (F $\land$ G), (F $\lor$ G), (F $\rightarrow$ G), and (F $\leftrightarrow$ G). For instance, $F = (\neg(a \land b) \lor (c \rightarrow d))$ is a sentence.
                   """;
        return cont;
    }
}