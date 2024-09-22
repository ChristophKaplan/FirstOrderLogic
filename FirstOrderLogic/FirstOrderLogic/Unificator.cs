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
            if (literal1.Arity != literal2.Arity && !literal1.Symbol.Equals(literal2.Symbol)) {
                return false;
            }

            var terms1 = literal1.Terms;
            var terms2 = literal2.Terms;

            for (var i = 0; i < terms1.Length; i++) {
                if (!terms1[i].Equals(terms2[i])) {
                    if (IsUnifiable(terms1[i], terms2[i])) {
                        return true;
                    }
                }
            }

            return false;
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
            if (t1 is Variable varT1) return new (varT1, t2);
            if (t2 is Variable varT2) return new (varT2, t1);
            
            throw new Exception("Unification error!");
        }

        public override string ToString() {
            var s = "";
            foreach (var (variable, term) in _substitutions) {
                s += $"{variable} -> {term}, ";
            }
            
            return s;
        }
    }
}