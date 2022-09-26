using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


namespace FirstOrderLogic {

    public enum TokenType { Negation, Conjunction, Disjunction, Implication, Biconditional, Univeral, Existential, Predicate, Function, Variable }

    public class TokenDefinition {
        private Regex regex;
        private readonly TokenType type;

        public TokenDefinition(TokenType type, string regexPattern) {
            regex = new Regex(regexPattern, RegexOptions.Compiled);
            this.type = type;
        }

        public IEnumerable<TokenMatch> FindMatches(string inputString) {
            MatchCollection matches = regex.Matches(inputString);
            for (int i = 0; i < matches.Count; i++) {
                yield return new TokenMatch(type, matches[i]);
            }
        }
    }

    public class TokenMatch {
        public TokenType tokenType;
        public string value;
        public string value2;
        public List<string> children;
        public int startIndex;
        public int endIndex;


        public TokenMatch(TokenType tokenType, Match match) {
            this.tokenType = tokenType;
            this.value = match.Value;
            this.startIndex = match.Index;
            this.endIndex = match.Index + match.Length;

            List<string> children = new List<string>();
            if (tokenType == TokenType.Predicate) {                
                for (int j = 2; j < match.Groups.Count; j++) children.Add(match.Groups[j].ToString());
                
                this.value2 = match.Groups[1].ToString();

            } else if (tokenType == TokenType.Existential || tokenType == TokenType.Univeral) {
                for (int j = 2; j < match.Groups.Count; j++) children.Add(match.Groups[j].ToString());

                this.value2 = match.Groups[1].ToString();

            } else {
                for (int j = 1; j < match.Groups.Count; j++) children.Add(match.Groups[j].ToString());
            }

            this.children = children;

            
        }



        public string GetChildrenAsString() {
            string s = "";
            for (int i = 0; i < this.children.Count; i++) {
                s += this.children[i] + ", ";
            }

            return s;
        }

    }


    public class Token {
        public TokenType type;
        public string value;
        public string value2;
        public List<Token> children;
        public Token(TokenType tokenType, string value,string value2, List<Token> children) {
            this.type = tokenType;
            this.value = value;
            this.value2 = value2;
            this.children = children;
        }

        public override string ToString() {
            string s = "";

            s += "t:" + type + " - " + value;
            s += "\n";
            for (int i = 0; i < children.Count; i++) {
                s += children[i];
            }

            return s;
        }

        public Sentence GetAsSentence() {

           switch (type) {
                case TokenType.Negation:
                return new ComplexSentence(children[0].GetAsSentence(), OperatorType.negation);

                case TokenType.Conjunction:
                return new ComplexSentence(children[0].GetAsSentence(), children[1].GetAsSentence(), OperatorType.conjunction);

                case TokenType.Disjunction:
                return new ComplexSentence(children[0].GetAsSentence(), children[1].GetAsSentence(), OperatorType.disjunction);

                case TokenType.Implication:
                return new ComplexSentence(children[0].GetAsSentence(), children[1].GetAsSentence(), OperatorType.implication);

                case TokenType.Biconditional:
                return new ComplexSentence(children[0].GetAsSentence(), children[1].GetAsSentence(), OperatorType.biconditional);

                case TokenType.Univeral:
                return new ComplexSentence(children[0].GetAsSentence(), OperatorType.universal, value2);

                case TokenType.Existential:
                return new ComplexSentence(children[0].GetAsSentence(), OperatorType.existential, value2);

                case TokenType.Predicate:
                Term[] terms = new Term[children.Count];
                for (int i = 0; i < children.Count; i++) {
                    terms[i] = children[i].GetAsTerm();
                }
                return new AtomicSentence(new PredicateSymbol(value2,terms.Length),terms);
            }

            Debug.LogError("Problem:"+ToString());
            return null;
        }
        private Term GetAsTerm() {
            switch (type) {
                case TokenType.Function:
                Term[] terms = new Term[children.Count];
                for (int i = 0; i < children.Count; i++) {
                    terms[i] = children[i].GetAsTerm();
                }
                return new FunctionTerm(new FunctionSymbol(value2,terms.Length),terms);

                case TokenType.Variable:
                return new VariableTerm(value);

            }

            Debug.LogError("Problem:" + ToString());
            return null;
        }

    }


