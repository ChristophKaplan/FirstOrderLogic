var logic = new PropositionalLogic.PropositionalLogic();

//logic.Interpret("Forget((A AND (A AND B)) OR C, A)");
//logic.Interpret("Simplify(Forget((A AND (A AND B)) OR C, A))");

logic.Interpret("Simplify( NOT(NOT(a)) OR NOT(b IMPLIES c))");