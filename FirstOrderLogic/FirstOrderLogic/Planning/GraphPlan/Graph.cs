using System.Text;

namespace FirstOrderLogic.Planning.GraphPlan;

public struct Layer
{
    public readonly char Type;
    public readonly int Level;

    public Layer(char type, int level)
    {
        this.Type = type;
        this.Level = level;
    }

    public override string ToString()
    {
        return $"{Type}{Level}";
    }
}

public class Graph
{
    private readonly Dictionary<Layer, List<Node>> _nodes = new();
    private readonly List<Action> _actions;
    private List<ISentence> _initialState;
    private List<ISentence> _goal;
    public int NumLevels => _nodes.Count;

    public Graph(List<ISentence> initialState, List<ISentence> goal, List<Action> actions)
    {
        _initialState = initialState;
        _goal = goal;
        _actions = actions;

        var initialLayer = new Layer('S', 0);
        foreach (var sentence in initialState)
        {
            var stateNode = new StateNode(initialLayer, sentence);
            TryAddToLayer(initialLayer, stateNode);
        }
    }

    public List<StateNode> GetLastStateNodes()
    {
        for (var i = _nodes.Count; i >= 0; i--)
        {
            var a = _nodes.ElementAt(i);
            if (a.Key.Type == 'S')
            {
                return a.Value.Select(n => (StateNode)n).ToList();
            }
        }

        return null;
    }

    public List<ActionNode> GetLastActionNodes()
    {
        for (var i = _nodes.Count; i >= 0; i--)
        {
            var a = _nodes.ElementAt(i);
            if (a.Key.Type == 'A')
            {
                return a.Value.Select(n => (ActionNode)n).ToList();
            }
        }

        return null;
    }

    private Node TryGetFromLayer(Layer layer, Node node)
    {
        _nodes.TryGetValue(layer, out var layerNodes);
        if (layerNodes != null)
        {
            return layerNodes.FirstOrDefault(n => n.Equals(node));
        }

        return null;
    }

    private void TryAddToLayer(Layer layer, Node node)
    {
        _nodes.TryGetValue(layer, out var layerNodes);
        if (layerNodes == null)
        {
            layerNodes = new List<Node>();
            _nodes.Add(layer, layerNodes);
        }

        var contained = layerNodes.FirstOrDefault(n => n.Equals(node));
        if (contained != null)
        {
            MergeRelations(node, contained);
            return;
        }

        layerNodes.Add(node);
    }

    private void MergeRelations(Node node, Node contained)
    {
        foreach (var n in node.InEdges)
        {
            contained.ConnectTo(n);
        }

        foreach (var n in node.OutEdges)
        {
            contained.ConnectTo(n);
        }

        foreach (var n in node.MutexRelation)
        {
            contained.MutexRelation.Add(n);
        }
    }

    public bool StateNotMutex(int i, List<ISentence> goals)
    {
        var elementAt =
            _nodes.FirstOrDefault(n => n.Key.Type == 'S' && n.Key.Level == i);

        List<StateNode> state = elementAt.Value.Select(n => (StateNode)n).ToList();
        var hasAllGoals = state.All(s => goals.Contains(s.Literal));
        return hasAllGoals && !state.Any(s => s.MutexRelation.Count > 0);
    }

    public List<ISentence> ExtractSolution(List<ISentence> goals, int numLevels, List<ISentence> nogoods)
    {
        //
    }

    public bool Balanced()
    {
        var count = _nodes.Count;

        if (count < 3)
        {
            return false;
        }

        var lastState = _nodes.ElementAt(count - 1);
        if (lastState.Key.Type != 'S')
        {
            throw new Exception("last layer is not a state layer");
        }

        var prevState = _nodes.ElementAt(count - 3);

        return prevState.Value.All(prevStateNode =>
            lastState.Value.Any(lastStateNode =>
                ((StateNode)prevStateNode).Literal.Equals(((StateNode)lastStateNode).Literal)));
    }

    public void ExpandGraph()
    {
        ExpandLastLayer();
        ExpandLastLayer();
    }

    private void ExpandLastLayer()
    {
        var currentLayer = _nodes.Last();
        switch (currentLayer.Key.Type)
        {
            case 'S':
                ExpandStateNodes(currentLayer.Key, currentLayer.Value.Select(n => (StateNode)n).ToList());
                break;
            case 'A':
                ExpandActionNodes(currentLayer.Key, currentLayer.Value.Select(n => (ActionNode)n).ToList());
                break;
        }
    }