    public class Tokenizer {
        List<TokenDefinition> tokenDefinitions;
        public Tokenizer() {

            tokenDefinitions = new List<TokenDefinition>();

            tokenDefinitions.Add(new TokenDefinition(TokenType.Biconditional, @"([(].*[)])[↔]([(].*[)])"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Implication, @"([(].*[)])[→]([(].*[)])"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Disjunction, @"([(].*[)])[∨]([(].*[)])"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Conjunction, @"([(].*[)])[∧]([(].*[)])"));

            tokenDefinitions.Add(new TokenDefinition(TokenType.Negation, @"[¬][(](.*)[)]"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Existential, @"[∃]([xyz])[(]?(.*)[)]?"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Univeral, @"[∀]([xyz])[(]?(.*)[)]?"));

            tokenDefinitions.Add(new TokenDefinition(TokenType.Predicate, @"(\p{Lu}+[^∀∃¬∧∨→↔]*)[(]([^∀∃¬∧∨→↔]*)[)]"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Function, @"(\p{Ll}+[^∀∃¬∧∨→↔]*)[(]([^∀∃¬∧∨→↔]*)[)]"));
            tokenDefinitions.Add(new TokenDefinition(TokenType.Variable, @"\w+"));


            string s0b = "∃x(P(x)∨Q(x))";
            string s0 = "∀y(∃x(P(Max,y)∨Q(x)))";
            string s1 = "(((∀x(Mensch(x) → Sich-irren(x))) ∧ Mensch(Max)) ∧ (¬(∃y (Sich-irren(y)))))";

            string s2 = "(A()→(B()∧(¬(C()) ↔ (¬(¬(E())∨F())))))";

            string s5 = "(((∀x(M(x)→I(x)))∧M(Max))∧(¬(∃y(I(y)))))";

            List<Token> t = Tokenize(s5);
            foreach (Token item in t) {
                Debug.Log("token:" + item +"\n"+ item.GetAsSentence());
            }
        }


        public List<Token> Tokenize(string message) {
            List<TokenMatch> tokenMatches = FindTokenMatches(message);

            foreach (TokenMatch item in tokenMatches) {
                Debug.Log("Tokenmatch: type:" + item.tokenType + " value:" + item.value + " value2:" + item.value2 + " children:" + item.GetChildrenAsString());
            }

            List<Token> tokens = AnalyzeRecursive(tokenMatches);
            return tokens;
        }

        private List<Token> AnalyzeRecursive(List<TokenMatch> matches) {
            List<Token> tokens = new List<Token>();

            for (int i = 0; i < matches.Count; i++) {
                List<Token> children = new List<Token>();

                for (int j = 0; j < matches[i].children.Count; j++) {
                    string child = matches[i].children[j];
                    List<TokenMatch> childMatches = FindTokenMatches(child);

                    foreach (TokenMatch item in childMatches) {
                        Debug.Log("Tokenmatch: type:" + item.tokenType + " value:" + item.value + " value2:" + item.value2 + " children:" + item.GetChildrenAsString());
                    }

                    children.AddRange(AnalyzeRecursive(childMatches));
                }

                Token token = new Token(matches[i].tokenType, matches[i].value, matches[i].value2, children);
                tokens.Add(token);
            }

            return tokens;
        }


        private List<TokenMatch> FindTokenMatches(string message) {
            List<TokenMatch> tokenMatches = new List<TokenMatch>();

            foreach (var tokenDefinition in tokenDefinitions) {
                List<TokenMatch> cur = tokenDefinition.FindMatches(message).ToList();
                tokenMatches.AddRange(cur);
                if (cur.Count > 0) break;
            }

            return tokenMatches;
        }
    }




    public class OldTokenizer {
        
        private char negation = '¬';
        private char conjunction = '∧';
        private char disjunction = '∨';
        private char implication = '→';
        private char bicondictional = '↔';
        private char universal = '∀';
        private char existential = '∃';
        private char close = ')';
        private char open = '(';

