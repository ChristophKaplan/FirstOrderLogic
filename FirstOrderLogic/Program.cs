using FirstOrderLogic;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();

//var p1 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
//var p2 = (ISentence)firstOrderLogic.TryParse("P(y,z,a)");
        
//var p1 = (ISentence)firstOrderLogic.TryParse("P(x,y,y)");
//var p2 = (ISentence)firstOrderLogic.TryParse("P(f(y),y,x)");

var p1 = (ISentence)firstOrderLogic.TryParse("P(f(x),a,x)");
var p2 = (ISentence)firstOrderLogic.TryParse("P(f(g(y)),z,z)");

var unificator = new Unificator(p1, p2);
        
Console.WriteLine(unificator);

unificator.Substitute(ref p1);
Console.WriteLine(p1);
unificator.Substitute(ref p2);
Console.WriteLine(p2);