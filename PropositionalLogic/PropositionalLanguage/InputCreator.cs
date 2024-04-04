namespace PropositionalLogic;

public class InputCreator
{
    public static List<string> GeneratePropositionalSentences(int n, LogicalConstant.LSymbol truthValue = LogicalConstant.LSymbol.TRUE) {
        var atomicSentences = GenerateAtomicSentences(n).ToList();
        atomicSentences[0] = truthValue.ToString();
        var connectives = GenerateConnectives(n - 1).ToList();

        var list = new List<string>();
        foreach (var c in connectives)
        {
            var connectivesRow = c.ToList();

            string newRow = "(";
            for (int i = 0; i < n-1; i++) newRow += "(";
            newRow += atomicSentences[0] + " ";
            for (int i = 1; i < n; i++) newRow += $"{connectivesRow[i-1]} {atomicSentences[i]}) ";
            newRow += ")";
            list.Add(newRow);
        }

        return list;
    }
    
    public static IEnumerable<string> GenerateAtomicSentences(int n) {
        return Enumerable.Range(0, n).Select(i => ((char)('a' + i)).ToString());
    }
    
    private static IEnumerable<IEnumerable<string>> GenerateConnectives(int n) {
        switch (n) {
            case 0:
                return Enumerable.Empty<IEnumerable<string>>();
            case 1:
                return new List<List<string>> { new() { "AND" }, new() { "OR" } };
            default: {
                var subTables = GenerateConnectives(n - 1);
                return subTables.SelectMany(row => new[] { row.Append("AND"), row.Append("OR") });
            }
        }
    }
}