using FirstOrderLogic;

var firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
var human = (Sentence)firstOrderLogic.TryParse("Human(f(g(X),X))");
var mortal = (Sentence)firstOrderLogic.TryParse("Mortal(f(Y,a))");
        
var unificator = new Unificator(human, mortal);
        
Console.WriteLine(unificator);