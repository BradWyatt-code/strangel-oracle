using StrangelOracle.Domain.Enums;

namespace StrangelOracle.Domain.Entities;

/// <summary>
/// A Strangel - a Strange Angel inhabiting modern New York
/// </summary>
public sealed class Strangel
{
    public StrangelType Type { get; private set; }
    public string Name { get; private set; }
    public string Title { get; private set; }
    public string Aspect { get; private set; }
    public string Function { get; private set; }
    public string Disposition { get; private set; }
    public string[] Domains { get; private set; }
    public string[] Manifestations { get; private set; }
    public string RitualInstruction { get; private set; }
    
    private Strangel() { }
    
    public static Strangel WomanWithHeart => new()
    {
        Type = StrangelType.WomanWithHeart,
        Name = "The Woman with Heart",
        Title = "She Who Bears",
        Aspect = "Devotion without irony",
        Function = "Bearing emotional surplus",
        Disposition = "Quietly radiant. Inexhaustibly burdened.",
        Domains = new[] 
        { 
            "Comfort", 
            "Absorption", 
            "Release", 
            "The unnamed", 
            "The overlooked" 
        },
        Manifestations = new[]
        {
            "Hospital waiting rooms at 3 a.m.",
            "The last car of late-night trains",
            "Back pews of churches that no longer hold services",
            "Anywhere people sit with what they cannot say"
        },
        RitualInstruction = "You do not pray to her. You do not speak. You touch."
    };
    
    public static Strangel Fox => new()
    {
        Type = StrangelType.Fox,
        Name = "The Fox",
        Title = "Murat Askarov, The Possessed",
        Aspect = "Cunning without cruelty",
        Function = "Destabilizing certainty",
        Disposition = "Quiet, curious, observant. A wildness under the calm.",
        Domains = new[] 
        { 
            "Luck", 
            "Trickery", 
            "Crossing", 
            "Riddles", 
            "The space between worlds" 
        },
        Manifestations = new[]
        {
            "A rickshaw moving against traffic",
            "Amber eyes in subway windows",
            "Laughter from empty alleys",
            "The feeling of being watched and liked"
        },
        RitualInstruction = "Petition him with a question. Accept that he may not answer, or may answer wrong on purpose."
    };
    
    public static Strangel Furies => new()
    {
        Type = StrangelType.Furies,
        Name = "The Furies",
        Title = "Alecto, Megaera, Tisiphone",
        Aspect = "Judgment without mercy",
        Function = "Enforcing moral consequence",
        Disposition = "Thunder in the skull. Ancient and unimpressed.",
        Domains = new[] 
        { 
            "Wrath", 
            "Envy", 
            "Vengeance", 
            "Conscience", 
            "The debt that must be paid" 
        },
        Manifestations = new[]
        {
            "Seagulls watching from lampposts",
            "Three shadows where one should fall",
            "The sound of wings in courtrooms",
            "Dreams of being followed"
        },
        RitualInstruction = "Confess what weighs on you. They will judge. They always judge."
    };
    
    public static Strangel Nokso => new()
    {
        Type = StrangelType.Nokso,
        Name = "Nok'so",
        Title = "The Falcon, The Disruptor",
        Aspect = "Protection through chaos",
        Function = "Breaking what needs breaking",
        Disposition = "Sharp. Sudden. Gone before you understand.",
        Domains = new[] 
        { 
            "Protection", 
            "Disruption", 
            "Childhood", 
            "Memory", 
            "The strike that saves" 
        },
        Manifestations = new[]
        {
            "A falcon circling buildings",
            "The moment before an accident doesn't happen",
            "Childhood memories surfacing unbidden",
            "Things falling from shelves"
        },
        RitualInstruction = "Invoke her when you need something broken. She will decide if you're right."
    };
    
    public static Strangel GetByType(StrangelType type) => type switch
    {
        StrangelType.WomanWithHeart => WomanWithHeart,
        StrangelType.Fox => Fox,
        StrangelType.Furies => Furies,
        StrangelType.Nokso => Nokso,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
