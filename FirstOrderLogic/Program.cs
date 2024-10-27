using FirstOrderLogic;
using FirstOrderLogic.Planning;
using Helpers;

var satplan = new SATPLan();

var given = new List<string>() {
    "HaveIngredients^0", 
    "Cook^0",
    "NOT Food^0", 
    "Hungry^0",
};
        
var transitions = new List<string>() {
    "Cook^0 => (HaveIngredients^0 AND Food^1 AND (NOT HaveIngredients^1))",
    "Eat^0 => (Food^0 AND (NOT Hungry^1) AND (NOT Food^1))",
};
        
var goal = new List<string>() {
    "NOT (Hungry^2)",
};

var actions = satplan.Run(given, transitions, goal);

foreach (var action in actions) {
    Logger.Log(action.ToString());
}