    private void ExpandStateNodes(Layer layer, List<StateNode> stateNodeList)
    {
        var newLayer = new Layer('A', layer.Level);

        foreach (var action in _actions)
        {
            var isApplicable = action.IsApplicable(stateNodeList, out var satNodes);
            if (isApplicable)
            {
                var actionNode = new ActionNode(newLayer, action, false);
                satNodes.Select(n => (Node)n).ToList().ForEach(p => p.ConnectTo(actionNode));
                TryAddToLayer(newLayer, actionNode);
            }
        }

        foreach (var stateNode in stateNodeList)
        {
            var action = new Action("Persist", new List<ISentence> { stateNode.Literal },
                new List<ISentence> { stateNode.Literal });
            var actionNode = new ActionNode(newLayer, action, true);
            stateNode.ConnectTo(actionNode);
            TryAddToLayer(newLayer, actionNode);
        }

        ScanMutex(newLayer);
    }

    private void ExpandActionNodes(Layer layer, List<ActionNode> actionNodes)
    {
        var newLayer = new Layer('S', layer.Level + 1);
        foreach (var actionNode in actionNodes)
        {
            foreach (var effect in actionNode.Action.Effects)
            {
                var effectNode = new StateNode(newLayer, effect);
                actionNode.ConnectTo(effectNode);
                TryAddToLayer(newLayer, effectNode);
            }
        }

        ScanMutex(newLayer);
    }

    private void ScanMutex(Layer layer)
    {
        _nodes.TryGetValue(layer, out var layerNodes);
        if (layerNodes == null)
        {
            return;
        }

        foreach (var n1 in layerNodes)
        {
            foreach (var n2 in layerNodes)
            {
                if (!n1.Equals(n2) && IsMutex(n1, n2))
                {
                    TryAddMutexrelation(n1, n2);
                }
            }
        }
    }

    private void TryAddMutexrelation(Node n1, Node n2)
    {
        if (!n1.MutexRelation.Contains(n2))
        {
            n1.MutexRelation.Add(n2);
        }

        if (!n2.MutexRelation.Contains(n1))
        {
            n2.MutexRelation.Add(n1);
        }
    }

    private bool IsMutex(Node n1, Node n2)
    {
        if (n1.Equals(n2)) return false;

        return n1 switch
        {
            StateNode s1 when n2 is StateNode s2 => IsInconsistentSupport(s1, s2) ||
                                                    IsInconsistent(s1.Literal, s2.Literal),
            ActionNode a1 when n2 is ActionNode a2 => IsInconsistentEffects(a1, a2) || IsInterference(a1, a2) ||
                                                      IsConflictingNeeds(a1, a2),
            _ => throw new Exception("Invalid node type")
        };
    }

    private bool IsInconsistentEffects(ActionNode a1, ActionNode a2)
    {
        return a1.Action.Effects.Any(effect1 => a2.Action.Effects.Any(effect2 => IsInconsistent(effect1, effect2)));
    }

    private bool IsInterference(ActionNode a1, ActionNode a2)
    {
        return a1.Action.Effects.Any(effect => a2.Action.Preconditions.Any(preCon => IsInconsistent(effect, preCon))) ||
               a2.Action.Effects.Any(effect => a1.Action.Preconditions.Any(preCon => IsInconsistent(preCon, effect)));
    }

    private bool IsConflictingNeeds(ActionNode a1, ActionNode a2)
    {
        return a1.Action.Preconditions.Any(preCon1 =>
            a2.Action.Preconditions.Any(preCon2 => IsInconsistent(preCon1, preCon2)));
    }

    private bool IsInconsistentSupport(StateNode s1, StateNode s2)
    {
        foreach (var n1 in s1.InEdges)
        {
            foreach (var n2 in s2.InEdges)
            {
                if (IsMutex(n1, n2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsInconsistent(ISentence s1, ISentence s2)
    {
        if (s1.IsNegation && !s2.IsNegation && s1.Children[0].Equals(s2))
        {
            return true;
        }

        if (!s1.IsNegation && s2.IsNegation && s1.Equals(s2.Children[0]))
        {
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var layer in _nodes)
        {
            sb.AppendLine($"Layer {layer.Key}");
            foreach (var node in layer.Value)
            {
                sb.AppendLine(node.ToString());
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}