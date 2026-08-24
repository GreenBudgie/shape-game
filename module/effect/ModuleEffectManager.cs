using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public partial class ModuleEffectManager : Node2D
{

    public static ModuleEffectManager Instance { get; private set; } = null!;

    private static readonly ImmutableHashSet<ModuleEffectPlace> Places = GetEffectPlaces();

    private static ImmutableHashSet<ModuleEffectPlace> GetEffectPlaces()
    {
        const float numberOfPlaces = 10f;
        var start = ShapeGame.PlayableArea.Size.X * 0.2f;
        var end = ShapeGame.PlayableArea.Size.X * 0.8f;
        var step = (end - start) / numberOfPlaces;
        var places = new HashSet<ModuleEffectPlace>();
        for (var i = 0; i <= numberOfPlaces; i++)
        {
            places.Add(new ModuleEffectPlace(start + i * step));
        }

        return places.ToImmutableHashSet();
    }

    public ModuleEffectManager()
    {
        Instance = this;
    }

    public ModuleEffect ShowEffect(Module module, string text, Color textColor)
    {
        var alreadyActiveEffect = GetEffects().FirstOrDefault(effect => effect.Module == module);
        if (alreadyActiveEffect != null)
        {
            alreadyActiveEffect.ProlongWithText(text, textColor);
            return alreadyActiveEffect;
        }
        
        var effect = ModuleEffect.Create(GetRandomFreePlace(), module, text, textColor);
        AddChild(effect);

        return effect;
    }
    
    private ModuleEffectPlace GetRandomFreePlace()
    {
        var occupiedPlaces = GetEffects().Select(effect => effect.Place).ToHashSet();
        var freePlaces = Places.Where(place => !occupiedPlaces.Contains(place)).ToHashSet();
        if (freePlaces.Count == 0)
        {
            return Places.GetRandom();
        }

        return freePlaces.GetRandom();
    }
    
    private IEnumerable<ModuleEffect> GetEffects()
    {
        return GetChildren().Cast<ModuleEffect>();
    }
    
}