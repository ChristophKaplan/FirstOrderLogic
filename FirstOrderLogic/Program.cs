using System.Text;
using FirstOrderLogic;
Console.OutputEncoding = Encoding.UTF8;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();

var p = (ISentence)firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
var p2 = firstOrderLogic.Simplify(p, out var steps);
Console.WriteLine(p2 +" cnf:"+ p2.IsCNF());
var set = p2.GetClauseSet();
Console.WriteLine(set.Aggregate("", (s, list) => s + list.Aggregate("", (s, l) => s + l + " ") + ", "));
