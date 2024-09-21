namespace FirstOrderLogic;

public class ScopeTable {
    private readonly List<Quantifier> _quantifiers = new();
    public void SetScope(Quantifier other) {
        _quantifiers.RemoveAll(q => q.Variable.Equals(other.Variable)); 
        _quantifiers.Add(other);
    }
    
    public Quantifier GetScope(Variable variable) {
        return _quantifiers.Find(quantifier => quantifier.Variable.Equals(variable));
    }

    private bool IsScoped(Variable variable) {
        return _quantifiers.Exists(quantifier => quantifier.Variable.Equals(variable));
    }
    
    public bool HasBoundVariables(Predicate predicate) {
        var variables = predicate.GetVariables();
        return variables.Any(IsScoped);
    }
}