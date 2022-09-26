using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public abstract class Symbol {
        private string name;
        private int arity;
        public Symbol(string name, int arity) {
            this.name = name;
            this.arity = arity;
        }

        public string GetName() => this.name;
        public override string ToString() => GetName();
        
        public int GetArity() => this.arity;
        public override bool Equals(object obj) => name.Equals(((Symbol)obj).GetName());
        public override int GetHashCode() => name.GetHashCode();

    }

    public class VariableSymbol : Symbol {
        public VariableSymbol(string name) : base(name, 0) {
        }
    }

    public class FunctionSymbol : Symbol {
        public FunctionSymbol(string name, int arity) : base(name, arity) {

        }
        public bool IsConstant() {
            if (GetArity() == 0) return true;
            return false;
        }
    }

    public class PredicateSymbol : Symbol {
        public PredicateSymbol(string name, int arity) : base(name, arity) {

        }
    }
}