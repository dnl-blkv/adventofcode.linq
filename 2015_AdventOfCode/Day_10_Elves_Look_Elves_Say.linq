<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private const int IterationCount1 = 40;
private const int IterationCount2 = 50;

private static int Solve1(string input) => Solve(input, IterationCount1);

private static int Solve2(string input) => Solve(input, IterationCount2);

private static int Solve(string input, int iterationCount)
{
    char[] currentState = input.ToArray();
    
    for (int i = 0; i < iterationCount; i++)
    {
        currentState = CreateLookAndSaySequence(currentState, dumbDown: i > 1);
    }
    
    return currentState.Length;
}

/// <summary>
/// Turns a list into a "Look and say" sequence over it.
/// <see href="https://en.wikipedia.org/wiki/Look-and-say_sequence" />
/// </summary>
private static char[] CreateLookAndSaySequence(char[] input, bool dumbDown = true)
{
    var result = new List<char>();
    int currentSequenceStartIndex = 0;
    
    while (currentSequenceStartIndex < input.Length)
    {
        int currentIndex = currentSequenceStartIndex;
        char currentSequenceLength = '0';
        
        while (currentIndex < input.Length && input[currentIndex] == input[currentSequenceStartIndex])
        {
            currentSequenceLength += (char)1;
            currentIndex++;
        }
        
        char valueChar =
            dumbDown && input[currentSequenceStartIndex] > '3'
                ? 'x'
                : input[currentSequenceStartIndex];
        result.Add(currentSequenceLength);
        result.Add(valueChar);
        currentSequenceStartIndex = currentIndex;
    }
    
    return result.ToArray();
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
