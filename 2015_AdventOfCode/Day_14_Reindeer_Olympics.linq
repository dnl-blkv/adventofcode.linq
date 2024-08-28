<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyList<Reindeer> input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int RaceDuration = 2503;

private static int Solve1(IReadOnlyList<Reindeer> input) =>
    input.Max(rd => rd.CalculateTravelDistance(RaceDuration));

private static int Solve2(IReadOnlyList<Reindeer> input)
{
    int[] results = Enumerable.Range(0, input.Count).Select(_ => 0).ToArray();

    for (int i = 1; i <= RaceDuration; i++)
    {
        int[] positions = input.Select(rd => rd.CalculateTravelDistance(i)).ToArray();
        int maxDistance = positions.Max();
        
        for (int j = 0; j < positions.Length; j++)
        {
            if (positions[j] != maxDistance)
            {
                continue;   
            }
            
            results[j]++;
        }
    }
    
    return results.Max();
}

private static IReadOnlyList<Reindeer> ParseInput(IEnumerable<string> input) =>
    input.Select(Reindeer.Parse).ToArray();

private class Reindeer
{
    private readonly int speed;
    private readonly int flyTime;
    private readonly int restTime;
    
    public Reindeer(int speed, int flyTime, int restTime)
    {
        this.speed = speed;
        this.flyTime = flyTime;
        this.restTime = restTime;
    }

    public static Reindeer Parse(string reindeerString)
    {
        string[] reindeerStringParts = reindeerString.Split(' ');
        
        return new Reindeer(
            speed: int.Parse(reindeerStringParts[3]),
            flyTime: int.Parse(reindeerStringParts[6]),
            restTime: int.Parse(reindeerStringParts[13]));
    }
    
    public int CalculateTravelDistance(int totalTime)
    {
        int cycleTime = this.flyTime + this.restTime;
        int wholeCycles = totalTime / cycleTime;
        int remainingCycleTime = totalTime % cycleTime;
        
        return (wholeCycles * this.flyTime + Math.Min(this.flyTime, remainingCycleTime)) * this.speed;
    }
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
