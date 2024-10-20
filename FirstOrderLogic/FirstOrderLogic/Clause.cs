namespace FirstOrderLogic;

public class Clause
{ 
    public List<ISentence> Literals{ get; private set; }
    
    public Clause(params ISentence[] literals)
    {
        if (literals.Any(t => !t.IsLiteral)) return;
        Literals = new List<ISentence>(literals);
    }
    
    public void AddLiteral(ISentence l)
    {
        if (Literals == null) Literals = new List<ISentence>();
        if (Literals.Contains(l)) return;
        
        if (!l.IsLiteral)
        {
            throw new Exception("is not a literal");
        }
        
        Literals.Add(l);
    }
}

public class Resolvent : Clause {
    private readonly Clause _parentClause1;
    private readonly Clause _parentClause2;

    public bool IsEmptyClause() => Literals == null || Literals.Count == 0;

    public Resolvent(Clause clause1, Clause clause2, params ISentence[] literals) : base(literals) {
        _parentClause1 = clause1;
        _parentClause2 = clause2;
    }
    
    public string ResolventAsString() {
        var s = "";
        s += "k1: " + _parentClause1 + "\n";
        s += "k2: " + _parentClause2 + "\n";
        s += "-----------------------" + "\n";
        s += "res: " + ToString() + "\n";
        return s;
    }
    
    public bool IsCircular(Clause k1, Clause k2) {
        if (_parentClause1.Equals(k1))
            if (_parentClause2.Equals(k2))
                return true;
        if (_parentClause1 is Resolvent) return ((Resolvent)_parentClause1).IsCircular(_parentClause1, k2);
        return false;
    }

    public string TraceResolution() {
        var s = "";

        if (_parentClause1 is Resolvent) {
            var parent1 = (Resolvent)_parentClause1;
            s += parent1.TraceResolution() + "\n";
        }

        if (_parentClause2 is Resolvent) {
            var parent2 = (Resolvent)_parentClause2;
            s += parent2.TraceResolution() + "\n";
        }

        if (this is Resolvent)
            s += ResolventAsString() + "\n";
        else
            s += "literal:\n" + ToString() + "\n";

        return s;
    }
}