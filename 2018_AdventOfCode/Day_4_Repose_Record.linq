<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    IReadOnlyDictionary<int, List<GuardShift>> input = ParseInput(GetInput());
	Solve(input).Dump();
}

private const int MinutesInHour = 60;

private static (long Result1, long Result2) Solve(IReadOnlyDictionary<int, List<GuardShift>> input)
{
    IReadOnlyDictionary<int, int[]> sleepyMinutesByGuardId =
        input.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                int[] mostSleepyMinutes = Enumerable.Repeat(0, MinutesInHour).ToArray();
    
                foreach ((int awakeMinute, int asleepMinute) in input[kv.Key].SelectMany(gs => gs.SleepyMinutes))
                {
                    for (int m = asleepMinute; m < awakeMinute; m++)
                    {
                        mostSleepyMinutes[m]++;
                    }
                }
                
                return mostSleepyMinutes;
            });
    
    return (Result1: GetResult(GetSleepiestGuardSleepiestMinute), Result2: GetResult(GetSleepiestMinute));
        
    int GetResult(Func<int[], (int C, int I)> selectionStrategy)
    {
        ((int _, int resultGuardMostSleepyMinute), int resultGuardId) =
            sleepyMinutesByGuardId
                .Select(kv => (M: selectionStrategy.Invoke(kv.Value), Id: kv.Key))
                .MaxBy(t => t.M.C);
                
        return resultGuardMostSleepyMinute * resultGuardId;
    }
    
    (int C, int I) GetSleepiestGuardSleepiestMinute(int[] sleepyMinutes) =>
        (C: sleepyMinutes.Sum(), I: GetSleepiestMinute(sleepyMinutes).I);
    
    (int C, int I) GetSleepiestMinute(int[] sleepyMinutes) =>
        sleepyMinutes.Select((c, i) => (c, i)).MaxBy(t => t.c);
}

private static IReadOnlyDictionary<int, List<GuardShift>> ParseInput(IEnumerable<string> input)
{
    Dictionary<int, List<GuardShift>> shifts = [];
    int currentGuardId = -1;
    List<string> guardShiftLines = [];
    
    foreach (string line in input.OrderBy(l => l).Append(" #-1 "))
    {
        if (TryGetGuardId(line, out int nextGuardId))
        {
            if (currentGuardId != -1)
            {
                if (!shifts.ContainsKey(currentGuardId))
                {
                    shifts[currentGuardId] = [];
                }
                
                shifts[currentGuardId].Add(GuardShift.Parse(guardShiftLines));
                guardShiftLines = [];
            }
            
            currentGuardId = nextGuardId;
        }
    
        guardShiftLines.Add(line);
    }
    
    return shifts;
    
    bool TryGetGuardId(string line, out int guardId)
    {
        int hashPos = line.IndexOf('#');
        guardId = -1;
        
        return hashPos > 0 && int.TryParse(line.AsSpan()[(hashPos + 1)..line.IndexOf(' ', hashPos)], out guardId);
    }
}

private record struct GuardShift(OldTime Begin, IReadOnlyList<(OldTime Asleep, OldTime Awake)> SleepyTimes)
{
    public IEnumerable<(int Awake, int Asleep)> SleepyMinutes =>
        SleepyTimes.Select(st => (Awake: st.Awake.Minute, Asleep: st.Asleep.Minute));

    public static GuardShift Parse(IReadOnlyList<string> shiftLines)
    {
        OldTime beginTime = ParseTime(shiftLines[0]);
        List<(OldTime Asleep, OldTime Awake)> sleepyTimes = [];
        
        for (int i = 1; i < shiftLines.Count; i += 2)
        {
            sleepyTimes.Add((Asleep: ParseTime(shiftLines[i]), Awake: ParseTime(shiftLines[i + 1])));
        }
        
        return new GuardShift(beginTime, sleepyTimes);
        
        OldTime ParseTime(string shiftLine) => OldTime.Parse(shiftLine[1..shiftLine.IndexOf(']')]);
    }
}

private record struct OldTime(int Year, int Month, int Day, int Hour, int Minute)
{
    private static readonly char[] TimeComponentSeparators = ['-', ' ', ':'];

    public static OldTime Parse(string oldTimeLine)
    {
        int[] timeComponents = oldTimeLine.Split(TimeComponentSeparators).Select(int.Parse).ToArray();
        
        return new OldTime(
            Year: timeComponents[0],
            Month: timeComponents[1],
            Day: timeComponents[2],
            Hour: timeComponents[3],
            Minute: timeComponents[4]);
    }
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}