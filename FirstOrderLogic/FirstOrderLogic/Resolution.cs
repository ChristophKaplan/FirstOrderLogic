namespace FirstOrderLogic;

public class Resolution
{
    private List<Resolvent> GetResolvents(Clause clause1, Clause clause2)
    {
        var resolvents = new List<Resolvent>();

        for (var i = 0; i < clause1.Literals.Count; i++)
        {
            for (var j = i; j < clause2.Literals.Count; j++)
            {
                var lit1 = clause1.Literals[i];
                var lit2 = clause2.Literals[j];
                
                var unify = new Unificator(lit1, lit2);
                if (unify.IsUnifiable && 
                    ((lit1.IsNegation && !lit2.IsNegation) || (!lit1.IsNegation && lit2.IsNegation)))
                {
                    unify.Substitute(clause1);
                    unify.Substitute(clause2);

                    var res = clause1.Literals.Union(clause2.Literals).ToList();
                    res.Remove(clause1.Literals[i]);
                    res.Remove(clause2.Literals[j]);
                    var resolvent = new Resolvent(clause1, clause2, res.ToArray());
                    resolvents.Add(resolvent);
                }
            }
        }

        return resolvents;
    }

    public bool Resolve(ISentence KB, ISentence consequence)
    {
        consequence = consequence.Negate();
        var joined = new ComplexSentence(KB, Connective.LogicSymbol.CONJUNCTION, consequence);
        var clauses = joined.GetClauseSet();
        HashSet<Clause> set = new();

        while (true)
        {
            for (var i = 0; i < clauses.Count; i++)
            {
                for (var j = i; j < clauses.Count; j++)
                {
                    var possibleResolvents = GetResolvents(clauses[i], clauses[j]);
                    if (possibleResolvents.Any(r => r.IsEmptyClause()))
                    {
                        return true;
                    }

                    set.UnionWith(possibleResolvents);
                }
            }

            if (set.IsSubsetOf(clauses))
            {
                return false;
            }

            clauses = clauses.Union(set).ToList();
        }
    }
}