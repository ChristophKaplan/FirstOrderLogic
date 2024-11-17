namespace FirstOrderLogic.Planning.GraphPlan;

public class GraphPlan {
    public List<ISentence> Run(List<ISentence> initialState, List<ISentence> goals, List<Action> actions) {
        var graph = new Graph(initialState, goals, actions);
        var nogoods = new List<ISentence>();

        int tl = 0;
        while (true) {
            if (graph.StateNotMutex(tl, goals)) {
                var solution = graph.ExtractSolution(goals, graph.NumLevels, nogoods);
                if (solution != null) {
                    return solution;
                }
            }

            if (graph.Balanced()) {
                return null;
            }

            graph.ExpandGraph();
            tl++;
        }
    }
}