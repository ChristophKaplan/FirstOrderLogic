using LRParser.Language;

namespace FirstOrderLogic;

public abstract class Term : ILanguageObject {
    private readonly string _termSymbol;

    public Term(string termSymbol) {
        _termSymbol = termSymbol;
    }

    public override string ToString() {
        return _termSymbol;
    }
}
