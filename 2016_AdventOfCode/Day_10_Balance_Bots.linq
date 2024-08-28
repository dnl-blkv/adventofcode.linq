<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (
        IReadOnlyDictionary<int, HashSet<int>> setup,
        IReadOnlyDictionary<int, (bool IsLowOut, int Low, bool IsHighOut, int High)> instructions) =
            ParseInput(GetInput());
    Solve(setup, instructions).Dump();
}

private static (long Result1, long Result2) Solve(
    IReadOnlyDictionary<int, HashSet<int>> setup,
    IReadOnlyDictionary<int, (bool IsLowOut, int Low, bool IsHighOut, int High)> instructions)
{
    var bots = setup.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));
    
    int startBot = bots.First(kv => kv.Value.Count == 2).Key;
    var botQueue = new Queue<int>();
    botQueue.Enqueue(startBot);
    HashSet<int> botQueueSet = [startBot];
    
    var output = new Dictionary<int, HashSet<int>>();
    long result1 = -1;
    
    while (botQueue.Count > 0)
    {
        int currentBot = botQueue.Dequeue();
        botQueueSet.Remove(currentBot);
        (bool isLowOut, int low, bool isHighOut, int high) = instructions[currentBot];
        (int lowValue, int highValue) = (bots[currentBot].Min(), bots[currentBot].Max());
        
        if (result1 < 0 && lowValue == 17 && highValue == 61)
        {
            result1 = currentBot;
        }
        
        ProcessVal(currentBot, isLowOut, low, lowValue);
        ProcessVal(currentBot, isHighOut, high, highValue);
    }
    
    return (
        Result1: result1,
        Result2: output[0].First() * output[1].First() * output[2].First());
    
    void ProcessVal(int source, bool isOut, int target, int val)
    {
        bots[source].Remove(val);
        StoreVal(store: (isOut ? output : bots), target: target, val: val);
        
        if (!bots.ContainsKey(target) || bots[target].Count < 2 || botQueueSet.Contains(target))
        {
            return;
        }
        
        botQueue.Enqueue(target);
        botQueueSet.Add(target);
    }
    
    static void StoreVal(Dictionary<int, HashSet<int>> store, int target, int val)
    {
        if (!store.ContainsKey(target))
        {
            store[target] = new HashSet<int>();
        }
        
        store[target].Add(val);
    }
}

private static (
    IReadOnlyDictionary<int, HashSet<int>> Setup,
    IReadOnlyDictionary<int, (bool IsLowOut, int Low, bool IsHighOut, int High)> Instructions) ParseInput(
        IEnumerable<string> input)
{
    var setup = new Dictionary<int, HashSet<int>>();
    var instructions = new Dictionary<int, (bool IsLowOut, int Low, bool IsHighOut, int High)>();

    foreach (string line in input)
    {
        string[] lineParts = line.Split(' ');
        string linePrefix = lineParts[0];
        
        switch (linePrefix)
        {
            case "value":
                int value = int.Parse(lineParts[1]);
                int target = int.Parse(lineParts[5]);
                
                if (!setup.ContainsKey(target))
                {
                    setup[target] = new HashSet<int>();
                }
                
                setup[target].Add(value);
                break;
                
            case "bot":
                instructions[int.Parse(lineParts[1])] = (
                    IsLowOut: IsOut(lineParts[5]),
                    Low: int.Parse(lineParts[6]),
                    IsHighOut: IsOut(lineParts[10]),
                    High: int.Parse(lineParts[11]));
                break;
                
            default:
                throw new ArgumentException($"Unexpected line prefix: '{linePrefix}'");
        }
    }
    
    return (setup, instructions);
    
    static bool IsOut(string targetType) => targetType == "output";
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
