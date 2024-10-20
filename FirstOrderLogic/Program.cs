using System.Text;
using FirstOrderLogic;
Console.OutputEncoding = Encoding.UTF8;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();

var PpImpliesQ = (ISentence)firstOrderLogic.TryParse("(P(x) AND (P(x) => Q(y)))");
var PpImpliesQ2 = firstOrderLogic.Simplify(PpImpliesQ, out var steps);
var q = (ISentence)firstOrderLogic.TryParse("Q(y)");
var resolution = new Resolution();
var b = resolution.PLResolution(PpImpliesQ2, q);