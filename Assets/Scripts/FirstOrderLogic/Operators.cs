using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//¬∧∨→↔∀∃

namespace FirstOrderLogic {

    public enum OperatorType { affirmation, negation, conjunction, disjunction, implication, biconditional, universal, existential }

    public abstract class Operator {
        private OperatorType operatorType;

        public abstract override string ToString();

        public bool IsConnective() => this is Connective;
        public bool IsQuantifier() => this is Quantifier;

        public Connective AsConnective() => (Connective)this;
        public Quantifier AsQuantifier() => (Quantifier)this;

        public OperatorType GetOperatorType() => this.operatorType;

        public void SetOperatorType(OperatorType operatorType) => this.operatorType = operatorType;

        public bool IsAffirmation() => this.operatorType == OperatorType.affirmation;
        public bool IsNegation() => this.operatorType == OperatorType.negation;
        public bool IsConjunction() => this.operatorType == OperatorType.conjunction;
        public bool IsDisjunction() => this.operatorType == OperatorType.disjunction;
        public bool IsImplication() => this.operatorType == OperatorType.implication;
        public bool IsBiconditional() => this.operatorType == OperatorType.biconditional;

        public bool IsUniversal() => this.operatorType == OperatorType.universal;
        public bool IsExistential() => this.operatorType == OperatorType.existential;
    }

    public class Connective : Operator, IEqualityComparer<Connective> {

        public Connective(OperatorType operatorType) {
            SetOperatorType(operatorType);
        }

        public bool IsOpposite(Connective opposite) {
            if (IsImplication() || opposite.IsImplication() || IsBiconditional() || opposite.IsBiconditional()) {
                //Debug.Log("Cant be oposite");
                return false;
            }
            if (IsAffirmation() && opposite.IsNegation()) return true;
            if (IsNegation() && opposite.IsAffirmation()) return true;
            if (IsConjunction() && opposite.IsDisjunction()) return true;
            if (IsDisjunction() && opposite.IsConjunction()) return true;
            return false;
        }

        public override string ToString() {
            if (IsAffirmation()) return "+";
            if (IsNegation()) return "¬";
            if (IsConjunction()) return "∧";
            if (IsDisjunction()) return "∨";
            if (IsImplication()) return "→";
            if (IsBiconditional()) return "↔";
            return "error";
        }

        public override bool Equals(object obj) => ((Connective)obj).GetOperatorType().Equals(this.GetOperatorType());
        public override int GetHashCode() => this.GetOperatorType().GetHashCode();
        public bool Equals(Connective x, Connective y) => x.Equals(y);
        public int GetHashCode(Connective obj) => GetHashCode();
    }


    public class Quantifier : Operator, IEqualityComparer<Quantifier> {
        private VariableSymbol v;

        public Quantifier(OperatorType operatorType, VariableSymbol v) {
            SetOperatorType(operatorType);
            this.v = v;
        }
        public Quantifier(OperatorType operatorType, string v) {
            SetOperatorType(operatorType);
            this.v = new VariableSymbol(v);
        }

        public VariableSymbol GetVariable() {
            return this.v;
        }
        public void SetVariable(VariableSymbol s) {
            this.v = s;
        }

        public override string ToString() {
            if (IsUniversal()) return "∀" + v.GetName();
            if (IsExistential()) return "∃" + v.GetName();
            return "error";
        }


        public Connective GetAsConnective() {
            if (IsUniversal()) return new Connective(OperatorType.conjunction);
            if (IsExistential()) return new Connective(OperatorType.disjunction);
            return null;
        }

        public Quantifier GetAsOpposite() {
            if (IsUniversal()) return new Quantifier(OperatorType.existential, this.v);
            if (IsExistential()) return new Quantifier(OperatorType.universal, this.v);
            throw new Exception("quantor type not found");
        }

        public override bool Equals(object obj) {
            Quantifier other = (Quantifier)obj;
            if (this.ToString().Equals(other.ToString())) return true;
            return false;
        }

        public override int GetHashCode() => this.ToString().GetHashCode();
        public bool Equals(Quantifier x, Quantifier y) => x.Equals(y);
        public int GetHashCode(Quantifier obj) => GetHashCode();
    }


}