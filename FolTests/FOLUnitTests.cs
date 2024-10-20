using FirstOrderLogic;

namespace FolTests;

public class Tests
{
    private FirstOrderLogic.FirstOrderLogic firstOrderLogic;
    private Interpretation interpretation;

    [SetUp]
    public void Setup()
    {
        firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
        interpretation = Inter01();
    }

    private Interpretation Inter01()
    {
        IDomainOfDiscourse Domain = new Domain(new Element(1), new Element(2), new Element(3), new Element(4));

        var relations = new Dictionary<string, Func<IElementOfDiscourse[], bool>>();
        var functions = new Dictionary<string, Func<Term[], IElementOfDiscourse>>();
        var variableAssignments = new Dictionary<string, IElementOfDiscourse>();
        var propositionalAssignments = new Dictionary<IAtomicSentence, bool>();

        relations.Add("Human", terms => terms[0] switch
        {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Human predicate not found.")
        });

        relations.Add("Mortal", terms => terms[0] switch
        {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Mortal predicate not found.")
        });

        return new Interpretation(Domain, relations, functions, variableAssignments, propositionalAssignments);
    }

    [Test]
    public void Evaluation()
    {
        var parsed = (Sentence)firstOrderLogic.TryParse("FORALL x (Human(x) => (Mortal(x)))");
        var simplified = firstOrderLogic.Simplify(parsed, out var steps);

        Assert.That(interpretation.Evaluate(parsed), Is.EqualTo(true));
    }

    [Test]
    public void Unification()
    {
        var p1 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
        var p2 = (ISentence)firstOrderLogic.TryParse("P(y,z,a)");
        var unificator1 = new Unificator(p1, p2);
        Console.WriteLine(unificator1);

        var p3 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
        var p4 = (ISentence)firstOrderLogic.TryParse("P(f(y),y,x)");
        var unificator2 = new Unificator(p3, p4);
        Console.WriteLine(unificator2);

        var p5 = (ISentence)firstOrderLogic.TryParse("P(f(x),a,x)");
        var l = p5.GetPredicate();
        l.Terms[1] = new Constant("a");
        var p6 = (ISentence)firstOrderLogic.TryParse("P(f(g(y)),z,z)");
        var unificator3 = new Unificator(p5, p6);
        Console.WriteLine(unificator3);

        Assert.That(unificator1.IsUnifiable, Is.EqualTo(true));
        Assert.That(unificator2.IsUnifiable, Is.EqualTo(false));
        Assert.That(unificator3.IsUnifiable, Is.EqualTo(false));
    }
    
    [Test]
    public void CNF()
    {
        var p = (ISentence)firstOrderLogic.TryParse("NOT(P(x) => P(y))");
        var p2 = firstOrderLogic.Simplify(p, out var steps);

        Console.WriteLine(p);
        Console.WriteLine(p2);

        Assert.That(p.IsCNF(), Is.EqualTo(false));
        Assert.That(p2.IsCNF(), Is.EqualTo(true));
    }

    [Test]
    public void ClauseSet()
    {
        var p = (ISentence)firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
        var p2 = firstOrderLogic.Simplify(p, out var steps);
        Console.WriteLine(p2 + " cnf:" + p2.IsCNF());
        var set = p2.GetClauseSet();
        Console.WriteLine(set.Aggregate("", (current, clause) => current + clause + "\n"));
        Assert.That(set.Count, Is.EqualTo(2));
    }

    [Test]
    public void ClauseUnification()
    {
        var P = (ISentence)firstOrderLogic.TryParse("P(x)");
        var Q = (ISentence)firstOrderLogic.TryParse("Q(y)");
        var NotPy = (ISentence)firstOrderLogic.TryParse("NOT P(y)");
        var clause1 = new Clause(P,Q);
        var clause2 = new Clause(NotPy);
        
        for (var i = 0; i < clause1.Literals.Count; i++)
        {
            for (var j = i; j < clause2.Literals.Count; j++)
            {
                var unify = new Unificator(clause1.Literals[i], clause2.Literals[j]);
                if (unify.IsUnifiable)
                {
                    unify.Substitute(clause1);
                    unify.Substitute(clause2);
                    
                    Console.WriteLine(clause1);
                    Console.WriteLine(clause2);
                }
            }
        }
        
        Assert.That(clause1.Literals[0].GetPredicate().Terms[0].TermSymbol, Is.EqualTo("y"));
    }
    
    [Test] 
    public void Resolution()
    {
        var PpImpliesQ = (ISentence)firstOrderLogic.TryParse("(Human(Sokrates) AND (Human(x) => Mortal(x)))");
        var PpImpliesQ2 = firstOrderLogic.Simplify(PpImpliesQ, out var steps);
        var q = (ISentence)firstOrderLogic.TryParse("Mortal(Sokrates)");
        var resolution = new Resolution();
        var b = resolution.Resolve(PpImpliesQ2, q);
        
        Assert.That(b, Is.EqualTo(true));
    }
}