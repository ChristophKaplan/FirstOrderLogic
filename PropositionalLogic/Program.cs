using FirstOrderLogic;

var propositionalLogic = new PropositionalLogic.PropositionalLogic();
//logic.Interpret("Forget((A AND (A AND B)) OR C, A)");
//logic.Interpret("Simplify(Forget((A AND (A AND B)) OR C, A))");
propositionalLogic.Interpret("A OR B");


var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
var get = firstOrderLogic.TryParse("!FORALL x Hum(x,Func(y)) <=> TRUE");
Console.WriteLine(firstOrderLogic.Simplify((Sentence)get, out var steps));
    
/*var debugLang = new DebugLang.DebugLang();
var get2 = debugLang.TryParse("A"); 
Console.WriteLine(get2);*/
