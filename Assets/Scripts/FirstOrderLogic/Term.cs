using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public abstract class Term {
        public abstract Symbol GetSymbol();
        public abstract Term GetCopy();
        public abstract override string ToString();
        public abstract bool IsVariableInTerm(VariableSymbol var);
        public abstract bool HasVariableIntersection(Term t);
        public abstract void RenameVariable(VariableSymbol from, VariableSymbol to);

        public override bool Equals(object obj) {
            Term other = (Term)obj;
            if (this.ToString().Equals(other.ToString())) return true;
            return false;
        }
        public override int GetHashCode() => ToString().GetHashCode();

    }

    public class VariableTerm : Term {
        private VariableSymbol symbol;
        public VariableTerm(VariableSymbol var) {
            this.symbol = var;
        }
        public VariableTerm(string varAsString) {
            this.symbol = new VariableSymbol(varAsString);
        }

        public override Symbol GetSymbol() => this.symbol;
        public override string ToString() => this.GetSymbol().GetName();
        public override bool IsVariableInTerm(VariableSymbol var) => this.symbol.Equals(var);

        public override bool HasVariableIntersection(Term t) {
            if (t is VariableTerm) return IsVariableInTerm((VariableSymbol)((VariableTerm)t).GetSymbol());
            if (t is FunctionTerm) {
                FunctionTerm f = (FunctionTerm)t;
                for (int i = 0; i < f.GetArguments().Length; i++)
                    if (HasVariableIntersection(f.GetArguments()[i])) return true;
            }
            return false;
        }
        public override void RenameVariable(VariableSymbol from, VariableSymbol to) {
            if (this.symbol.Equals(from)) this.symbol = to;
        }

        public override Term GetCopy() {
            return new VariableTerm(symbol.GetName());
        }
    }

    public class FunctionTerm : Term {
        private FunctionSymbol functionssymbol;
        private Term[] arguments;

        public FunctionTerm(FunctionSymbol func, params Term[] arguments) {
            this.functionssymbol = func;
            this.arguments = arguments;
        }
        public FunctionTerm(FunctionSymbol func, params Symbol[] arguments) {
            this.functionssymbol = func;
            this.arguments = new Term[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                this.arguments[i] = new VariableTerm((VariableSymbol)arguments[i]);
            }
        }
        public FunctionTerm(FunctionSymbol func, params string[] arguments) {
            this.functionssymbol = func;
            this.arguments = new Term[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                this.arguments[i] = new VariableTerm(arguments[i]);
            }
        }

        public override Symbol GetSymbol() => this.functionssymbol;
        public Term[] GetArguments() => this.arguments;

        public override bool IsVariableInTerm(VariableSymbol var) {
            for (int i = 0; i < arguments.Length; i++) if (arguments[i].IsVariableInTerm(var)) return true;
            return false;
        }
        public override bool HasVariableIntersection(Term t) {
            for (int i = 0; i < arguments.Length; i++) if (arguments[i].HasVariableIntersection(t)) return true;
            return false;
        }
        public override void RenameVariable(VariableSymbol from, VariableSymbol to) {
            for (int i = 0; i < arguments.Length; i++) arguments[i].RenameVariable(from, to);
        }
        public override string ToString() {
            if (this.arguments == null) return this.GetSymbol().GetName();
            string s = "";
            s += this.GetSymbol().GetName() + "(";
            for (int i = 0; i < arguments.Length; i++) {
                if (i > 0) s += ", ";
                s += arguments[i].ToString();
            }
            s += ")";
            return s;
        }
        public override Term GetCopy() {
            Term[] newArgs = new Term[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                newArgs[i] = arguments[i].GetCopy();
            }
            return new FunctionTerm(this.functionssymbol, newArgs);
        }
    }


}