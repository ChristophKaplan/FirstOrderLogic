using FirstOrderLogic;

//var propositionalLogic = new PropositionalLogic.PropositionalLogic();
//logic.Interpret("Forget((A AND (A AND B)) OR C, A)");
//logic.Interpret("Simplify(Forget((A AND (A AND B)) OR C, A))");
//propositionalLogic.Interpret("A OR B");


var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
var get = (Sentence)firstOrderLogic.TryParse("FORALL x (Human(x) OR FORALL y (Mortal(y)))");
Console.WriteLine(firstOrderLogic.Simplify(get, out var steps));
var interpretation = new Interpretation(null);
var eval = interpretation.Evaluate(get);
Console.WriteLine(eval);


/*var debugLang = new DebugLang.DebugLang();
var get2 = debugLang.TryParse("A"); 
Console.WriteLine(get2);*/
