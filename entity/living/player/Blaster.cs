using System;
using System.Collections.Generic;
using System.Linq;

public partial class Blaster : Node
{

    private const float MinDelay = 0.01f;

    private ModuleInventory _inventory = null!;

    public float Delay { get; private set; }

    public static Blaster Create(ModuleInventory inventory)
    {
        return new Blaster
        {
            _inventory = inventory
        };
    }

    public override void _Process(double delta)
    {
        if (Delay > 0)
        {
            Delay = Max(Delay - (float)delta, 0);
        }
    }

    public bool Trigger()
    {
        if (Delay > 0)
        {
            return false;
        }

        var spawnableModules = _inventory.GetModules<SpawnableModule>();
        foreach (var spawnableModule in spawnableModules)
        {
            var module = (SpawnableModule)spawnableModule.Module;
            var modifiers = spawnableModule
                .GetAllIncomingConnectedModules()
                .Select(x => x.Module)
                .OfType<ModifierModule>()
                .ToList();

            ShootProjectile(modifiers, module);
        }

        return true;
    }

    private void ShootProjectile(List<ModifierModule> modifiers, SpawnableModule spawnableModule)
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            throw new Exception("Blaster cannot fire - player wasn't found");
        }
        
        var context = new SpawnableContext(spawnableModule.CreateSpawnable())
        {
            Position = player.GetGlobalNosePosition(),
            Direction = Vector2.FromAngle(player.GetTilt() - Pi / 2),
            Source = player
        };
        
        var modifierStats = modifiers.SelectMany(modifier => modifier.Stats);
        context.Stats.AddRange(modifierStats);
        
        context.Stats.AddRange(spawnableModule.Stats);

        foreach (var modifier in modifiers)
        {
            modifier.Modify(context);
            context.AppliedModifiers.Add(modifier);
        }
        
        context.Spawn();

        var reload = context.CalculateStat<ReloadStat>();
        Delay += Max(reload, MinDelay);
    }

}