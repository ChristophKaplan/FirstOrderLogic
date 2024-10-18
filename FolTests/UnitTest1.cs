using FirstOrderLogic;

namespace FolTests;

public class Tests {
    private FirstOrderLogic.FirstOrderLogic firstOrderLogic;
    private Interpretation interpretation;
    
    [SetUp]
    public void Setup() {
        firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
        interpretation = Inter01();
    }

    private Interpretation Inter01() {
        IDomainOfDiscourse Domain = new Domain(new Element(1), new Element(2), new Element(3), new Element(4));

        var relations = new Dictionary<string, Func<IElementOfDiscourse[], bool>>();
        var functions = new Dictionary<string, Func<Term[], IElementOfDiscourse>>();
        var variableAssignments = new Dictionary<string, IElementOfDiscourse>();
        var propositionalAssignments = new Dictionary<IAtomicSentence, bool>();
        
        relations.Add("Human", terms => terms[0] switch {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Human predicate not found.")
        });
        
        relations.Add("Mortal", terms => terms[0] switch {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Mortal predicate not found.")
        });
        
        return new Interpretation(Domain, relations, functions, variableAssignments, propositionalAssignments);
    }
    
    [Test]
    public void Test1() {
        var parsed = (Sentence)firstOrderLogic.TryParse("FORALL x (Human(x) => (Mortal(x)))");
        var simplified = firstOrderLogic.Simplify(parsed, out var steps);
        
        Assert.That(interpretation.Evaluate(parsed), Is.EqualTo(true));
    }
    
    [Test]
    public void Test_Unification() {
        var p1 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
        var p2 = (ISentence)firstOrderLogic.TryParse("P(y,z,a)");
        var unificator1 = new Unificator(p1, p2);
        Console.WriteLine(unificator1);
        
        var p3 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
        var p4 = (ISentence)firstOrderLogic.TryParse("P(f(y),y,x)");
        var unificator2 = new Unificator(p3, p4);
        Console.WriteLine(unificator2);
        
        var p5 = (ISentence)firstOrderLogic.TryParse("P(f(x),a,x)");
        var l = (ILiteral)p5;
        l.Terms[1] = new Constant("a");
        var p6 = (ISentence)firstOrderLogic.TryParse("P(f(g(y)),z,z)");
        var unificator3 = new Unificator(p5, p6);
        Console.WriteLine(unificator3);
        
        Assert.That(unificator1.IsUnifiable, Is.EqualTo(true));
        Assert.That(unificator2.IsUnifiable, Is.EqualTo(false));
        Assert.That(unificator3.IsUnifiable, Is.EqualTo(false));
    }
}