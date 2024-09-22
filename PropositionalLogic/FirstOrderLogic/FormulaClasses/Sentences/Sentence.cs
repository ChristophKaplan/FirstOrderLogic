using LRParser.Language;
namespace FirstOrderLogic;

public abstract class Sentence : ILanguageObject {
    public Sentence Parent { get; protected set; }
    public readonly List<Sentence> Children = new();

    public bool IsBinary => Children.Count == 2;
    public bool IsUnary => Children.Count == 1;
    public bool IsNullary => Children.Count == 0;
    
    public bool IsLiteral =>
        this is AtomicSentence || (this is ComplexSentence { IsNegation: true } complexSentence && complexSentence.Children[0] is AtomicSentence);

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence.Parent = this;
    }

    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index, sentence);
        sentence.Parent = this;
    }

    public void SetParentToParentOf(Sentence parentOfThis) {
        if (parentOfThis.Parent == null) {
            this.Parent = null; //??
            return;
        }

        var parent = parentOfThis.Parent;
        Sentence found = null;
        foreach (var childInParent in parent.Children) {
            if (childInParent.Equals(parentOfThis)) {
                found = childInParent;
            }
        }

        if (found == null) {
            throw new Exception($"this not found in Parent.Children");
        }

        var index = parent.Children.IndexOf(found);
        parent.Children.RemoveAt(index);
        parent.InsertChild(index, this);
    }
    
    public abstract void SubstituteTerm(Term term, Term replacement);
    
    public Sentence Clone() {
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
                throw new Exception($"Clone: Sentence type {this.GetType()} not found!");
        }
    }

    public void Negate() {
        //TODO: what if its negation already
        var negated = new ComplexSentence(Connective.LogicSymbol.NEGATION, Clone());
        negated.SetParentToParentOf(this);
    }

    public bool HasScopeConflict(List<Variable> boundVariables = default) {
        boundVariables ??= new List<Variable>();
        
        if(this is ComplexSentence { IsQuantifier: true } complexSentence) {
            var boundVariable = ((Quantifier) complexSentence.Connective).Variable;
            if (boundVariables.Contains(boundVariable)) {
                return true;
            }
            
            boundVariables.Add(boundVariable);
        }

        return Children.Any(child => child.HasScopeConflict());
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