using System.Text;
using LRParser.Language;

namespace FirstOrderLogic;

public class Interpretation : ILanguageObject{
    private IDomainOfDiscourse Domain { get; set; }
    private readonly Dictionary<IAtomicSentence, bool> _propositionAssignment = new();
    private readonly Dictionary<string, IElementOfDiscourse> _variableAssigment = new();
    private readonly Dictionary<string, Func<IElementOfDiscourse[], bool>> _relations = new();
    private readonly Dictionary<string, Func<Term[], IElementOfDiscourse>> _functions = new();

    private void TestSetUp() {
        Domain = new Domain(new Element(1), new Element(2), new Element(3), new Element(4));
        
        _relations.Add("Human", terms => terms[0] switch {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Human predicate not found.")
        });
        
        _relations.Add("Mortal", terms => terms[0] switch {
            Element element => element.Id == 1 || element.Id == 2,
            _ => throw new Exception("Error: Mortal predicate not found.")
        });
    }
    
    public Interpretation(IDomainOfDiscourse domain) {
        Domain = domain;

        TestSetUp();
    }
    
    public Interpretation(Interpretation other, IDomainOfDiscourse domain) {
        Domain = domain;
        
        foreach (var kv in other._propositionAssignment) {
            _propositionAssignment.Add(kv.Key, kv.Value);
        }
    }
    
    public bool Evaluate(ISentence sentence) {
        if(sentence.HasScopeConflict()) {
            throw new Exception("Error: Sentence has scope conflict.");
        }
        
        return sentence switch {
            AtomicSentence atomicSentence => Evaluate(atomicSentence),
            ComplexSentence complexSentence => Evaluate(complexSentence),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }
    
    private bool Evaluate(IAtomicSentence atomicSentence) {
        return atomicSentence switch {
            Proposition proposition => Evaluate(proposition),
            Predicate predicate => Evaluate(predicate),
            _ => throw new Exception($"Error: {atomicSentence} not found in interpretation.")
        };
    }
    
    private bool Evaluate(IProposition proposition) {
        if (_propositionAssignment.TryGetValue(proposition, out var value)) {
            return value;
        }
        
        throw new Exception($"Error: {proposition} not found in interpretation.");
    }
    
    private bool Evaluate(IPredicate predicate) {
        if(!_relations.TryGetValue(predicate.Symbol, out var relation)) {
            throw new Exception($"Error: {predicate} not found in interpretation.");
        }
        
        if (predicate.HasBoundVariables()) {
          throw new Exception($"Error: {predicate} has bound variables.");
        }
        
        return relation(Array.ConvertAll(predicate.Terms, Evaluate));
    }
    
    private IElementOfDiscourse Evaluate(Term term) {
        return term switch {
            Constant constant => _functions.TryGetValue(constant.TermSymbol, out var nullAryFunc) ? nullAryFunc(Array.Empty<Term>()) : throw new Exception("Error: constant not found in interpretation."),
            Function function =>  _functions.TryGetValue(function.TermSymbol, out var func) ? func(function.Terms) : throw new Exception("Error: function not found in interpretation."),
            Variable variable => _variableAssigment.TryGetValue(variable.TermSymbol, out var domain) ? domain : throw new Exception("Error: variable not found in interpretation."),
            _ => throw new Exception($"Error: {term} not found in interpretation.")
        };
    }
    
    private bool Evaluate(IComplexSentence complexSentence) {
        return complexSentence.Connective.Symbol switch {
            Connective.LogicSymbol.TRUE => true,
            Connective.LogicSymbol.FALSE => false,
            Connective.LogicSymbol.NEGATION => !Evaluate(complexSentence.Children[0]),
            Connective.LogicSymbol.CONJUNCTION => Evaluate(complexSentence.Children[0]) && Evaluate(complexSentence.Children[1]),
            Connective.LogicSymbol.DISJUNCTION => Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]),
            Connective.LogicSymbol.IMPLICATION => !Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]),
            Connective.LogicSymbol.BICONDITIONAL => Evaluate(complexSentence.Children[0]) == Evaluate(complexSentence.Children[1]),
            Connective.LogicSymbol.UNIVERSAL => Domain.Elements.All(element => Evaluate(InstantiateVariable(((Quantifier)complexSentence.Connective).Variable, complexSentence.Children[0], element))),
            Connective.LogicSymbol.EXISTENTIAL => Domain.Elements.Any(element => Evaluate(InstantiateVariable(((Quantifier)complexSentence.Connective).Variable, complexSentence.Children[0], element))),
            _ => throw new Exception($"Error: subtype of {complexSentence.Connective.Symbol} not found.")
        };
    }

    private ISentence InstantiateVariable(Variable variable, ISentence sentence, IElementOfDiscourse element) {
        //TODO: pass the constants as additional param, or ass variable assigment? how is it donw usually?
        //TODO: check how skolemiztion works, and unification
        
        var constantToElement = new Constant($"{variable}_element_{element.Id}");

        if (!_functions.TryAdd(constantToElement.TermSymbol, _ => element)) {
            Console.WriteLine("");
        }
        
        var clone = sentence.Clone(); 
        clone.SubstituteTerm(variable, constantToElement);
        clone.SetParentToParentOf(sentence.Parent); //remove quantifier
        return clone;
    }
    
    public override bool Equals(object? obj) {
        return GetHashCode().Equals(obj?.GetHashCode());
    }

    public override int GetHashCode() {
        var hash = 17;
        foreach (var kv in _propositionAssignment) {
            var (key, value) = kv;
            hash = HashCode.Combine(hash ,key, value);
        }

        return hash;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in _propositionAssignment) {
            sb.Append($"{key}={value}, ");
        }

        return sb.ToString();
    }
}