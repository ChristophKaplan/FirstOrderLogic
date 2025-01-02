using NUnit.Framework;
using System;
using FirstOrderLogic;
using System.Collections.Generic;
using System.Linq;
using LogHelper;

namespace FolTests {
    public class Tests {
        private FirstOrderLogic.FirstOrderLogic _firstOrderLogic;
        private Interpretation _interpretation;

        [SetUp]
        public void Setup() {
            _firstOrderLogic = new FirstOrderLogic.FirstOrderLogic();
            _interpretation = Inter01();
        }

        private Interpretation Inter01() {
            IDomainOfDiscourse domain = new Domain(new Element(1), new Element(2), new Element(3), new Element(4));

            var relations = new Dictionary<string, Func<IElementOfDiscourse[], bool>>();
            var functions = new Dictionary<string, Func<Term[], IElementOfDiscourse>>();
            var variableAssignments = new Dictionary<string, IElementOfDiscourse>();
            var propositionalAssignments = new Dictionary<IProposition, bool>();

            relations.Add("Human",
                terms => terms[0] switch {
                    Element element => element.Id == 1 || element.Id == 2, _ => throw new Exception("Error: Human predicate not found.")
                });

            relations.Add("Mortal",
                terms => terms[0] switch {
                    Element element => element.Id == 1 || element.Id == 2, _ => throw new Exception("Error: Mortal predicate not found.")
                });

            return new Interpretation(domain, relations, functions, variableAssignments, propositionalAssignments);
        }

        [Test]
        public void PrenexForm() {
            var p = (ISentence)_firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
            var p2 = _firstOrderLogic.ToPrenexForm(p, out var steps);
            var shouldbe = (ISentence)_firstOrderLogic.TryParse("((NOT P(x)) OR Q(y)) AND R(z)");

            Assert.That(p2, Is.EqualTo(shouldbe));
        }
    
        [Test]
        public void Evaluation() {
            var parsed = (Sentence)_firstOrderLogic.TryParse("FORALL x (Human(x) => (Mortal(x)))");
            Assert.That(_interpretation.Evaluate(parsed), Is.EqualTo(true));
        }


        [Test]
        public void TermOrder()
        {
            var p1 = (Predicate)_firstOrderLogic.TryParse("P(a,b,c)");
            Assert.That(p1.Terms[0].TermSymbol, Is.EqualTo("a"));
            Assert.That(p1.Terms[1].TermSymbol, Is.EqualTo("b"));
            Assert.That(p1.Terms[2].TermSymbol, Is.EqualTo("c"));
        }

        [Test]
        public void Unification() {
            var p1 = (ISentence)_firstOrderLogic.TryParse("P(x,y,y)");
            var p2 = (ISentence)_firstOrderLogic.TryParse("P(y,z,a)");
            var unificator1 = new Unificator(p1, p2);
            Logger.Log(unificator1.ToString());

            var p3 = (ISentence)_firstOrderLogic.TryParse("P(x,y,y)");
            var p4 = (ISentence)_firstOrderLogic.TryParse("P(f(y),y,x)");
            var unificator2 = new Unificator(p3, p4);
            Logger.Log(unificator2.ToString());

            var p5 = (ISentence)_firstOrderLogic.TryParse("P(f(x),a,x)");
            var p6 = (ISentence)_firstOrderLogic.TryParse("P(f(g(y)),z,z)");
            var unificator3 = new Unificator(p5, p6);
            Logger.Log(unificator3.ToString());
        
            var r1 = (ISentence)_firstOrderLogic.TryParse("R(x)");
            var s1 = (ISentence)_firstOrderLogic.TryParse("S(x)");
            var unificator4 = new Unificator(r1, s1);
            Logger.Log(unificator4.ToString());
        
            Assert.That(unificator1.IsUnifiable, Is.EqualTo(true));
            Assert.That(unificator2.IsUnifiable, Is.EqualTo(false));
            Assert.That(unificator3.IsUnifiable, Is.EqualTo(false));
            Assert.That(unificator4.IsUnifiable, Is.EqualTo(false));
        }

        [Test]
        public void ConjunctiveNormalForm() {
            var p = (ISentence)_firstOrderLogic.TryParse("P(x) => (P(y) AND Q(z))");
            var p2 = _firstOrderLogic.ToConjunctiveNormalForm(p, out var steps);

            Logger.Log(p.ToString());
            Logger.Log(p2.ToString());

            Assert.That(p.IsCNF(), Is.EqualTo(false));
            Assert.That(p2.IsCNF(), Is.EqualTo(true));
        }

