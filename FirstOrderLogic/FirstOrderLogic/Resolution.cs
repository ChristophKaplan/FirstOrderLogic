namespace FirstOrderLogic;

public class Resolution
{
    private List<Resolvent> Resolve(Clause clause1, Clause clause2)
    {
        var resolvents = new List<Resolvent>();

        for (var i = 0; i < clause1.Literals.Count; i++)
        {
            for (var j = i + 1; j < clause2.Literals.Count; j++)
            {
                if (IsResolvable(clause1.Literals[i], clause2.Literals[j]))
                {
                    var r = clause1.Literals.Union(clause2.Literals).ToList();
                    r.Remove(clause1.Literals[i]);
                    r.Remove(clause2.Literals[j]);
                    var resolvent = new Resolvent(clause1, clause2, r.ToArray());
                    resolvents.Add(resolvent);
                }
            }
        }

        return resolvents;
    }
    
    public bool PLResolution(ISentence kb, ISentence alpha)
    {
        alpha = alpha.Negate();
        var joined = new ComplexSentence(kb, Connective.LogicSymbol.CONJUNCTION, alpha);
        var clauses = joined.GetClauseSet();
        HashSet<Clause> set = new();
        
        while (true) {
            for (var i = 0; i < clauses.Count; i++) {
                for (var j = i + 1; j < clauses.Count; j++)
                {
                    var clause1 = clauses[i];
                    var clause2 = clauses[j];
                    var unificator = new Unificator(clause1, clause2);

                    if (!unificator.IsUnifiable)
                    {
                        continue;
                        throw new Exception("Clauses are not unifiable.");
                    }
        
                    unificator.Substitute(clause1);
                    unificator.Substitute(clause2);
                    
                    var resolvents = Resolve(clauses[i], clauses[j]);
                    if(resolvents.Any(r => r.IsEmptyClause())) {
                        return true;
                    }
                    
                    set.UnionWith(resolvents);    
                }
            }
        
            if (set.IsSubsetOf(clauses))
            {
                return false;
            }
            
            clauses = clauses.Union(set).ToList();
        }
    }
    
    private bool IsResolvable(ISentence lit1, ISentence lit2)
    {
        var neg = (lit1.IsNegation && !lit2.IsNegation) || (!lit1.IsNegation && lit2.IsNegation);
        return neg && lit1.GetPredicate().Equals(lit2.GetPredicate());
    }
}