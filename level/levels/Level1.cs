using System.Collections.Generic;

public class Level1 : Level
{
    public override int Number => 1;
    public override int DestroyRequirement => 10;
    public override float PhaseDuration => 30;
    public override float PhaseDurationDec => 5;
    public override float MinPhaseDuration => 5;
    public override int EnemiesPerPhase => 5;
    public override float EnemiesPerPhaseInc => 1;
    public override int MaxEnemiesPerPhase => 10;

    public override List<EnemyTypeDistribution> EnemyTypeDistributions =>
    [
        new(EnemyTypeRegistry.Square)
    ];
}