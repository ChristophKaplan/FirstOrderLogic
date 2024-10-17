using System.Text;

namespace FirstOrderLogic {

    public class Unificator {
        private readonly List<(Variable, Term)> _substitutions = new();

        public Unificator(ISentence s1, ISentence s2) {
            if (!s1.IsLiteral || !s2.IsLiteral) {
                throw new Exception("must be literals");
            }
            
            var l1 = s1 as ILiteral;
            var l2 = s2 as ILiteral;

            if (IsUnifiable(l1, l2)) {
                _substitutions = GetSubstitutions(l1, l2);
            }
            else {
                Console.WriteLine("not unifyable!");
            }
        }

        private bool IsUnifiable(Term term1, Term term2) {
            if (term1 is Variable varTerm1) {
                if (!term2.Contains(varTerm1)) return true;
            }
                
            if (term2 is Variable varTerm2) {
                if (!term1.Contains(varTerm2)) return true;
            }

            return false;
        }

        private bool IsUnifiable(ILiteral literal1, ILiteral literal2) {
            if (literal1.Arity != literal2.Arity && !literal1.Pred.Equals(literal2.Pred)) {
                return false;
            }

            var terms1 = literal1.Terms;
            var terms2 = literal2.Terms;

            return terms1.Where((term1_i, i) => !term1_i.Equals(terms2[i]) && IsUnifiable(term1_i, terms2[i])).Any(); //but what if they are equal it ?
        }

        private List<(Variable, Term)> GetSubstitutions(ILiteral l1, ILiteral l2) {
            var terms1 = l1.Terms;
            var terms2 = l2.Terms;
            
            var subs = new List<(Variable, Term)>();
            
            for (var i = 0; i < terms1.Length; i++)
                if (IsUnifiable(terms1[i], terms2[i])) {
                    var mapTuple = GetSubstitution(terms1[i], terms2[i]);
                    subs.Add(mapTuple);
                }

            return subs;
        }
        
        private (Variable, Term) GetSubstitution(Term t1, Term t2) {
            if (t1 is Variable varT1) return new ValueTuple<Variable, Term>(varT1, t2);
            if (t2 is Variable varT2) return new ValueTuple<Variable, Term>(varT2, t1);
            
            throw new Exception("Unification error!");
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (var (variable, term) in _substitutions) {
                sb.Append($"[{variable}\\{term}], ");
            }

            if (sb.Length > 0) {
                sb.Length -= 2;
            }

            return sb.ToString();
        }


        private List<(Variable, Term)> Step5(Term term1, Term term2) {
            
        }

        private (Variable, Term) Step1(Term term1, Term term2) {
            if (term1.Equals(term2)) {
                return (null, null);
            }
            
            if (term1 is Variable varTerm1) {
                if (term2.Contains(varTerm1)) {
                    return (null, null);
                }
                else {
                    return (varTerm1, term2);
                }
            }
            
            if (term2 is Variable varTerm2) {
                if (term1.Contains(varTerm2)) {
                    return (null, null);
                }
                else {
                    return (varTerm2, term1);
                }
            }

            return (null, null);
        }
    }
}