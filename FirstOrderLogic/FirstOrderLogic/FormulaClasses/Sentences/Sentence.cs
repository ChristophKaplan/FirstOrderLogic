using Helpers;
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
    bool IsNegation { get; }
    bool IsImplication { get; }
    bool IsNegationOf(ISentence other);
    void AddChild(ISentence sentence);
    void InsertChild(int index, ISentence sentence);
    void SetParentToParentOf(ISentence parentOfThis);
    void SubstituteTerm(Term term, Term replacement);
    ISentence Clone();
    ISentence Negate();
    bool HasScopeConflict(List<Variable> boundVariables = default);
    bool IsCNF();
    bool IsPropositional();
    bool IsDisjunctionOfLiterals();
    List<ISentence> GetLiterals();
    IPredicate GetPredicate();
    IProposition GetProposition();
    void AddTime(int i);
    bool IsImplicationAndEqualPremise(ISentence premise);
}

public abstract class Sentence : ISentence {
    public ISentence Parent { get; set; }
    public List<ISentence> Children { get; } = new();

    public bool IsBinary => Children.Count == 2;
    public bool IsUnary => Children.Count == 1;
    public bool IsNullary => Children.Count == 0;
    public virtual int Arity => Children.Count;
    public bool IsLiteral => this is IAtomicSentence || 
                             (this is IComplexSentence { IsNegation: true } complex && complex.Children[0] is IAtomicSentence);
    public bool IsNegation => this is IComplexSentence complex && complex.Connective == Connective.LogicSymbol.NEGATION;
    public bool IsImplication => this is IComplexSentence complex && complex.Connective == Connective.LogicSymbol.IMPLICATION;
    public abstract void SubstituteTerm(Term term, Term replacement);
    public abstract ISentence Negate();
    public abstract ISentence Clone();
    public bool IsNegationOf(ISentence other) {
        if (IsNegation && !other.IsNegation && Children[0].Equals(other))
        {
            return true;
        }

        if (other.IsNegation && !IsNegation && Equals(other.Children[0]))
        {
            return true;
        }

        return false;
    }

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
        if (IsLiteral) return true;

        var complexSentence = this as IComplexSentence;

        if (complexSentence.IsNegation || 
            complexSentence.IsQuantifier ||
            complexSentence.Connective == Connective.LogicSymbol.IMPLICATION || 
            complexSentence.Connective == Connective.LogicSymbol.BICONDITIONAL) {
            return false;
        } 
        
        var eval = complexSentence.Connective == Connective.LogicSymbol.DISJUNCTION ? complexSentence.Children.All(child => child.IsDisjunctionOfLiterals()) : Children.All(child => child.IsCNF());
        return eval;
    }

    public bool IsDisjunctionOfLiterals() {
        if (IsLiteral) return true;
        return this is IComplexSentence { Connective.Symbol: Connective.LogicSymbol.DISJUNCTION } &&
               Children.All(child => child.IsDisjunctionOfLiterals());
    }

    public List<ISentence> GetLiterals() {
        var literals = new List<ISentence>();
        
        if (IsLiteral)
        {
            literals.Add(this);
            return literals;
        }
        
        foreach (var child in Children)
        {
            literals.AddRange(child.GetLiterals());    
        }
        
        return literals;
    }

    public IPredicate GetPredicate()
    {
        if(!IsLiteral) throw new Exception("Sentence is not a literal");
        return this switch
        {
            IPredicate predicate => predicate,
            IComplexSentence => Children[0] as IPredicate,
        };
    }
    
    public IProposition GetProposition()
    {
        if(!IsLiteral) throw new Exception("Sentence is not a literal");
        return this switch
        {
            IProposition proposition => proposition,
            IComplexSentence => Children[0] as IProposition,
        };
    }

    public void AddTime(int t) {
        if (this is IAtomicSentence atomicSentence) {
            atomicSentence.Time += t;
        }
        else {
            foreach (var child in Children) {
                child.AddTime(t);
            }
        }
    }

    public bool IsImplicationAndEqualPremise(ISentence premise) {
        if (!IsImplication) {
            return false;
        }

        var complexSentence = (IComplexSentence)this;
        return complexSentence.Children[0].Equals(premise);
    }

    public bool IsPropositional() {
        var b = this switch {
            IProposition => true,
            IComplexSentence complexSentence => complexSentence.Children.All(child => child.IsPropositional()),
            _ => false
        };

        if (!b) {
            Logger.Log($"{this} is not propositional");
        }
        return b;
    }
    
    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        
        return Children.SequenceEqual(((Sentence)obj).Children);
    }

    public override int GetHashCode() {
        return Children.Aggregate(0, (current, child) => HashCode.Combine(current, child.GetHashCode()));
    }

    public override string ToString() {
        return "Sentence";
    }
}