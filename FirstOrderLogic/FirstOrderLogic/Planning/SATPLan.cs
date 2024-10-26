using System.Text;

namespace FirstOrderLogic.Planning;

public class SATPLan {
    FirstOrderLogic _firstOrderLogic = new FirstOrderLogic();
    
    public void DO() {
        var given = new List<string>() {
            "HaveIngredients0", "Cook0", "NOT Food0", "Hungry0",
        };
        
        var transitions = new List<string>() {
            "Cook0 => (HaveIngredients0 AND Food1)",
            "Cook1 => (HaveIngredients1 AND Food2)",
            "Eat0 => (Food0 AND NOT (Hungry1))",
            "Eat1 => (Food1 AND NOT (Hungry2))",
        };
        
        var goal = new List<string>() {
            "NOT (Hungry2)",
        };
        
        var combined = given.Concat(transitions).Concat(goal).ToList();

        var conjuncts = "";
        for (var index = 0; index < combined.Count-1; index++) {
            var s = combined[index];
            conjuncts += $" ({s}) AND";
        }
        conjuncts += $" ({combined[^1]})";

        var p = (ISentence)_firstOrderLogic.TryParse(conjuncts);
        var conjunctiveNormalForm = _firstOrderLogic.ToConjunctiveNormalForm(p, out var steps);

        var sat = new SatSolvers();
        var clauseSet = conjunctiveNormalForm.GetClauseSet();
        var model = sat.WalkSAT(clauseSet, 0.5f, 100);
        Console.WriteLine($"{model} models {clauseSet.Aggregate(new StringBuilder(), (sb, clause) => sb.Append(clause).Append(" AND ")).ToString().TrimEnd(" AND ".ToCharArray())}");
    }
}