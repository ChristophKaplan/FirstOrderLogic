using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace FirstOrderLogic {

    public abstract class Universe {
        public abstract class Element {
            private int id;
            public int GetID() => this.id;

            public Element() {
                this.id = Structure.GetNewId();
            }

            public override string ToString() {
                return "Element_" + GetID();
            }
            public override bool Equals(object obj) {
                return GetID().Equals(((Element)obj).GetID());
            }
            public override int GetHashCode() {
                return GetID();
            }
        }

        public abstract Element GetElementById(int id);
        public abstract void AddElement(Universe.Element element);
        public abstract void AddElements(List<Universe.Element> elements);
        public abstract void RemoveElement(Universe.Element element);
        public abstract List<int> GetAllElementIDs();
    }


    public abstract class ElementArrayHelper {
        protected int[] ElementArrayToIndexArray(Universe.Element[] elements) {
            int[] result = new int[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                result[i] = elements[i].GetHashCode();
            }
            return result;
        }
        protected bool IsArrayEqual(int[] first, int[] second) {
            if (first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++) {
                if (!first[i].Equals(second[i])) {
                    return false;
                }
            }
            return true;
        }
        protected int Hash(int hash1, int hash2) {
            //first value should be 17
            return hash1 * 31 * hash2;
        }
    }


    public abstract class PredicateRelationBase : ElementArrayHelper {
        private int id;
        public int GetID() => this.id;

        public PredicateRelationBase() {
            this.id = Structure.GetNewId();
        }

        public abstract bool Contains(int[] other);

        public override bool Equals(object obj) {
            PredicateRelationBase other = (PredicateRelationBase)obj;
            return this.GetHashCode().Equals(other.GetHashCode());
        }
        public override int GetHashCode() => this.id;


        public abstract string ToString(Universe uni);
    }

    public class PredicateRelation : PredicateRelationBase {
        private List<int[]> elements = new List<int[]>();
        public List<int[]> GetElements() => this.elements;

        public PredicateRelation() {}

        public PredicateRelation(List<Universe.Element[]> args) {
            for (int i = 0; i < args.Count; i++) Add(args[i]);
        }
        public PredicateRelation(List<Universe.Element> args) {
            for (int i = 0; i < args.Count; i++) Add(args[i]);
        }

        public void Add(params Universe.Element[] args) {
            Add(ElementArrayToIndexArray(args));
        }
        public void Add(params int[] args) {
            elements.Add(args);
        }

        public override bool Contains(int[] other) {
            for (int j = 0; j < elements.Count; j++) {
                if (IsArrayEqual(elements[j], other)) return true;
            }
            return false;
        }

        /*
        public override bool Equals(object obj) {
            PredicateRelation other = (PredicateRelation)obj;
            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode() {
            int hash = 17;
            for (int i = 0; i < elements.Count; i++) {
                int[] el = elements[i];
                for (int j = 0; j < el.Length; j++) {
                    int e = el[j];
                    hash = Hash(hash, e.GetHashCode());
                }
            }
            return hash;
        }*/

        public override string ToString() {
            if (elements.Count == 0) return "()";
            string s = "";
            for (int i = 0; i < elements.Count; i++) {
                s += "( ";
                for (int j = 0; j < elements[i].Length; j++) s += elements[i][j].ToString() + ", ";
                s += " ) ";
            }
            return s;
        }

        public override string ToString(Universe uni) {
            if (elements.Count == 0) return "()";

            string s = "";
            if (elements.Count > 10) {
                s += "( ";
                for (int j = 0; j < elements[0].Length; j++) s += uni.GetElementById(elements[0][j]) + ", ";
                s += " ... ";
                for (int j = 0; j < elements[elements.Count - 1].Length; j++) s += uni.GetElementById(elements[elements.Count - 1][j]) + ", ";
                s += " ) ";
                return s;
            }

            for (int i = 0; i < elements.Count; i++) {
                s += "( ";
                for (int j = 0; j < elements[i].Length; j++) s += uni.GetElementById(elements[i][j]) + ", ";
                s += " ) ";
            }
            return s;
        }


    }



    public abstract class FunctionBase : ElementArrayHelper {
        private int id;
        public int GetID() => this.id;

        public FunctionBase() {
            this.id = Structure.GetNewId();
        }

        public abstract int Map(int[] other);
        public abstract bool ContainsDomain(int[] other);

        public override bool Equals(object obj) {
            PredicateRelationBase other = (PredicateRelationBase)obj;
            return this.GetHashCode().Equals(other.GetHashCode());
        }
        public override int GetHashCode() => this.id;

        public abstract string ToString(Universe uni);

    }


    public class Function : FunctionBase {
        //problem with array in dictionary - equality checks
        private Dictionary<int[], int> mapping = new Dictionary<int[], int>();
        public Dictionary<int[], int> GetMapping() => this.mapping;

        public Function() {

        }

        public Function(List<Universe.Element[]> from, Universe.Element to) {
            for (int i = 0; i < from.Count; i++) Add(from[i], to);
        }

        public Function(int[] from, int to) {
            Add(from, to);
        }
        public Function(Universe.Element[] from, Universe.Element to) {
            Add(from, to);
        }

        public void Add(int[] domain, int mapping) {
            this.mapping.Add(domain, mapping);
        }
        public void Add(Universe.Element[] domain, Universe.Element mapping) {
            this.mapping.Add(ElementArrayToIndexArray(domain), mapping.GetHashCode());
        }
        public override int Map(int[] other) {
            foreach (int[] args in mapping.Keys) {
                if (IsArrayEqual(args, other)) return mapping[args];
            }
            return -1;
        }
        public int[] GetDomain(int[] other) {
            foreach (int[] args in mapping.Keys) {
                if (IsArrayEqual(args, other)) return args;
            }
            return null;
        }
        public override bool ContainsDomain(int[] other) {
            if (GetDomain(other) == null) return false;
            return true;
        }

        /*
        public override bool Equals(object obj) {
            Function other = (Function)obj;

            return this.GetHashCode().Equals(other.GetHashCode());

            if (mapping.Keys.Count != other.mapping.Keys.Count) return false;

            for (int i = 0; i < mapping.Keys.Count; i++) {
                KeyValuePair<int[], int> pairThis = GetMapping().ElementAt(i);
                KeyValuePair<int[], int> pairOther = other.GetMapping().ElementAt(i);
                if (!(IsArrayEqual(pairThis.Key, pairOther.Key) && pairThis.Value.Equals(pairOther.Value))) return false;
            }
            return true;
        }

        public override int GetHashCode() {
            int hash = 17;
            foreach (int[] args in mapping.Keys) {
                for (int i = 0; i < args.Length; i++) {
                    int e = args[i];
                    hash = Hash(hash, e.GetHashCode());
                }
                int t = mapping[args];
                hash = Hash(hash, t.GetHashCode());
            }
            return hash;
        }*/

        public override string ToString() {
            string s = "";
            foreach (int[] args in mapping.Keys) {
                s += "(";
                for (int i = 0; i < args.Length; i++) {
                    s += args[i] + ", ";
                }
                s += ") = (" + mapping[args] + ") , ";
            }
            return s;
        }

        public override string ToString(Universe uni) {
            string s = "";
            foreach (int[] args in mapping.Keys) {
                s += "(";
                for (int i = 0; i < args.Length; i++) {
                    s += uni.GetElementById(args[i]) + ", ";
                }
                s += ") = (" + uni.GetElementById(mapping[args]) + ") , ";
            }

            return s;
        }
    }



    public class Structure {
        private static int currentID;
        public static int GetNewId() => currentID++;

        private Universe universe;
        private Dictionary<int, PredicateRelationBase> predicateRelations = new Dictionary<int, PredicateRelationBase>();
        private Dictionary<int, FunctionBase> functions = new Dictionary<int, FunctionBase>();

        public Universe GetUniverse() => this.universe;
        public Dictionary<int, PredicateRelationBase> GetPredicateRelations() => this.predicateRelations;
        public Dictionary<int, FunctionBase> GetFunctions() => this.functions;

        private Dictionary<int, IEnumerable<IEnumerable<int[]>>> powersetPool = new Dictionary<int, IEnumerable<IEnumerable<int[]>>>();

        public Structure() {

        }

        public void AddUniverse(Universe universe) {
            this.universe = universe;
        }

        public int AddFunction(Function function) {
            int id = function.GetHashCode();
            if (!functions.ContainsKey(id)) {
                functions.Add(id, function);
            } else {
                Debug.LogError("DOUBLE");
            }
            return id;
        }

        public int AddPredicateRelation(PredicateRelationBase prb) {
            int id = prb.GetHashCode();
            if (!predicateRelations.ContainsKey(id)) {
                predicateRelations.Add(id, prb);
            }
            return id;
        }



        private List<List<int>> GetSequence(Signature signature) {

            List<Task<List<int>>> tasks = new List<Task<List<int>>>();
            List<List<int>> seq = new List<List<int>>();

            Parallel.ForEach(signature.GetPredicateSymbols(), (PredicateSymbol ps) => {
                List<int> yo = GetPossiblePredicateRelations(ps);
                seq.Add(yo);
            });

            Parallel.ForEach(signature.GetFunctionSymbols(), (FunctionSymbol fs) => {
                List<int> yo = GetPossibleFunctions(fs);
                seq.Add(yo);
            });


            /*

            foreach (PredicateSymbol ps in signature.GetPredicateSymbols()) {
                Task<List<int>> task = Task<List<int>>.Factory.StartNew(() => {
                    return GetPossiblePredicateRelations(ps);
                });
                tasks.Add(task);

            }
            foreach (FunctionSymbol fs in signature.GetFunctionSymbols()) {
                Task<List<int>> task = Task<List<int>>.Factory.StartNew(() => {
                    return GetPossibleFunctions(fs);
                });
                tasks.Add(task);

            }

            Task.WhenAll(tasks);

            for (int i = 0; i < tasks.Count; i++) {
                List<int> p = tasks[i].Result;
                seq.Add(p);
            }
            */
            return seq;
        }

        private List<Interpretation> BuildInterpretations(Signature signature, IEnumerable<IEnumerable<int>> crossed) {
            List<Interpretation> interpretations = new List<Interpretation>();

            foreach (var item in crossed) {
                Interpretation curInt = new Interpretation(this, signature);

                int predIndex = 0;
                int funcIndex = 0;
                int predCount = signature.GetPredicateSymbols().Count;

                foreach (int obj in item) {

                    if (predIndex < predCount) {
                        PredicateSymbol correspond = signature.GetPredicateSymbols()[predIndex];
                        curInt.AddToPredicateMap(correspond.GetName(), obj);
                        predIndex++;
                    } else {
                        FunctionSymbol correspond = signature.GetFunctionSymbols()[funcIndex];
                        curInt.AddToFunctionMap(correspond.GetName(), obj);
                        funcIndex++;
                    }

                }
                interpretations.Add(curInt);
            }
            return interpretations;
        }

        public async void GenerateInt(Signature signature) {
            Debug.Log("Start.");

            List<List<int>> seq = GetSequence(signature);
            Debug.Log("mid");

            IEnumerable<IEnumerable<int>> crossed = CartesianProduct(seq);

            List<Interpretation> interpretations = BuildInterpretations(signature, crossed);

            Debug.Log("Done. " + interpretations.Count);
        }



        private List<int> GetPossibleFunctions(FunctionSymbol fs) {
            Debug.Log("GetPossibleFunctions " + fs);

            IEnumerable<IEnumerable<int[]>> p = GetPowerSetOf(fs.GetArity() + 1);

            List<int> funcs = new List<int>();

            foreach (IEnumerable<int[]> item in p) {
                Function cur = ArrayToFunction(item);
                int index = AddFunction(cur);
                funcs.Add(index);
            }

            return funcs;

        }

        private Function ArrayToFunction(IEnumerable<int[]> patternList) {
            Function cur = new Function();
            foreach (int[] all in patternList) {
                int[] pre = new int[all.Length - 1];
                for (int k = 0; k < all.Length - 1; k++) pre[k] = all[k];
                cur.Add(pre, all[all.Length - 1]);
            }
            return cur;
        }


        private List<int> GetPossiblePredicateRelations(PredicateSymbol ps) {
            IEnumerable<IEnumerable<int[]>> p = GetPowerSetOf(ps.GetArity());

            List<int> relations = new List<int>();

            foreach (IEnumerable<int[]> sets in p) {
                //guess hier dann 1er 2er mengen ...

                PredicateRelation cur = new PredicateRelation();

                foreach (int[] array in sets) {
                    cur.Add(array);
                }
                relations.Add(AddPredicateRelation(cur));
            }
            return relations;
        }


        //transform data
        public IEnumerable<IEnumerable<int[]>> GetPowerSetOf(int arity) {
            if (powersetPool.ContainsKey(arity)) return powersetPool[arity];

            Debug.Log("make " + arity);

            List<int[]> perm = GetPermutation(arity);
            IEnumerable<IEnumerable<int[]>> powerSet = GetPowerSet(perm);

            powersetPool.Add(arity, powerSet);
            return powerSet;
        }
        private List<int[]> GetPermutation(int arity) {
            //gets a set/matrix of permutations of all objects

            List<int> elementIndexe = GetUniverse().GetAllElementIDs();

            IEnumerable<IEnumerable<int>> perm = PermutationOfObjects(elementIndexe, arity);
            List<IEnumerable<int>> permToList = perm.ToList();

            List<int[]> permutationSet = new List<int[]>();
            for (int i = 0; i < permToList.Count; i++) permutationSet.Add(permToList[i].ToArray());

            return permutationSet;
        }

        //combinatorics
        private IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list) {
            return from m in Enumerable.Range(0, 1 << list.Count)
                   select
                       from i in Enumerable.Range(0, list.Count)
                       where (m & (1 << i)) != 0
                       select list[i];
        }
        private IEnumerable<IEnumerable<T>> PermutationOfObjects<T>(IEnumerable<T> objects, int length) {
            if (length == 1)
                return objects.Select(t => new T[] { t });
            return PermutationOfObjects(objects, length - 1).SelectMany(t => objects, (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences) {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            IEnumerable<IEnumerable<T>> result = emptyProduct;
            foreach (IEnumerable<T> sequence in sequences) {
                result = from accseq in result from item in sequence select accseq.Concat(new[] { item });
            }
            return result;
        }


    }

}