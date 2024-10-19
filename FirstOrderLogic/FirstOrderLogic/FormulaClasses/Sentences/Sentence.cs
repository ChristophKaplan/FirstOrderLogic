using LRParser.Language;

namespace FirstOrderLogic;

public interface ISentence : ILanguageObject {
    ISentence Parent { get; set; }
    List<ISentence> Children { get; }
    bool IsBinary { get; }
    bool IsUnary { get; }
    bool IsNullary { get; }
    int Arity { get; }
    bool IsLiteral { get; }
    void AddChild(ISentence sentence);
    void InsertChild(int index, ISentence sentence);
    void SetParentToParentOf(ISentence parentOfThis);
    void SubstituteTerm(Term term, Term replacement);
    ISentence Clone();
    void Negate();
    bool HasScopeConflict(List<Variable> boundVariables = default);

    bool IsCNF();
    bool IsDisjunctionOfLiterals();
}

public abstract class Sentence : ISentence {
    public ISentence Parent { get; set; }
    public List<ISentence> Children { get; } = new();

    public bool IsBinary => Children.Count == 2;
    public bool IsUnary => Children.Count == 1;
    public bool IsNullary => Children.Count == 0;
    public virtual int Arity => Children.Count;

    public bool IsLiteral =>
        this is IAtomicSentence || (this is IComplexSentence { IsNegation: true } complexSentence && complexSentence.Children[0] is IAtomicSentence);

    public abstract void SubstituteTerm(Term term, Term replacement);
    public abstract void Negate();

    public void AddChild(ISentence sentence) {
        Children.Add(sentence);
        sentence.Parent = this;
    }

    public void InsertChild(int index, ISentence sentence) {
        Children.Insert(index, sentence);
        sentence.Parent = this;
    }

    public void SetParentToParentOf(ISentence parentOfThis) {
        if (parentOfThis.Parent == default) {
            Parent = default;
            return;
        }

        var parent = parentOfThis.Parent;
        ISentence found = default;
        foreach (var childInParent in parent.Children) {
            if (!childInParent.Equals(parentOfThis)) {
                continue;
            }

            found = childInParent;
        }

        if (found == null) {
            throw new Exception($"this not found in Parent.Children");
        }

        var index = parent.Children.IndexOf(found);
        parent.Children.RemoveAt(index);
        parent.InsertChild(index, this);
    }

    public ISentence Clone() {
        switch (this) {
            case Proposition proposition:
                return new Proposition(proposition);
            case Predicate predicate:
                return new Predicate(predicate);
            case ComplexSentence complexSentence: {
                var result = new ComplexSentence(complexSentence);
                return result;
            }
            default:
                throw new Exception($"Clone: Sentence type {GetType()} not found!");
        }
    }

    public bool HasScopeConflict(List<Variable> boundVariables = default) {
        boundVariables ??= new List<Variable>();

        if (this is IComplexSentence { IsQuantifier: true } complexSentence) {
            var boundVariable = ((Quantifier)complexSentence.Connective).Variable;
            if (boundVariables.Contains(boundVariable)) {
                return true;
            }

            boundVariables.Add(boundVariable);
        }

        return Children.Any(child => child.HasScopeConflict());
    }

    public bool IsCNF() {
        if (this is ILiteral || this is AtomicSentence) return true;

        var complexSentence = this as IComplexSentence;

        if (complexSentence.IsNegation || 
            complexSentence.IsQuantifier ||
            complexSentence.Connective == Connective.LogicSymbol.IMPLICATION || 
            complexSentence.Connective == Connective.LogicSymbol.BICONDITIONAL) {
            return false;
        }

        return complexSentence.Connective == Connective.LogicSymbol.DISJUNCTION ? complexSentence.Children.All(child => child.IsDisjunctionOfLiterals()) : Children.All(child => child.IsCNF());
    }

    public bool IsDisjunctionOfLiterals() {
        if (this is ILiteral) return true;
        return this is IComplexSentence { Connective.Symbol: Connective.LogicSymbol.DISJUNCTION } &&
               Children.All(child => child.IsDisjunctionOfLiterals());
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        return "Sentence";
    }
}