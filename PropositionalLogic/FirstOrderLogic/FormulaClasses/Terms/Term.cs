using LRParser.Language;

namespace FirstOrderLogic;

public abstract class Term : ILanguageObject {
    private readonly string _termSymbol;

    public Term(string termSymbol) {
        _termSymbol = termSymbol;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return _termSymbol.Equals(((Term)obj)._termSymbol);
    }
    
    public override int GetHashCode() {
        return _termSymbol.GetHashCode();
    }

    public override string ToString() {
        return _termSymbol;
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
}
