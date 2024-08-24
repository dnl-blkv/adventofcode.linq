<Query Kind="Program">
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

void Main()
{
    (Shop shop, Fighter boss) = ParseInput(GetInput());
	Solve1(shop, boss).Dump();
	Solve2(shop, boss).Dump();
}

const int PlayerHitPoints = 100;

private static int Solve1(Shop shop, Fighter boss) =>
    GetAllLoadoutsAsCompositeItems(shop).Where(i => WillBeatTheBoss(i, boss)).Min(i => i.Cost);

private static int Solve2(Shop shop, Fighter boss) =>
    GetAllLoadoutsAsCompositeItems(shop).Where(i => !WillBeatTheBoss(i, boss)).Max(i => i.Cost);

private static IEnumerable<Item> GetAllLoadoutsAsCompositeItems(Shop shop) =>
    shop.GetAllLoadouts().Select(l => l.Aggregate((r, n) => r.Fuse(n)));
    
private static bool WillBeatTheBoss(Item playerLoadout, Fighter boss) =>
    new Fighter(PlayerHitPoints, playerLoadout.Damage, playerLoadout.Armor).InitiateFight(boss);

private static IEnumerable<IEnumerable<T>> Multiply<T>(
    IEnumerable<IReadOnlyCollection<T>> one,
    IReadOnlyCollection<IReadOnlyCollection<T>> another) =>
    one.SelectMany(oneCollection => another.Select(anotherCollection => oneCollection.Concat(anotherCollection)));

private static IEnumerable<IEnumerable<T>> Multiply<T>(
    IEnumerable<IReadOnlyCollection<T>> one,
    IEnumerable<IReadOnlyCollection<IReadOnlyCollection<T>>> others) =>
    others.Aggregate(one, (result, next) => Multiply(result, next).Select(e => e.ToArray()));

private static IEnumerable<IEnumerable<T>> GetAllSubsets<T>(IReadOnlyList<T> items, int subsetSize, int startAt = 0)
{
    if (items.Count - startAt < subsetSize)
    {
        yield break;
    }
    
    if (subsetSize == 0)
    {
        yield return Enumerable.Empty<T>();
        yield break;
    }
    
    for (int i = startAt; i < items.Count; i++)
    {
        foreach (IEnumerable<T> subset in GetAllSubsets(items, subsetSize - 1, startAt: i + 1))
        {
            yield return subset.Append(items[i]);
        }
    }
}

private static (Shop shop, Fighter boss) ParseInput(IEnumerable<string> input)
{
    var itemLists = new List<List<string>>();
    List<string>? nextList = new List<string>();
    
    foreach (string line in input)
    {
        if (line is { Length: 0 })
        {
            itemLists.Add(nextList);
            nextList = new List<string>();
            continue;
        }
        
        nextList.Add(line);
    }
    
    return (shop: Shop.Parse(itemLists.ToArray()), boss: Fighter.Parse(nextList));
}
    
private class Shop
{
    private readonly IReadOnlyDictionary<(int MinCount, int MaxCount), IReadOnlyList<Item>> itemCollections;
    
    private Shop(IReadOnlyDictionary<(int MinCount, int MaxCount), IReadOnlyList<Item>> itemCollections) =>
        this.itemCollections = itemCollections;
    
    public static Shop Parse(IReadOnlyList<IReadOnlyList<string>> lines)
    {
        IReadOnlyList<IReadOnlyList<Item>> itemLists = lines.Select(ParseItemList).ToArray();
        
        return new Shop(
            itemCollections: new Dictionary<(int MinCount, int MaxCount), IReadOnlyList<Item>>
            {
                [(MinCount: 1, MaxCount: 1)] = itemLists[0],
                [(MinCount: 0, MaxCount: 1)] = itemLists[1],
                [(MinCount: 0, MaxCount: 2)] = itemLists[2]
            });
        
        IReadOnlyList<Item> ParseItemList(IReadOnlyList<string> itemStringList) =>
            itemStringList.Select(Item.Parse).ToArray();
    }
    
    public IEnumerable<IReadOnlyCollection<Item>> GetAllLoadouts()
    {
        IReadOnlyCollection<IReadOnlyCollection<Item>>? one = null;
        var others = new List<IReadOnlyCollection<IReadOnlyCollection<Item>>>();
        
        foreach (((int minCount, int maxCount), IReadOnlyList<Item> items) in this.itemCollections)
        {
            IReadOnlyCollection<IReadOnlyCollection<Item>> nextSubLoadouts =
                GetSubLoadouts(items, minCount, maxCount).ToArray();
                
            if (one is null)
            {
                one = nextSubLoadouts;
                continue;
            }
            
            others.Add(nextSubLoadouts);
        }
        
        return Multiply(one!, others).Select(l => l.ToArray());
    }
    
    private static IEnumerable<IReadOnlyCollection<Item>> GetSubLoadouts(IReadOnlyList<Item> items, int minCount, int maxCount)
    {
        for (int subLoadoutSize = minCount; subLoadoutSize <= maxCount; subLoadoutSize++)
        {
            foreach (IEnumerable<Item> subLoadout in GetAllSubsets<Item>(items, subLoadoutSize))
            {
                yield return subLoadout.ToArray();
            };
        }
    }
}

private class Item
{
    private Item(int cost, int damage, int armor)
    {
        this.Cost = cost;
        this.Damage = damage;
        this.Armor = armor;
    }
    
    public int Cost { get; }
    
    public int Damage { get; }
    
    public int Armor { get; }

    public static Item Parse(string line)
    {
        int[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(v => int.Parse(v)).ToArray();
        
        return new Item(cost: values[0], damage: values[1], armor: values[2]);
    }
    
    public Item Fuse(Item other) =>
        new Item(
            cost: this.Cost + other.Cost,
            damage: this.Damage + other.Damage,
            armor: this.Armor + other.Armor);
}

private class Fighter
{
    private readonly int hitPoints;
    private readonly int damage;
    private readonly int armor;
    
    public Fighter(int hitPoints, int damage, int armor)
    {
        this.hitPoints = hitPoints;
        this.damage = damage;
        this.armor = armor;
    }

    public static Fighter Parse(IReadOnlyList<string> lines)
    {
        int[] values = lines.Select(l => int.Parse(l.Split(": ")[1])).ToArray();
        
        return new Fighter(hitPoints: values[0], damage: values[1], armor: values[2]);
    }
   
    public bool InitiateFight(Fighter enemy) =>
        CountAttacksToWin(this, enemy) <= CountAttacksToWin(enemy, this);
        
    private static int CountAttacksToWin(Fighter attacker, Fighter defender) =>
        (int)Math.Ceiling(((double)defender.hitPoints) / CalculateDamage(attacker, defender));
    
    private static int CalculateDamage(Fighter attacker, Fighter defender) =>
        Math.Max(attacker.damage - defender.armor, 1);
}

private static IEnumerable<string> GetInput()
{
	using var inputStream = new StreamReader($"{Util.CurrentQueryPath[..^5]}_Input.txt");
	while (inputStream.ReadLine() is { } line) yield return line;
}