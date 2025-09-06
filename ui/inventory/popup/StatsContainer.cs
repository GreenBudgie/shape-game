public partial class StatsContainer : GridContainer
{

    public void AddStat(ModuleStat stat)
    {
        var presentation = StatPresentation.Create(stat);
        AddChild(presentation);

        var value = StatValue.Create(stat);
        AddChild(value);
    }

}
