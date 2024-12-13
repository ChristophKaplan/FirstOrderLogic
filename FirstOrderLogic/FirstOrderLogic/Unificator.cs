using System.Text;

namespace FirstOrderLogic
{
    public class Unificator
    {
        private readonly Dictionary<Variable, Term> _substitutions = new();
        public readonly bool IsUnifiable;
        public bool IsEmpty => _substitutions.Count == 0;

        public override bool Equals(object? obj) {
            if (obj is Unificator unificator) {
                if (unificator._substitutions.Count != _substitutions.Count) return false;
                foreach (var (key, value) in _substitutions) {
                    if (!unificator._substitutions.TryGetValue(key, out var otherValue) || !value.Equals(otherValue)) return false;
                }
                return true;
            }
            return false;
        }
        
        public override int GetHashCode() {
            var hash = 17;
            foreach (var (key, value) in _substitutions) {
                hash = hash * 31 + key.GetHashCode();
                hash = hash * 31 + value.GetHashCode();
            }
            return hash;
        }

        public Unificator(ISentence s1, ISentence s2)
        {
            IsUnifiable = UnifyLiteral(s1, s2);
        }
        
        private bool UnifyLiteral(ISentence lit1, ISentence lit2)
        {
            if (!lit1.IsLiteral || !lit2.IsLiteral)
            {
                throw new Exception("Both sentences must be literals");
            }

            var pred1 = lit1.GetPredicate();
            var pred2 = lit2.GetPredicate();
            var len = lit1.Arity;

            if(pred1.Symbol != pred2.Symbol || lit1.Arity != pred2.Arity)
            {
                return false;
            }
            
            for (var i = len - 1; i >= 0; i--)
            {
                //terms sind falschrum?
                if (!UnifyTerm(pred1.Terms[i], pred2.Terms[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool UnifyTerm(Term term1, Term term2)
        {
            if (term1.Equals(term2))
            {
                return true;
            }

            if (term1 is Variable var1)
            {
                return UnifyVar(var1, term2);
            }

            if (term2 is Variable var2)
            {
                return UnifyVar(var2, term1);
            }

            if (term1 is Function func1 && term2 is Function func2)
            {
                return UnifyFunction(func1, func2);
            }

            //throw new Exception($"Unification failed for {term1} and {term2}");
            return false;
        }

        private bool UnifyFunction(Function func1, Function func2)
        {
            if (!func1.EqualSignature(func2))
            {
                return false;
            }

            for (var i = 0; i < func1.Terms.Length; i++)
            {
                if (!UnifyTerm(func1.Terms[i], func2.Terms[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool UnifyVar(Variable var, Term term)
        {
            if (_substitutions.TryGetValue(var, out var subVar))
            {
                return UnifyTerm(subVar, term);
            }

            if (term is Variable termVar && _substitutions.TryGetValue(termVar, out var subTerm))
            {
                return UnifyTerm(var, subTerm);
            }

            if (term.Occurs(var))
            {
                //throw new Exception($"Occurs check failed for {var} and {term}");
                return false;
            }

            _substitutions.Add(var, term);
            return true;
        }

        public override string ToString()
        {
            if (_substitutions.Count == 0)
            {
                return $"No substitutions ,IsUnifiable: {IsUnifiable}";
            }

            var sb = new StringBuilder();
            foreach (var (variable, term) in _substitutions)
            {
                sb.Append($"[{variable}/{term}], ");
            }

            if (sb.Length > 0)
            {
                sb.Length -= 2;
            }

            return sb.ToString();
        }

        public void Substitute(Clause clause)
        {
            clause.Literals.ForEach(lit => Substitute(ref lit));
        }
        
        public void Substitute(ref ISentence sentence)
        {
            if (!IsUnifiable)
            {
                throw new Exception("unifactor is not usable!");
            }

            foreach (var var in _substitutions.Keys)
            {
                sentence.SubstituteTerm(var, _substitutions[var]);
            }
        }
    }
}