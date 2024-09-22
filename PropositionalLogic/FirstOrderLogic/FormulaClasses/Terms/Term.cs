using LRParser.Language;

namespace FirstOrderLogic;

public abstract class Term : ILanguageObject {
    public readonly string TermSymbol;

    public Term(string termSymbol) {
        TermSymbol = termSymbol;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return TermSymbol.Equals(((Term)obj).TermSymbol);
    }
    
    public override int GetHashCode() {
        return TermSymbol.GetHashCode();
    }

    public override string ToString() {
        return TermSymbol;
    }
    
    public Variable[] GetVariables() {
        switch (this) {
            case Variable variable:
                return new[] { variable };
            case Function function: {
                var variables = new List<Variable>();
                foreach (var term in function.Terms) {
                    variables.AddRange(term.GetVariables());
                }
                return variables.ToArray();
            }
            default:
                return Array.Empty<Variable>();
        }
    }
    
    public bool Contains(Variable variable) {
        return GetVariables().Contains(variable);
    }
}
