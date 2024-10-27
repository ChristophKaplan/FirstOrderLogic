using System.Data;
using System.Text;
using Helpers;
using LRParser.Language;

namespace FirstOrderLogic.Planning;

public class SATPLan {
    readonly FirstOrderLogic _firstOrderLogic = new ();
    readonly SatSolvers _satSolvers = new ();
    List<ISentence> transitionTimeInstances = new ();
    int toTime = 2;
    
    public (List<ISentence> given, List<ISentence> transitions, List<ISentence> goal) Parse()
    {
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

        var givenParsed = _firstOrderLogic.TryParse(given).Select(c => (ISentence)c).ToList();
        var transitionsParsed = _firstOrderLogic.TryParse(transitions).Select(c => (ISentence)c).ToList();
        var goalParsed = _firstOrderLogic.TryParse(goal).Select(c => (ISentence)c).ToList();
        
        return (givenParsed, transitionsParsed, goalParsed);
    }

    public ISentence PrepareCNF(List<ISentence> given, List<ISentence> transitions, List<ISentence> goal)
    {
        transitionTimeInstances.Clear();
        
        foreach (var trans in transitions)
        {
            var range = trans.GetInstancesOverTime(0, toTime);
            transitionTimeInstances.AddRange(range);
        }

        var allSentences = new List<ISentence>();
        allSentences.AddRange(given);
        allSentences.AddRange(transitionTimeInstances);
        allSentences.AddRange(goal);
        
        var connected = _firstOrderLogic.ConnectSentences(allSentences);
        var cnf = _firstOrderLogic.ToConjunctiveNormalForm(connected, out var steps);
        return cnf;
    }

    public void Run(ISentence cnf){
        var clauseSet = cnf.GetClauseSet();
        var model = _satSolvers.WalkSAT(clauseSet, 0.5f, 100);

        //extract plan
        var trueAssigments = model.GetPropositionsWhere(true); //consider negation

        var actions = new List<ISentence>();
        foreach (var trueAssigment in trueAssigments) {
            if (IsAction(trueAssigment, transitionTimeInstances)) {
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