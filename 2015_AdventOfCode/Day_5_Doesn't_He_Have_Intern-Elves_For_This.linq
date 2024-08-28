<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<string> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static readonly HashSet<char> Vowels = new HashSet<char>("aeiou".ToArray());

private static readonly HashSet<string> NaughtySubstrings1 = new HashSet<string>
{
    "ab",
    "cd",
    "pq",
    "xy"
};

private static int Solve1(IEnumerable<string> input) =>
    input.Count(s =>
    {
        int vowelCount = 0;
        bool hasDoubleChar = false;
        var window = new List<char>();
        
        foreach (char c in s)
        {
            if (Vowels.Contains(c))
            {
                vowelCount++;
            }
                
            window.Add(c);
            
            if (window.Count < 2)
            {
                continue;
            }
            
            hasDoubleChar |= window[0] == window[1];
            
            if (NaughtySubstrings1.Contains(new string(window.ToArray())))
            {
                return false;
            }

            window.RemoveAt(0);
        }
        
        return vowelCount >= 3 && hasDoubleChar;
    });

private static int Solve2(IEnumerable<string> input) =>
    input.Count(s =>
    {
        var window2 = new List<char>();
        var pairStartPositions = new Dictionary<string, int>();
        
        bool hasNonOverlappingDoublePair = false;
        var window3 = new List<char>();
        bool hasSymmetricTrio = false;
        
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
       
            window2.Add(c);
            
            if (window2.Count < 2)
            {
                continue;
            }
            
            string currentPair = new string(window2.ToArray());
            int currentPairStartPosition = i - 1;
            
            if (!pairStartPositions.ContainsKey(currentPair))
            {
                pairStartPositions[currentPair] = currentPairStartPosition;
            }
            
            hasNonOverlappingDoublePair |= pairStartPositions[currentPair] < currentPairStartPosition - 1;

            window2.RemoveAt(0);
            
            window3.Add(c);
            
            if (window3.Count < 3)
            {
                continue;
            }
            
            hasSymmetricTrio |= window3[0] == window3[2];

            window3.RemoveAt(0);
        }
        
        return hasNonOverlappingDoublePair && hasSymmetricTrio;
    });

private static IReadOnlyList<string> ParseInput(IEnumerable<string> input) => input.ToArray();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
