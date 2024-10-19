using FirstOrderLogic;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();

var p = (ISentence)firstOrderLogic.TryParse("FORALL x FORALL y EXISTS z EXISTS w (P(x,y,z,w))");
var p2 = firstOrderLogic.SkolemForm(p);

Console.WriteLine(p +" cnf:"+ p.IsCNF());
Console.WriteLine(p2 +" cnf:"+ p2.IsCNF());