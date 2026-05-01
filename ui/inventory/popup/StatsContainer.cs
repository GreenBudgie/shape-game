public partial class StatsContainer : GridContainer
{

    public void AddStat(SpawnableStat stat)
    {
        var presentation = StatPresentation.Create(stat);
        AddChild(presentation);

        var value = StatValue.Create(stat);
        AddChild(value);
    }

}
