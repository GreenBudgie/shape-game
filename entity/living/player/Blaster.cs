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

        var spawnableModules = _inventory.GetModules<SpawnableModuleType>();

        List<SpawnableData> spawnables = [];
        foreach (var spawnableModule in spawnableModules)
        {
            var module = (SpawnableModule)spawnableModule.Module;
            
            var incomingModules = spawnableModule.GetAllIncomingConnectedModules();
            var outgoingModules = spawnableModule.GetAllOutgoingConnectedModules();
            
            var modifiers = incomingModules
                .Select(inventoryModule => inventoryModule.Module)
                .OfType<ModifierModule>()
                .Select(modifier => modifier.CreateModifier())
                .ToList();
            
            var incomingDelegators = incomingModules
                .Where(inventoryModule => inventoryModule.Module.Type is DelegatorModuleType)
                .ToHashSet();
            var outgoingDelegators = outgoingModules
                .Where(inventoryModule => inventoryModule.Module.Type is DelegatorModuleType)
                .ToHashSet();

            spawnables.Add(new SpawnableData(module.CreateProducer(), modifiers, incomingDelegators, outgoingDelegators));
        }
        
        var spawnablesWithoutDelegators = spawnables.Where(spawnable => spawnable.IncomingDelegatorModules.Count == 0).ToList();
        foreach (var spawnable in spawnablesWithoutDelegators)
        {
            var context = CreateContext(spawnables, spawnable);
            Spawn(context);
        }

        return true;
    }

    private SpawnableContext CreateContext(List<SpawnableData> allData, SpawnableData data)
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            throw new Exception("Blaster cannot fire - player wasn't found");
        }

        var context = new SpawnableContext(data.SpawnableProducer)
        {
            Position = player.GetGlobalNosePosition(),
            Direction = Vector2.FromAngle(player.GetTilt() - Pi / 2),
            Source = player,
            Modifiers = data.Modifiers
        };

        foreach (var delegatorModule in data.OutgoingDelegatorModules)
        {
            var spawnablesToDelegate = allData
                .Where(currentData => currentData.IncomingDelegatorModules.Contains(delegatorModule))
                .ToHashSet();

            foreach (var spawnableToDelegate in spawnablesToDelegate)
            {
                var delegator = ((DelegatorModule)delegatorModule.Module).CreateDelegator();
                var delegatedContext = CreateContext(allData, spawnableToDelegate);
                context.AddLifecycleDelegator(delegator, delegatedContext);
            }
        }
        
        context.CalculateStats();

        return context;
    }

    private void Spawn(SpawnableContext context)
    {
        context.ForceSpawn();

        var reload = context.GetStatWithDelegated<ReloadStat>();
        Delay += Max(reload, MinDelay);
    }

    private readonly record struct SpawnableData(
        ISpawnableProducer SpawnableProducer,
        List<ISpawnableModifier> Modifiers,
        HashSet<InventoryModule> IncomingDelegatorModules,
        HashSet<InventoryModule> OutgoingDelegatorModules
    );

}