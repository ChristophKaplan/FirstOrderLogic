namespace PropositionalLogic;

public static class SentenceExtensions {

    public static void FindReplaceAtom(this Sentence sentence, AtomicSentence replaceMe, string replaceWith) {
        foreach (var child in sentence.Children) {
            child.FindReplaceAtom(replaceMe, replaceWith);
        }

        if (sentence.Equals(replaceMe) && sentence is AtomicSentence atomicSentence) {
            atomicSentence.Symbol = replaceWith;
        }
    }

    public static List<AtomicSentence> GetAtoms(this Sentence sentence) {
        if (sentence is AtomicSentence atomicSentence) {
            return new List<AtomicSentence> { atomicSentence };
        }

        var atoms = new List<AtomicSentence>();
        foreach (var child in sentence.Children) {
            atoms.AddRange(child.GetAtoms());
        }

        return atoms;
    }

    public static List<ComplexSentence> GetComplexChildren(this Sentence sentence) {
        var complexSentences = new List<ComplexSentence>();
        
        foreach (var child in sentence.Children)
        {
            if (child is not ComplexSentence childComplexSentence) continue;
            complexSentences.Add(childComplexSentence);
            complexSentences.AddRange(child.GetComplexChildren());
        }
        
        return complexSentences;
    }
    
    public static Sentence GetCopy(this Sentence sentence) {
        switch (sentence) {
            case AtomicSentence atomicSentence:
                return new AtomicSentence(atomicSentence.Symbol);
            case ComplexSentence complexSentence: {
                var result = new ComplexSentence(complexSentence.Operator);
                foreach (var child in complexSentence.Children) {
                    result.AddChild(child.GetCopy());
                }

                return result;
            }
            default:
                throw new Exception("Sentence type not found!");
        }
    }
    
    public static bool IsAtomComplexRelation(this Sentence sentence, out AtomicSentence atomicSentence, out ComplexSentence complex) {
        atomicSentence = null;
        complex = null;
            
        if (sentence is AtomicSentence) return false;

        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if((lhs is AtomicSentence && rhs is AtomicSentence) || (lhs is ComplexSentence && rhs is ComplexSentence)) return false;

        if (lhs is AtomicSentence lhs1 && rhs is ComplexSentence rhs1) {
            atomicSentence = lhs1;
            complex = rhs1;
        }
        else if (rhs is AtomicSentence rhs2 && lhs is ComplexSentence lhs2) {
            atomicSentence = rhs2;
            complex = lhs2;
        }

        return true;
    }
}