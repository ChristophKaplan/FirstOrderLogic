using System.Text;

namespace FirstOrderLogic {

    public class Unificator {
        private readonly Dictionary<Variable, Term> _substitutions = new();
        public readonly bool IsUnifiable;
 
        public Unificator(ISentence s1, ISentence s2) {
            if (s1 is not ILiteral l1 || s2 is not ILiteral l2) {
                throw new Exception("Both sentences must be literals");
            }
            
          IsUnifiable = UnifyLiteral(l1, l2);
        }

        private bool UnifyLiteral(ILiteral lit1, ILiteral lit2) {
            if(!lit1.Pred.EqualSignature(lit2.Pred)) {
                //throw new Exception("predicates not unifyable");
                return false;
            }
            
            var len = lit1.Arity;
            for (var i = len-1; i >= 0 ; i--) { //terms sind falschrum?
                if (!UnifyTerm(lit1.Terms[i], lit2.Terms[i])) {
                    return false;
                }
            }

            return true;
        }

        private bool UnifyTerm(Term term1, Term term2) {
            if (term1.Equals(term2)) {
                return true;
            }
            
            if (term1 is Variable var1) {
                return UnifyVar(var1, term2);
            }
            
            if (term2 is Variable var2) {
                return UnifyVar(var2, term1);
            }
            
            if (term1 is Function func1 && term2 is Function func2) {
                return UnifyFunction(func1, func2);
            }
            
            //throw new Exception($"Unification failed for {term1} and {term2}");
            return false;
        }

        private bool UnifyFunction(Function func1, Function func2) {
            if (!func1.EqualSignature(func2)) {
                return false;
            }

            for (var i = 0; i < func1.Terms.Length; i++) {
                if (!UnifyTerm(func1.Terms[i], func2.Terms[i])) {
                    return false;
                }
            }

            return true;
        }

        private bool UnifyVar(Variable var, Term term) {
            if(_substitutions.TryGetValue(var, out var subVar)) {
                return UnifyTerm(subVar, term);
            }
            
            if(term is Variable termVar && _substitutions.TryGetValue(termVar, out var subTerm)) {
                return UnifyTerm(var, subTerm);
            }

            if (term.Occurs(var)) {
                //throw new Exception($"Occurs check failed for {var} and {term}");
                return false;
            }

            _substitutions.Add(var, term);
            return true;
        }
        
        public override string ToString() {
            if(_substitutions.Count == 0) {
                return $"No substitutions ,IsUnifiable: {IsUnifiable}";
            }
            
            var sb = new StringBuilder();
            foreach (var (variable, term) in _substitutions) {
                sb.Append($"[{variable}/{term}], ");
            }

            if (sb.Length > 0) {
                sb.Length -= 2;
            }

            return sb.ToString();
        }
        
        public void Substitute(ref ISentence sentence) {
            if (!IsUnifiable) {
                throw new Exception("unifactor is not usable!");
            }
            
            if (sentence is not ILiteral literal) {
                throw new Exception("Only literals can be substituted");
            }

            foreach (var var in _substitutions.Keys) {
                literal.Pred.SubstituteTerm(var, _substitutions[var]);
            }
        }
    }
}