var logic = new PropositionalLogic.PropositionalLogic();

logic.Interpret("Forget((A AND (A AND B)) OR C, A)");
logic.Interpret("Simplify(Forget((A AND (A AND B)) OR C, A))");

//logic.Interpret("Simplify((TRUE AND (TRUE AND B)) OR C)");