<Query Kind="Program">
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

const string Problem1PrefixRequired = "00000";
const string Problem2PrefixRequired = "000000";

private static int Solve1(string input) => Solve(input, Problem1PrefixRequired);

private static int Solve2(string input) => Solve(input, Problem2PrefixRequired);

private static int Solve(string input, string prefixRequired)
{
    using MD5 md5 = MD5.Create();
    
    for (int i = 0; i < int.MaxValue; i++)
    {
        string candidate = $"{input}{i}";
        byte[] candidateBytes = Encoding.ASCII.GetBytes(candidate);
        byte[] candidateHashBytes = md5.ComputeHash(candidateBytes);
        string candidateHashHex = Convert.ToHexString(candidateHashBytes);
        
        if (candidateHashHex.StartsWith(prefixRequired))
        {
            return i;
        }
    }
    
    return -1;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
