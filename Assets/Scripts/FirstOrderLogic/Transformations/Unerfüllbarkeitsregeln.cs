using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstOrderLogic {

    public class Unerfuellbarkeitsregeln : TransformationRule {
        public override Sentence GetEquivalent(Sentence f) {
            ComplexSentence v = f.AsComplex();
            return v;
        }
        public override bool IsPossible(Sentence f) {
            return false;
        }
    }

}