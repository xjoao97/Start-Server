namespace Oblivion.HabboHotel.Items.Wired
{
    internal interface IWiredCycle
    {
        double Delay { get; set; }
        double TickCount { get; set; }
        bool OnCycle();
    }
}