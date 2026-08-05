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

        List<SpawnableData> spawnables = [];
        foreach (var spawnableModule in spawnableModules)
        {
            var module = (SpawnableModule)spawnableModule.Module;
            
            var incomingModules = spawnableModule.GetAllIncomingConnectedModules();
            var outgoingModules = spawnableModule.GetAllOutgoingConnectedModules();
            
            var modifiers = incomingModules
                .Select(inventoryModule => inventoryModule.Module)
                .OfType<ModifierModule>()
                .ToList();
            
            var incomingTriggers = incomingModules
                .Where(inventoryModule => inventoryModule.Module is TriggerModule)
                .ToHashSet();
            var outgoingTriggers = outgoingModules
                .Where(inventoryModule => inventoryModule.Module is TriggerModule)
                .ToHashSet();

            spawnables.Add(new SpawnableData(module, modifiers, incomingTriggers, outgoingTriggers));
        }
        
        var spawnablesWithoutTriggers = spawnables.Where(spawnable => spawnable.IncomingTriggers.Count == 0).ToList();
        foreach (var spawnable in spawnablesWithoutTriggers)
        {
            var context = CreateContext(spawnables, spawnable);
            Spawn(context);
        }

        return true;
    }

    private SpawnableContext CreateContext(List<SpawnableData> allSpawnables, SpawnableData spawnable)
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            throw new Exception("Blaster cannot fire - player wasn't found");
        }
        
        var context = new SpawnableContext(spawnable.SpawnableModule.CreateSpawnable())
        {
            Position = player.GetGlobalNosePosition(),
            Direction = Vector2.FromAngle(player.GetTilt() - Pi / 2),
            Source = player,
            Modifiers = spawnable.Modifiers
        };
        
        context.Stats.AddRange(spawnable.SpawnableModule.Stats);

        foreach (var triggerModule in spawnable.OutgoingTriggers)
        {
            var spawnablesToTrigger = allSpawnables
                .Where(currentSpawnable => currentSpawnable.IncomingTriggers.Contains(triggerModule))
                .ToHashSet();
            
            foreach (var spawnableToTrigger in spawnablesToTrigger)
            {
                var triggerContext = CreateContext(allSpawnables, spawnableToTrigger);
                context.Triggers.Add(triggerContext);
            }
        }

        return context;
    }

    private void Spawn(SpawnableContext context)
    {
        context.Spawn();

        var reload = context.CalculateStatWithTriggers<ReloadStat>();
        Delay += Max(reload, MinDelay);
    }

    private readonly record struct SpawnableData(
        SpawnableModule SpawnableModule,
        List<ModifierModule> Modifiers,
        HashSet<InventoryModule> IncomingTriggers,
        HashSet<InventoryModule> OutgoingTriggers
    );

}