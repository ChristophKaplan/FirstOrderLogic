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
        var human = (Sentence)firstOrderLogic.TryParse("Human(x)");
        var mortal = (Sentence)firstOrderLogic.TryParse("Mortal(x)");
        
        Unificator unificator = new(human, mortal);
        
        Console.WriteLine(unificator);
        
        Assert.That(true, Is.EqualTo(true));
    }
}