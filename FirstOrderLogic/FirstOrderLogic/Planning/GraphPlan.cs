namespace FirstOrderLogic.Planning;

public class Problem {
    public List<Clause> Goal { get; set; }
}

public class Graph {
    public Graph(Problem problem) {
        throw new NotImplementedException();
    }

    public bool StateNotMutex(int tl, List<Clause> goals) {
        throw new NotImplementedException();
    }

    public int NumLevels() {
        throw new NotImplementedException();
    }

    public List<Clause> ExtractSolution(List<Clause> goals, int numLevels, List<Clause> nogoods) {
        throw new NotImplementedException();
    }

    public bool Balanced(List<Clause> nogoods) {
        throw new NotImplementedException();
    }

    public void ExpandGraph(Problem problem) {
        throw new NotImplementedException();
    }
}

public class GraphPlan {
    public List<Clause> Run(Problem problem) {
        var graph = new Graph(problem);
        var goals = problem.Goal;
        var nogoods = new List<Clause>();
        
        int tl = 0;
        while (true) {
            if (graph.StateNotMutex(tl, goals)) {
                var solution = graph.ExtractSolution(goals, graph.NumLevels(), nogoods);
                if (solution != null) {
                    return solution;
                }
            }

            if (graph.Balanced(nogoods)) {
                return null;
            }

            graph.ExpandGraph(problem);
            tl++;
        }
    }
}