using FirstOrderLogic;
using FirstOrderLogic.Planning;
using Helpers;

var satplan = new SATPLan();

var given = new List<string>() {
    "HaveIngredients^0", 
    "NOT Food^0", 
    "Hungry^0",
    "Cook^0",
};
        
var transitions = new List<string>() {
    "Food^1 <=> Cook^0 AND HaveIngredients^0",
    "Hungry^1 <=> NOT Eat^0",
    //"Cook^0 => (HaveIngredients^0 AND Food^1 AND (NOT HaveIngredients^1))",
    //"Eat^0 => (Food^0 AND (NOT Hungry^1) AND (NOT Food^1))",
    //"(NOT Eat^0) OR (NOT Food^0)",
    //"(NOT HaveIngredients^0) => (NOT HaveIngredients^1)",
};
        
var goal = new List<string>() {
    "NOT (Hungry^2)",
};

var actions = satplan.Run(given, transitions, goal);

foreach (var action in actions) {
    Logger.Log(action.ToString());
}