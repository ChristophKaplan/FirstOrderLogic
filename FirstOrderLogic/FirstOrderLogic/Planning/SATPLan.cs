using System.Data;
using System.Text;
using Helpers;
using LRParser.Language;

namespace FirstOrderLogic.Planning;

public class SATPLan {
    readonly FirstOrderLogic _firstOrderLogic = new ();
    readonly SatSolvers _satSolvers = new ();
    
    public void SetUp() {
        var given = new List<string>() {
            "HaveIngredients^0", 
            "Cook^0", 
            "NOT Food^0", 
            "Hungry^0",
        };
        
        var transitions = new List<string>() {
            "Cook^0 => (HaveIngredients^0 AND Food^1)",
            "Eat^0 => (Food^0 AND NOT (Hungry^1))",
        };
        
        var goal = new List<string>() {
            "NOT (Hungry^2)",
        };

        var givenParsed = _firstOrderLogic.TryParse(given);
        var transitionsParsed = _firstOrderLogic.TryParse(transitions);
        var goalParsed = _firstOrderLogic.TryParse(goal);
        
        int toTime = 2;
        var transitionInstances = new List<ISentence>();
        foreach (var trans in transitionsParsed) {
            var range = ((ISentence)trans).GetInstancesOverTime(0, toTime);
            transitionInstances.AddRange(range);
        }

        //to CNF
        var all = new List<ISentence>();
        all.AddRange(givenParsed.Select(s => (ISentence)s));
        all.AddRange(transitionInstances);
        all.AddRange(goalParsed.Select(s => (ISentence)s));
        var kb = _firstOrderLogic.ConnectSentences(all);
        var cnf = _firstOrderLogic.ToConjunctiveNormalForm(kb, out var steps);
        
        //solve
        var clauseSet = cnf.GetClauseSet();
        var model = _satSolvers.WalkSAT(clauseSet, 0.5f, 100);

        //extract plan
        var trueAssigments = model.GetPropositionsWhere(true); //consider negation

        var actions = new List<ISentence>();
        foreach (var trueAssigment in trueAssigments) {
            if (IsAction(trueAssigment, transitionInstances)) {
                actions.Add(trueAssigment);
            }
        }

        foreach (var action in actions) {
            Logger.Log(action.ToString());
        }
    }

    private bool IsAction(IProposition trueAssigment, List<ISentence> transitionInstances) {
        foreach (var instance in transitionInstances) {
            if (instance.IsImplicationAndEqualPremise(trueAssigment)) {
                return true;
            }
        }

        return false;
    }
}