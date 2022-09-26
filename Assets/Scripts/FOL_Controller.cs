using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FirstOrderLogic;
using System.Linq;


//Testing
public class TestElement : Universe.Element {
    private string name;
    public TestElement(string name) {
        this.name = name;
    }
    public string GetName() => this.name;
}

public class TestUniverse : Universe {
    private Dictionary<int, Element> entites = new Dictionary<int, Element>();

    public override void AddElement(Element element) {
        entites.Add(element.GetHashCode(), element);
    }
    public override void AddElements(List<Element> elements) {
        for (int i = 0; i < elements.Count; i++) AddElement(elements[i]);        
    }
    public override List<int> GetAllElementIDs() => entites.Keys.ToList();    

    public override Element GetElementById(int id) {
        Element temp = null;
        entites.TryGetValue(id,out temp);
        if(temp == null) Debug.LogError("cant find element " + id);        
        return temp;
    }

    public override void RemoveElement(Element element) {
        entites.Remove(element.GetHashCode());
    }

    public Element GetElementByName(string name) {
        foreach (Element el in entites.Values) {
            if (name.Equals(((TestElement)el).GetName())) return el;
        }
        Debug.LogError("cant find element " + name);
        return null;
    }

}



public class FOL_Controller : MonoBehaviour {

    public LogicSystemInterface logicSystemInterface = new LogicSystemInterface();


    void Awake() {
        SetupSystem();
        SystemTest();
    }


    private void SystemTest() {
        //Tokenizer tokenizer = new Tokenizer();


        string hundeDieBellenBeissenNicht = "∀x(((Hund(x))∧(Bellen(x)))→(¬(Beissen(x))))";
        Sentence s = logicSystemInterface.SringToSentence(hundeDieBellenBeissenNicht, true);
        Debug.Log("Hunde die Bellen beissen nicht : " + s);
        s.PrintSyntaxTree();
        logicSystemInterface.AddSentence(s);


        Sentence pnf = logicSystemInterface.GetPrenexNormalForm(s);
        Debug.Log("pnf:" + pnf);

        Sentence skolem = logicSystemInterface.GetSkolemForm(pnf, logicSystemInterface.GetInterpretations()[0], logicSystemInterface.GetVariableAssignment());
        Debug.Log("skolemform:" + skolem);

        //Sentence cnf = logicSystemInterface.GetConjunktiveNormalForm(skolem);
        //Debug.Log("KNF:" + cnf);

        ClauseSet set = logicSystemInterface.GetClauseSet(skolem);
        Debug.Log("clauseform:" + set.ToString());


        logicSystemInterface.PrintSentencesWithTruthValue();
    }

    private void SetupSystem() {
        //elements
        logicSystemInterface.AddUniverse(new TestUniverse());

        List<Universe.Element> humanlist = new List<Universe.Element> { new TestElement("Tim")};
        List<Universe.Element> roboDoglist = new List<Universe.Element> { new TestElement("Aibo") };
        List<Universe.Element> doglist = new List<Universe.Element> { new TestElement("Bello"), new TestElement("Pluto"), new TestElement("Rex"), new TestElement("Rocky"), new TestElement("Struppi") };
        List<Universe.Element> dogBark = new List<Universe.Element> { roboDoglist[0], doglist[0], doglist[1], doglist[4] };
        List<Universe.Element> dogBite = new List<Universe.Element> { roboDoglist[0], doglist[2], doglist[3] };

        logicSystemInterface.AddElements(humanlist);
        logicSystemInterface.AddElements(doglist);


        //symbols
        logicSystemInterface.AddPredicateSymbol(new PredicateSymbol("Mensch", 1));
        logicSystemInterface.AddPredicateSymbol(new PredicateSymbol("Hund", 1));
        logicSystemInterface.AddPredicateSymbol(new PredicateSymbol("RoboHund", 1));
        logicSystemInterface.AddPredicateSymbol(new PredicateSymbol("Bellen", 1));
        logicSystemInterface.AddPredicateSymbol(new PredicateSymbol("Beissen", 1));
                        
        //one interpretation for now
        Interpretation interpretation = new Interpretation(logicSystemInterface.GetStructure(), logicSystemInterface.GetSignature());
        logicSystemInterface.AddInterpretation(interpretation);

        //relations
        logicSystemInterface.AddPredicateRelation(interpretation, "Mensch", humanlist);
        logicSystemInterface.AddPredicateRelation(interpretation, "RoboHund", roboDoglist);
        logicSystemInterface.AddPredicateRelation(interpretation, "Hund",doglist);
        logicSystemInterface.AddPredicateRelation(interpretation, "Bellen", dogBark);
        logicSystemInterface.AddPredicateRelation(interpretation, "Beissen", dogBite);
    }



}


