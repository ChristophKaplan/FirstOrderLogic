using System.Text;
using LRParser.Language;

namespace FirstOrderLogic;

public interface IElementOfDiscourse {
    public int Id{ get; }
}

public interface IDomainOfDiscourse {
    public List<IElementOfDiscourse> Elements { get; }
}

public class Interpretation : ILanguageObject{
    IDomainOfDiscourse Domain { get; }
    
    private readonly Dictionary<AtomicSentence, bool> _propositionAssignment = new();
    private readonly Dictionary<Variable, IElementOfDiscourse> _variableAssigment = new();
    private readonly Dictionary<Predicate, Func<IElementOfDiscourse[], bool>> _relations = new();
    private readonly Dictionary<Function, Func<Term[], IElementOfDiscourse>> _functions = new();

    public Interpretation(IDomainOfDiscourse domain) {
        Domain = domain;
    }
    
    public Interpretation(Interpretation other, IDomainOfDiscourse domain) {
        Domain = domain;
        
        foreach (var kv in other._propositionAssignment) {
            _propositionAssignment.Add(kv.Key, kv.Value);
        }
    }
    
    public bool Evaluate(Sentence sentence, ScopeTable table = null) {
        return sentence switch {
            AtomicSentence atomicSentence => Evaluate(atomicSentence, table),
            ComplexSentence complexSentence => Evaluate(complexSentence, table),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }
    
    private bool Evaluate(AtomicSentence atomicSentence, ScopeTable table) {
        return atomicSentence switch {
            Proposition proposition => Evaluate(proposition),
            Predicate predicate => Evaluate(predicate, table),
            _ => throw new Exception($"Error: {atomicSentence} not found in interpretation.")
        };
    }
    
    private bool Evaluate(Proposition proposition) {
        if (_propositionAssignment.TryGetValue(proposition, out var value)) {
            return value;
        }
        
        throw new Exception($"Error: {proposition} not found in interpretation.");
    }
    
    private bool Evaluate(Predicate predicate, ScopeTable table) {
        
        if(!_relations.TryGetValue(predicate, out var relation)) {
            throw new Exception($"Error: {predicate} not found in interpretation.");
        }
        
        var terms = predicate.Terms; // need to evaluate terms
        
        if (!table.HasBoundVariables(predicate)) {
            return relation(Array.ConvertAll(terms, term => Evaluate(term)));
        }
        
        //how to evaluate bound variables? Ex Ay,z P(x,y,z)
        //probably send in the quantifier evaluate for every variable.
        throw new Exception("Error: bound variables not implemented.");
    }
    
    private IElementOfDiscourse Evaluate(Term term) {
        return term switch {
            Variable variable => _variableAssigment.TryGetValue(variable, out var domain) ? domain : throw new Exception("Error: variable not found in interpretation."),
            Constant constant => _functions.TryGetValue(constant, out var nullAryFunc) ? nullAryFunc(Array.Empty<Term>()) : throw new Exception("Error: constant not found in interpretation."),
            Function function =>  _functions.TryGetValue(function, out var func) ? func(function.Terms) : throw new Exception("Error: function not found in interpretation."),
            _ => throw new Exception($"Error: {term} not found in interpretation.")
        };
    }
    
    private bool Evaluate(ComplexSentence complexSentence, ScopeTable table) {
        return complexSentence.Connective.Symbol switch {
            Connective.LogicSymbol.TRUE => true,
            Connective.LogicSymbol.FALSE => false,
            Connective.LogicSymbol.NEGATION => !Evaluate(complexSentence.Children[0], table),
            Connective.LogicSymbol.CONJUNCTION => Evaluate(complexSentence.Children[0], table) && Evaluate(complexSentence.Children[1], table),
            Connective.LogicSymbol.DISJUNCTION => Evaluate(complexSentence.Children[0], table) || Evaluate(complexSentence.Children[1], table),
            Connective.LogicSymbol.IMPLICATION => !Evaluate(complexSentence.Children[0], table) || Evaluate(complexSentence.Children[1], table),
            Connective.LogicSymbol.BICONDITIONAL => Evaluate(complexSentence.Children[0], table) == Evaluate(complexSentence.Children[1], table),
            Connective.LogicSymbol.UNIVERSAL => Evaluate((Quantifier)complexSentence.Connective, complexSentence.Children[0], table),
            Connective.LogicSymbol.EXISTENTIAL => Evaluate((Quantifier)complexSentence.Connective, complexSentence.Children[0], table),
            _ => throw new Exception($"Error: subtype of {complexSentence.Connective.Symbol} not found.")
        };
    }
    
    private bool Evaluate(Quantifier quantifier, Sentence sentence, ScopeTable table) {
        table ??= new ScopeTable();
        table.SetScope(quantifier);
        
        //one way, to generate constants for all variables and send them through the evaluate route.
        //so for exmaple, if we have ForAll y,z P(x,y,z) we substitue y and z make a lot of new sentence.
        //then we evaluate them all and return true if all are true. but could this be a efficiency problem?
        
        return Evaluate(sentence, table);
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