<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    string input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int PasswordLength = 8;
private const string InterestingPrefix = "00000";

// This one runs for about 30 sec on my machine, do brace yourself
private static (string Result1, string Result2) Solve(string input)
{
    var password1Chars = new char[PasswordLength];
    var password2Chars = new char[PasswordLength];

    int pos1 = 0;
    int mask2 = 0;
    int i = 0; 
    
    using MD5 md5 = MD5.Create();
    
    while (pos1 < PasswordLength || mask2 < 255)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes($"{input}{i}");
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        string hashHex = Convert.ToHexString(hashBytes);
        
        if (hashHex.StartsWith(InterestingPrefix))
        {
            if (pos1 < PasswordLength)
            {
                password1Chars[pos1] = hashHex[5];
                pos1++;
            }
            
            int pos2 = hashHex[5] - '0';
            int curMask = 1 << pos2;
            
            if (pos2 < PasswordLength && (mask2 & curMask) == 0)
            {
                password2Chars[pos2] = hashHex[6];
                mask2 |= curMask;
            }
        }
        
        i++;
    }
    
    return (
        Result1: string.Join(string.Empty, password1Chars),
        Result2: string.Join(string.Empty, password2Chars));
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}