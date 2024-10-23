using FirstOrderLogic;
using Helpers;

namespace FolTests;

public class Tests
{
    private FirstOrderLogic.FirstOrderLogic _firstOrderLogic;
    private Interpretation _interpretation;

    [SetUp]
    public void Setup()
    {
        _firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
        _interpretation = Inter01();
    }

    private Interpretation Inter01()
    {
        IDomainOfDiscourse domain = new Domain(new Element(1), new Element(2), new Element(3), new Element(4));
        
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

        return new Interpretation(domain, relations, functions, variableAssignments, propositionalAssignments);
    }

    [Test]
    public void Simplify() {
        var p = (ISentence)_firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
        var p2 = _firstOrderLogic.Simplify(p, out var steps);
        var shouldbe = (ISentence)_firstOrderLogic.TryParse("((NOT P(x)) OR Q(y)) AND R(z)");
        
        Assert.That(p2, Is.EqualTo(shouldbe));
    }
    

    [Test]
    public void Evaluation()
    {
        var parsed = (Sentence)_firstOrderLogic.TryParse("FORALL x (Human(x) => (Mortal(x)))");
        Assert.That(_interpretation.Evaluate(parsed), Is.EqualTo(true));
    }

    [Test]
    public void Unification()
    {
        var p1 = (ISentence)_firstOrderLogic.TryParse("P(x,y,y)");
        var p2 = (ISentence)_firstOrderLogic.TryParse("P(y,z,a)");
        var unificator1 = new Unificator(p1, p2);
        Logger.Log(unificator1.ToString());

        var p3 = (ISentence)_firstOrderLogic.TryParse("P(x,y,y)");
        var p4 = (ISentence)_firstOrderLogic.TryParse("P(f(y),y,x)");
        var unificator2 = new Unificator(p3, p4);
        Logger.Log(unificator2.ToString());

        var p5 = (ISentence)_firstOrderLogic.TryParse("P(f(x),a,x)");
        var l = p5.GetPredicate();
        l.Terms[1] = new Constant("a");
        var p6 = (ISentence)_firstOrderLogic.TryParse("P(f(g(y)),z,z)");
        var unificator3 = new Unificator(p5, p6);
        Logger.Log(unificator3.ToString());

        Assert.That(unificator1.IsUnifiable, Is.EqualTo(true));
        Assert.That(unificator2.IsUnifiable, Is.EqualTo(false));
        Assert.That(unificator3.IsUnifiable, Is.EqualTo(false));
    }
    
    [Test]
    public void ConjunctiveNormalForm()
    {
        var p = (ISentence)_firstOrderLogic.TryParse("NOT(P(x) => P(y))");
        var p2 = _firstOrderLogic.Simplify(p, out var steps);

        Logger.Log(p.ToString());
        Logger.Log(p2.ToString());

        Assert.That(p.IsCNF(), Is.EqualTo(false));
        Assert.That(p2.IsCNF(), Is.EqualTo(true));
    }

    [Test]
    public void ClauseSet()
    {
        var p = (ISentence)_firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
        var p2 = _firstOrderLogic.Simplify(p, out var steps);
        Logger.Log(p2 + " cnf:" + p2.IsCNF());
        var set = p2.GetClauseSet();
        Logger.Log(set.Aggregate("", (current, clause) => current + clause + "\n"));
        Assert.That(set.Count, Is.EqualTo(2));
    }

    [Test]
    public void ClauseUnification()
    {
        var p = (ISentence)_firstOrderLogic.TryParse("P(x)");
        var q = (ISentence)_firstOrderLogic.TryParse("Q(y)");
        var notPy = (ISentence)_firstOrderLogic.TryParse("NOT P(y)");
        var clause1 = new Clause(p,q);
        var clause2 = new Clause(notPy);
        
        for (var i = 0; i < clause1.Literals.Count; i++)
        {
            for (var j = i; j < clause2.Literals.Count; j++)
            {
                var unify = new Unificator(clause1.Literals[i], clause2.Literals[j]);
                if (unify.IsUnifiable)
                {
                    unify.Substitute(clause1);
                    unify.Substitute(clause2);
                    
                    Logger.Log(clause1.ToString());
                    Logger.Log(clause2.ToString());
                }
            }
        }
        
        Assert.That(clause1.Literals[0].GetPredicate().Terms[0].TermSymbol, Is.EqualTo("y"));
    }
    
    [Test] 
    public void Resolution()
    {
        var PpImpliesQ = (ISentence)_firstOrderLogic.TryParse("(Human(Sokrates) AND (Human(x) => Mortal(x)))");
        var PpImpliesQ2 = _firstOrderLogic.Simplify(PpImpliesQ, out var steps);
        var q = (ISentence)_firstOrderLogic.TryParse("Mortal(Sokrates)");
        var resolution = new Resolution();
        var b = resolution.Resolve(PpImpliesQ2, q);
        
        Assert.That(b, Is.EqualTo(true));
    }
}