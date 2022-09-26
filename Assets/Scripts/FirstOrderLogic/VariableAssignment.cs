using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstOrderLogic {

    public class VariableAssignment {
        private Dictionary<VariableSymbol, int> assignment;

        public VariableAssignment() {
            this.assignment = new Dictionary<VariableSymbol, int>();
        }
        public VariableAssignment(Dictionary<VariableSymbol, int> belegung) {
            this.assignment = belegung;
        }
        public Dictionary<VariableSymbol, int> GetAssignment() {
            return this.assignment;
        }
        public  int GetAssignmentFor(VariableSymbol var) {
            if (!assignment.ContainsKey(var)) {
                //Debug.LogError(var.GetName() + " does not exist");
                return -1;
            }
            return assignment[var];
        }
        public void AddAssignment(VariableSymbol v, int element) {
            if (assignment.ContainsKey(v)) {
                return;
            }
            this.assignment.Add(v, element);
        }

        public void AddAssignment(string vs, Universe.Element element) {
            VariableSymbol v = new VariableSymbol(vs);
            if (assignment.ContainsKey(v)) {
                return;
            }
            this.assignment.Add(v, element.GetHashCode());
        }
        public void RemoveAssignment(VariableSymbol v) {
            if (assignment.ContainsKey(v)) {
                return;
            }
            this.assignment.Remove(v);
        }
        public override string ToString() {
            string s = "Assignment:\n";
            foreach (VariableSymbol vs in assignment.Keys) {
                s += vs.GetName() + " -> " + assignment[vs] + "\n";
            }
            return s;
        }
    }
}