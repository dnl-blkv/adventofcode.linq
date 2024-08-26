<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
	Solve1(input).Dump();
	Solve2(input).Dump();
}

private const int LookAheadDistance = 1000;
private const int Rep = 3;
private const int RepAhead = 5;
private const int StretchBy2 = 2016;

private static long Solve1(string input) => Solve(CreateHashProvider1(input));

private static long Solve2(string input) => Solve(CreateHashProvider2(input));

private static long Solve(Func<MD5, int, string> hashProvider)
{   
    var hashQueue = new Queue<string>();
    var totalFiveQueue = new Queue<(int I, HashSet<char> Chars)>();
    var charFiveQueues = new Dictionary<char, Queue<int>>();
    
    using MD5 md5 = MD5.Create();
    
    int i, c;
    
    for (i = 0, c = 0; c < 64; i++)
    {
        string lookAheadHash = hashProvider.Invoke(md5, i);
        Enqueue(lookAheadHash, i);
        
        if (i < LookAheadDistance)
        {
            continue;
        }
        
        string currentHash = Dequeue(i - LookAheadDistance);
        char threeC = GetRepeated(currentHash, Rep).FirstOrDefault();
        
        if (threeC == default || !TryGetCharFiveQueue(threeC, out _))
        {
            continue;
        }
        
        c++;
    }
    
    return i - LookAheadDistance - 1;
    
    void Enqueue(string val, int index)
    {
        hashQueue.Enqueue(val);
    
        HashSet<char> chars = [];
        
        foreach (char c in GetRepeated(val, RepAhead))
        {
            chars.Add(c);
        
            if (!charFiveQueues.ContainsKey(c))
            {
                charFiveQueues[c] = new Queue<int>();
            }
        
            charFiveQueues[c].Enqueue(c);
        }
        
        if (chars.Count == 0)
        {
            return;
        }
        
        totalFiveQueue.Enqueue((I: index, Chars: chars));
    }
    
    string Dequeue(int index)
    {
        string value = hashQueue.Dequeue();
    
        if (totalFiveQueue.Count == 0)
        {
            return value;
        }
    
        (int firstI, _) = totalFiveQueue.Peek();
        
        if (firstI > index)
        {
            return value; 
        }
        
        (_, HashSet<char> charsToRemove) = totalFiveQueue.Dequeue();
        
        foreach (char c in charsToRemove)
        {
            if (!TryGetCharFiveQueue(c, out Queue<int>? charFiveQueue))
            {
                continue;
            }
            
            charFiveQueue.Dequeue();
        }
        
        return value;
    }
    
    bool TryGetCharFiveQueue(char c, [MaybeNullWhen(returnValue: false)]out Queue<int> result) =>
        charFiveQueues.TryGetValue(c, out result) && result.Count > 0;
}

private static Func<MD5, int, string> CreateHashProvider1(string input) =>
    (md5, i) => CreateHash(md5, input, i);

private static Func<MD5, int, string> CreateHashProvider2(string input) =>
    (md5, i) =>
    {
        string result = CreateHash(md5, input, i);
        
        for (int y = 0; y < StretchBy2; y++)
        {
            result = CreateHash(md5, result);
        }
        
        return result;
    };
    
private static string CreateHash(MD5 md5, string input, int i) =>
    CreateHash(md5, $"{input}{i}");
    
private static string CreateHash(MD5 md5, string input)
{
    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
    byte[] hashBytes = md5.ComputeHash(inputBytes);
    
    return Convert.ToHexString(hashBytes).ToLower();
}

private static IEnumerable<char> GetRepeated(string input, int minRep)
{
    for (int i = 0; i < input.Length; i++)
    {
        int j = i;
        
        while (++j < input.Length && input[j] == input[i]);
        
        if (j - i < minRep)
        {
            continue;
        }
        
        yield return input[i = j - 1];
    }
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}