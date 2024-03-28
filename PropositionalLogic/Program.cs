var logic = new PropositionalLogic.PropositionalLogic();

/*
var c = InputCreator.GeneratePropositionalSentences(3);
c.ForEach(x => {
    Console.WriteLine(x);
});
Console.WriteLine("\n");
logic.Interpret(c.ToArray());
*/


logic.Interpret(new []{
   "(A AND B) OR (C AND D)",
});