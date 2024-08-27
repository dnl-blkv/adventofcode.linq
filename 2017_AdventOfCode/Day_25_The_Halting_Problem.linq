<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    Given input = ParseInput(GetInput());
	Solve(input).Dump();
}

private delegate StateBranch ProcessState(bool currentValue);

private static long Solve(Given input)
{
    (
        char initialState,
        int stepCount,
        IReadOnlyDictionary<char, ProcessState> states) = input;

    var tape = new Dictionary<int, bool>();
    int curPos = 0;
    char curState = initialState;
    
    for (int i = 0; i < stepCount; i++)
    {
        bool currentValue = tape.TryGetValue(curPos, out bool cv) && cv;
        (bool valueToWrite, int moveDelta, char nextState) = states[curState](currentValue);
        tape[curPos] = valueToWrite;
        curPos += moveDelta;
        curState = nextState;
    }
    
    return tape.Values.Count(v => v);
}

private static Given ParseInput(IEnumerable<string> input)
{
    string[] inputArray = input.ToArray();
    char initialState = inputArray[0][^2];
    int stepCount = int.Parse(inputArray[1].Split(' ')[^2]);
    Dictionary<char, ProcessState> states = [];
    List<string> stateLines = [];
    int i = 3;
    
    while (i < inputArray.Length)
    {
        stateLines.Add(inputArray[i]);
        i++;
        
        if (i < inputArray.Length && inputArray[i].Length > 0)
        {
            continue;
        }
        
        char stateName = stateLines[0][^2];
        IReadOnlyList<StateBranch> branches = [ParseBranch(stateLines, 2), ParseBranch(stateLines, 6)];
        states[stateName] = currentValue => branches[currentValue ? 1 : 0];
        stateLines = [];
        i++;
    }

    return new(initialState, stepCount, states);
    
    static StateBranch ParseBranch(IReadOnlyList<string> stateLines, int startIndex) => new(
        ValueToWrite: stateLines[startIndex][^2] is '1',
        MoveDelta: (stateLines[startIndex + 1][^5] is 'l' ? -1 : 1),
        NextState: stateLines[startIndex + 2][^2]);
}

private readonly record struct Given(
    char InitialState,
    int StepCount,
    IReadOnlyDictionary<char, ProcessState> States);

private readonly record struct StateBranch(bool ValueToWrite, int MoveDelta, char NextState);

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}