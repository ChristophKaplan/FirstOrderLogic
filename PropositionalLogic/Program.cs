/*var propositionalLogic = new PropositionalLogic.PropositionalLogic();
//logic.Interpret("Forget((A AND (A AND B)) OR C, A)");
//logic.Interpret("Simplify(Forget((A AND (A AND B)) OR C, A))");
propositionalLogic.Interpret("Simplify( NOT(NOT(a)) OR NOT(b IMPLIES c))");
*/

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
var get = firstOrderLogic.TryParse("Human(Func(Xo,A(ka)))");
Console.WriteLine(get);