        [Test]
        public void Distribution() {
            var p = (ISentence)_firstOrderLogic.TryParse("X => (A AND (NOT B) AND (NOT C))");
            var p2 = _firstOrderLogic.ToConjunctiveNormalForm(p, out var steps);

            Logger.Log(p.ToString());
            Logger.Log(p2.ToString());

            Assert.That(false, Is.EqualTo(false));
        }


        [Test]
        public void ClauseSet() {
            var p = (ISentence)_firstOrderLogic.TryParse("(P(x) => Q(y)) AND R(z)");
            var p2 = _firstOrderLogic.ToPrenexForm(p, out var steps);
            Logger.Log(p2 + " cnf:" + p2.IsCNF());
            var set = p2.GetClauseSet();
            Logger.Log(set.Aggregate("", (current, clause) => current + clause + "\n"));
            Assert.That(set.Count, Is.EqualTo(2));
        }

        [Test]
        public void ClauseUnification() {
            var p = (ISentence)_firstOrderLogic.TryParse("P(x)");
            var q = (ISentence)_firstOrderLogic.TryParse("Q(y)");
            var notPy = (ISentence)_firstOrderLogic.TryParse("NOT P(y)");
            var clause1 = new Clause(p, q);
            var clause2 = new Clause(notPy);

            for (var i = 0; i < clause1.Literals.Count; i++) {
                for (var j = i; j < clause2.Literals.Count; j++) {
                    var unify = new Unificator(clause1.Literals[i], clause2.Literals[j]);
                    if (unify.IsUnifiable) {
                        unify.Substitute(clause1);
                        unify.Substitute(clause2);

                        Logger.Log(clause1.ToString());
                        Logger.Log(clause2.ToString());
                    }
                }
            }

            Assert.That(clause1.Literals[0].GetPredicate().Terms[0].TermSymbol, Is.EqualTo("y"));
        }

        [Test]
        public void Resolution() {
            var sentence = (ISentence)_firstOrderLogic.TryParse("(Human(Sokrates) AND (FORALL x (Human(x) => Mortal(x))))");
            var prenexForm = _firstOrderLogic.ToPrenexForm(sentence, out var steps);
            var skolemForm = _firstOrderLogic.SkolemForm(prenexForm);
            var consequence = (ISentence)_firstOrderLogic.TryParse("Mortal(Sokrates)");
            var resolution = new Resolution();

            var b = resolution.Resolve(skolemForm, consequence);

            Assert.That(b, Is.EqualTo(true));
        }

        [Test]
        public void IsPropositional() {
            var fol = (ISentence)_firstOrderLogic.TryParse("(Human(Sokrates) AND (FORALL x (Human(x) => Mortal(x))))");
            var prop = (ISentence)_firstOrderLogic.TryParse("A OR B AND C");
            Assert.That(fol.IsPropositional(), Is.EqualTo(false));
            Assert.That(prop.IsPropositional(), Is.EqualTo(true));
        }

        [Test]
        public void WalkSat() {
            var p = (ISentence)_firstOrderLogic.TryParse("(P => Q) AND R");
            var prenexForm = _firstOrderLogic.ToPrenexForm(p, out var steps);

            var sat = new SatSolvers();
            var clauseSet = prenexForm.GetClauseSet();
            var model = sat.WalkSAT(clauseSet, 0.5f, 100);
            //Console.WriteLine($"{model} models {clauseSet.Aggregate(new StringBuilder(), (sb, clause) => sb.Append(clause).Append(" AND ")).ToString().TrimEnd(" AND ".ToCharArray())}");

            Assert.That(model.Evaluate(clauseSet), Is.EqualTo(true));
        }

        [Test]
        public void GetInstancesOverTime() {
            var action = (ISentence)_firstOrderLogic.TryParse("Cook^0 => HaveIngredient^0 AND Food^1");
            var instancesOverTime = action.GetInstancesOverTime(0, 3);

            foreach (var instance in instancesOverTime) {
                Console.WriteLine(instance);
            }

            Assert.That(instancesOverTime.Count, Is.EqualTo(3));
        }
    
    
        [Test]
        public void TestUTF8() {
            var input = "¬ At(Work)";
            var parsed = (ISentence)_firstOrderLogic.TryParse(input);
            Logger.Log($"Parsed sentence: {parsed}");

            var expected = new ComplexSentence(new Connective(Connective.LogicSymbol.NEGATION), new Predicate("At", new Term[] { new Constant("Work") }));
            Assert.That(parsed.ToString(), Is.EqualTo(expected.ToString()));

            var again = expected.ToString();
            var parsedAgain = (ISentence)_firstOrderLogic.TryParse(again);
            Assert.That(parsedAgain.ToString(), Is.EqualTo(expected.ToString()));
        }
    
    }
}