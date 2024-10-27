using System.Data;
using System.Text;
using Helpers;
using LRParser.Language;

namespace FirstOrderLogic.Planning;

public class SATPLan {
    readonly FirstOrderLogic _firstOrderLogic = new ();
    readonly SatSolvers _satSolvers = new ();
    readonly List<ISentence> transitionTimeInstances = new ();
    readonly int toTime = 2;
    readonly int fromTime = 0;
    
    public List<ISentence> Run(List<string> given, List<string> transitions, List<string> goal) {
        var parsed = Parse(given, transitions, goal);
        var merged = InstantiateAndMerge(parsed.given, parsed.transitions, parsed.goal);
        var cnf = _firstOrderLogic.ToConjunctiveNormalForm(merged, out var steps);
        var model = Solve(cnf);
        var actions = Extract(model);
        
        Debug(model, cnf);
        return actions;
    }
    
    private (List<ISentence> given, List<ISentence> transitions, List<ISentence> goal) Parse(List<string> given, List<string> transitions, List<string> goal)
    {
        var givenParsed = _firstOrderLogic.TryParse(given).Select(c => (ISentence)c).ToList();
        var transitionsParsed = _firstOrderLogic.TryParse(transitions).Select(c => (ISentence)c).ToList();
        var goalParsed = _firstOrderLogic.TryParse(goal).Select(c => (ISentence)c).ToList();
        
        return (givenParsed, transitionsParsed, goalParsed);
    }

    private ISentence InstantiateAndMerge(List<ISentence> given, List<ISentence> transitions, List<ISentence> goal)
    {
        transitionTimeInstances.Clear();
        
        foreach (var trans in transitions)
        {
            var range = trans.GetInstancesOverTime(fromTime, toTime);
            transitionTimeInstances.AddRange(range);
        }

        var allSentences = new List<ISentence>();
        allSentences.AddRange(given);
        allSentences.AddRange(transitionTimeInstances);
        allSentences.AddRange(goal);
        
        return _firstOrderLogic.ConnectSentences(allSentences);
    }

    private PossibleWorld Solve(ISentence cnf){
        var clauseSet = cnf.GetClauseSet();
        return _satSolvers.WalkSAT(clauseSet, 0.5f, 100);
    }

    private List<ISentence> Extract(PossibleWorld model) {
        var actions = new List<ISentence>();

        foreach (var assigment in model._propositionalAssignment.Keys) {
            if (IsAction(assigment, transitionTimeInstances)) {

                if (model._propositionalAssignment[assigment]) {
                    actions.Add(assigment);
                }
            }
        }
        
        return actions;
    }

    private bool IsAction(IProposition premise, List<ISentence> transitionInstances) {
        foreach (var instance in transitionInstances) {
            if (instance.IsImplicationAndEqualPremise(premise)) {
                return true;
            }
        }

        return false;
    }

    private void Debug(PossibleWorld model, ISentence cnf) {
        Logger.Log($"{model} models {cnf}");
    }
}