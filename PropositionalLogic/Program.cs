using PropositionalLogic.Helpers;

var logic = new PropositionalLogic.PropositionalLogic();

/*
var c = InputCreator.GeneratePropositionalSentences(3);
c.ForEach(x => {
    Console.WriteLine(x);
});
Console.WriteLine("\n");
logic.Interpret(c.ToArray());
*/


var f1 = "(A OR B) AND C";
var f2 = "(A AND B) OR C";

string forgetVar = "A";
ForgetCompare fc1 = new ForgetCompare(f1, forgetVar);
ForgetCompare fc2 = new ForgetCompare(f2, forgetVar);

forgetVar = "C";
ForgetCompare fc3 = new ForgetCompare(f1, forgetVar);
ForgetCompare fc4 = new ForgetCompare(f2, forgetVar);

InputOutput.WriteFile("forgetCompare.html", fc1.ToHTML() + "<br>" + fc2.ToHTML() + "<br>" + fc3.ToHTML() + "<br>" + fc4.ToHTML() );
//InputOutput.OpenFile("test.html");


logic.Interpret(
    $"Int(Forget({f1},{forgetVar}))",
    $"Mod({forgetVar})"
    );