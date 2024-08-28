<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    EncryptedName[] input = ParseInput(GetInput());
    Solve1(input).Dump();
    Solve2(input).Dump();
}

private static long Solve1(EncryptedName[] input) =>
    input.Sum(en => GetTopChars(en.EncryptedParts.SelectMany(p => p), n: 5) == en.CheckSum ? en.SectorId : 0);

private static long Solve2(EncryptedName[] input) =>
    input.Where(en => en.Decrypt().Contains("north")).Single().SectorId;
    
private static string GetTopChars(IEnumerable<char> chars, int n) =>
    string.Join(
        string.Empty,
        chars.GroupBy(c => c).OrderBy(g => (-g.Count(), g.Key)).Take(n).Select(g => g.Key));

private static EncryptedName[] ParseInput(IEnumerable<string> input) =>
    input.Select(EncryptedName.Parse).ToArray();

private readonly record struct EncryptedName(string[] EncryptedParts, int SectorId, string CheckSum)
{
    public static EncryptedName Parse(string nameString)
    {
        string[] nameStringParts = nameString.Split('-');
        string[] sectorIdAndCheckSum = nameStringParts[^1].Trim(']').Split('[');
        
        return new EncryptedName(
            EncryptedParts: nameStringParts[..^1],
            SectorId: int.Parse(sectorIdAndCheckSum[0]),
            CheckSum: sectorIdAndCheckSum[1]);
    }
    
    public string Decrypt() =>
        Decrypt(this.EncryptedParts, this.SectorId);
    
    private static string Decrypt(string[] parts, int sectorId) =>
        string.Join(' ', parts.Select(ep => Decrypt(ep, sectorId)));
    
    private static string Decrypt(string part, int sectorId) =>
        string.Join(string.Empty, part.Select(c => (char)((c - 'a' + sectorId) % 26 + 'a')));
}

private static IEnumerable<string> GetInput()
{
    using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
    while (inputStream.ReadLine() is { } line) yield return line;
}
