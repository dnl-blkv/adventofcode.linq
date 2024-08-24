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
   
private static char[] Alphabet =
    Enumerable.Range('a', 'z' + 1).Select(i => (char)i).ToArray();
    
private static string Solve1(string input) => GetNextValidPassword(input);

private static string Solve2(string input) => GetNextValidPassword(GetNextValidPassword(input));

private static string GetNextValidPassword(string input)
{
    string currentPassword = input;

    do
    {
        currentPassword = Increment(currentPassword);
    } while (!IsValid(currentPassword));
    
    return currentPassword;
}

private static string Increment(string password)
{
    char[] passwordChars = password.ToArray();

    for (int i = password.Length - 1; i >= 0; i--)
    {
        if (passwordChars[i] < 'z')
        {
            do 
            {
                passwordChars[i] += (char)1;
            } while (passwordChars[i] is 'i' or 'l' or 'o');
            
            break;
        }
        
        passwordChars[i] = 'a';
    }
    
    return string.Join(string.Empty, passwordChars);
}

private static bool IsValid(string password)
{
    bool hasTriplet = false;

    for (int i = 0; i < password.Length - 2; i++)
    {
        if (password[i + 1] - password[i] != 1 || password[i + 2] - password[i + 1] != 1)
        {
            continue;
        }
        
        hasTriplet = true;
        break;
    }
    
    int j = 0;
    int doubleCount = 0;
    
    while (j < password.Length - 1)
    {
        if (password[j] == password[j + 1])
        {
            doubleCount++;
            j += 1;
        }
        
        j += 1;
    }
    
    return hasTriplet && doubleCount >= 2;
}

private static string ParseInput(IEnumerable<string> input) => input.Single();

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}