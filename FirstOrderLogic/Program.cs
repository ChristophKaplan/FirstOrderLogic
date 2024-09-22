using FirstOrderLogic;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
var get = (Sentence)firstOrderLogic.TryParse("FORALL x (Human(x) OR FORALL y (Mortal(y)))");
Console.WriteLine(firstOrderLogic.Simplify(get, out var steps));
var interpretation = new Interpretation(null);
var eval = interpretation.Evaluate(get);
Console.WriteLine(eval);