        public Sentence FormularScan(string formula, int level) {
            int openAt = -1;
            int bracketCount = 0;
            char[] formulaArray = formula.ToCharArray();
            char lOperator = ' ';
            int predStart = 0;
            string lastPredicate = "";

            char lastBoundVar = ' ';
            Sentence subOld = null;
            Sentence sub = null;

            bool isPredicate = false;

            for (int i = 0; i < formulaArray.Length; i++) {
                char cur = formulaArray[i];

                //check symbols&operators
                if (bracketCount == 0) {
                    if (cur.Equals(existential) || cur.Equals(universal)) lOperator = cur;
                    if (cur.Equals(negation)) lOperator = cur;
                    if (cur.Equals(conjunction) || cur.Equals(disjunction) || cur.Equals(implication) || cur.Equals(bicondictional)) lOperator = cur;

                    if (char.IsLetter(cur) && !isPredicate && !(i > 0 && (formulaArray[i - 1].Equals(existential) || formulaArray[i - 1].Equals(universal)))) {
                        predStart = i;
                        isPredicate = true;
                    }

                    if (char.IsLower(cur) && (i > 0 && (formulaArray[i - 1].Equals(existential) || formulaArray[i - 1].Equals(universal)))) lastBoundVar = cur;

                }

                if (cur.Equals(close)) {
                    bracketCount--;
                    if (bracketCount == 0) {
                        int from = openAt + 1;
                        int to = i;
                        string subFormula = formula.Substring(from, to - from);
                        subOld = sub;

                        //get new sub
                        if (isPredicate) { //ATOMs
                            lastPredicate = formula.Substring(predStart, openAt - predStart);

                            Term[] t = TermScan(subFormula);

                            //PredicateSymbol ps = this.GetPredicate(lastPredicate);
                            //if (ps == null) throw new System.Exception("ps is null! " + lastPredicate);
                            PredicateSymbol ps = new PredicateSymbol(lastPredicate, t.Length);

                            sub = new AtomicSentence(ps, t);
                            isPredicate = false;
                        } else {
                            sub = FormularScan(subFormula, level + 1);
                        }

                        if (lOperator.Equals(existential)) sub = new ComplexSentence(sub, OperatorType.existential, lastBoundVar.ToString());
                        if (lOperator.Equals(universal)) sub = new ComplexSentence(sub, OperatorType.universal, lastBoundVar.ToString());
                        if (lOperator.Equals(negation)) sub = new ComplexSentence(sub, OperatorType.negation);

                        if (subOld == null) continue;

                        if (lOperator.Equals(conjunction)) sub = new ComplexSentence(subOld, sub, OperatorType.conjunction);
                        if (lOperator.Equals(disjunction)) sub = new ComplexSentence(subOld, sub, OperatorType.disjunction);
                        if (lOperator.Equals(implication)) sub = new ComplexSentence(subOld, sub, OperatorType.implication);
                        if (lOperator.Equals(bicondictional)) sub = new ComplexSentence(subOld, sub, OperatorType.biconditional);

                    }
                }

                if (cur.Equals(open)) {
                    if (bracketCount == 0) {
                        openAt = i;
                    }
                    bracketCount++;
                }
            }

            return sub;
        }


        private Term[] TermScan(string terms) {
            int openAt = 0;
            char[] formulaArray = terms.ToCharArray();
            List<Term> t = new List<Term>();

            int bracketCount = 0;
            for (int i = 0; i < formulaArray.Length; i++) {
                char cur = formulaArray[i];

                if (cur.Equals(close)) bracketCount--;
                if (cur.Equals(open)) bracketCount++;

                if (bracketCount == 0 && (cur.Equals(',') || (i == formulaArray.Length - 1))) {
                    int from = openAt;
                    int to = i;
                    if ((i == formulaArray.Length - 1)) to = formulaArray.Length;
                    string term = terms.Substring(from, to - from);
                    Term newTerm = ParseTerm(term);
                    t.Add(newTerm);
                    openAt = i + 1;
                }

            }

            return t.ToArray();
        }
        private Term ParseTerm(string term) {
            if (term.Contains(open + "") || term.Contains(close + "")) {
                int openAt = 0;
                int bracketCount = 0;
                char[] funcArray = term.ToCharArray();
                string functionSymbol = "";
                Term[] inside = null;
                for (int j = 0; j < funcArray.Length; j++) {
                    char cur2 = funcArray[j];

                    if (cur2.Equals(close)) {
                        bracketCount--;
                        if (bracketCount == 0) {
                            int from = openAt + 1;
                            int to = j;
                            string sub = term.Substring(from, to - from);
                            inside = TermScan(sub);
                        }

                    }
                    if (cur2.Equals(open)) {
                        if (bracketCount == 0) {
                            functionSymbol = term.Substring(0, j);
                            openAt = j;
                        }
                        bracketCount++;
                    }
                }


                //FunctionSymbol fs = this.GetFunctionSymbol(functionSymbol);
                //if (fs == null) throw new System.Exception("fs is null! " + functionSymbol);

                FunctionSymbol fs = new FunctionSymbol(functionSymbol, inside.Length);
                return new FunctionTerm(fs, inside);
            } else {
                //FunctionSymbol fs = this.GetFunctionSymbol(term); //constant
                //if (fs != null) return new Function(fs, term);
                return new VariableTerm(term); // or constant
            }

        }

        public bool BracketCheck(string input) {
            char[] array = input.ToCharArray();
            int bracketCount = 0;
            for (int i = 0; i < array.Length; i++) {
                char cur = array[i];
                if (cur.Equals(close)) bracketCount--;
                if (cur.Equals(open)) bracketCount++;
            }
            return bracketCount == 0;
        }





    }

}
