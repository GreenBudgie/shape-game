using System;
using System.Collections.Generic;
using System.Linq;

public partial class Blaster : Node
{

    private const float MinDelay = 0.01f;

    private BlasterInventory _inventory = null!;
    private int _lastSlot;

    public float Delay { get; private set; }

    public static Blaster Create(BlasterInventory inventory)
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
            Delay -= (float)delta;
        }
    }

    public bool Trigger()
    {
        if (Delay > 0)
        {
            return false;
        }

        var slots = _inventory.GetSlots();
        var startSlot = _lastSlot;
        var modifiers = new List<ModifierModule>();
        for (var i = startSlot; NextSlot() != startSlot; i = NextSlot())
        {
            _lastSlot = i;
            var slot = slots[i];
            var module = slot.GetModule()?.Module;

            if (module == null)
            {
                continue;
            }

            if (module is ModifierModule modifierModule)
            {
                modifiers.Add(modifierModule);
                continue;
            }

            if (module is SpawnableModule projectileModule)
            {
                ShootProjectile(modifiers, projectileModule);
                _lastSlot = NextSlot();
                return true;
            }
        }

        return false;
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
            Direction = Vector2.FromAngle(player.GetTilt()),
            Source = player
        };
        
        var modifierStats = modifiers.SelectMany(modifier => modifier.Stats);
        context.Stats.AddRange(modifierStats);

        foreach (var modifier in modifiers)
        {
            modifier.Modify(context);
            context.AppliedModifiers.Add(modifier);
        }
        
        context.Spawn();

        var reload = context.CalculateStat<ReloadStat>();
        Delay = Max(reload, MinDelay);
    }

    private int NextSlot()
    {
        return _lastSlot + 1 >= _inventory.GetSlots().Count ? 0 : _lastSlot + 1;
    }

}