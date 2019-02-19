namespace Oblivion.Communication.Packets.Outgoing.Notifications
{
    internal class TargetOfferComposer : ServerPacket
    {
        public TargetOfferComposer() : base(ServerPacketHeader.TargetOfferMessageComposer)
        {
            WriteInteger(1);
            WriteInteger(190);
            WriteString("bf16_tko_gr1");
            WriteString("bf16_tko1");
            WriteInteger(105); //Credits 
            WriteInteger(105); //Diamonds 
            WriteInteger(5);
            WriteInteger(2);
            WriteInteger(259199); //3 Days ... time in seconds 
            WriteString("targeted.offer.bf16_tko_gr1.title"); //Title 
            WriteString("targeted.offer.bf16_tko_gr1.desc"); //Description 
            WriteString("targetedoffers/tko_xmas16.png"); //Image Large 
            WriteString("targetedoffers/tto_blkfri_20_small.png"); //Image on Close Notification 
            WriteInteger(1);
            WriteInteger(15);
            WriteString("HC_1_MONTH_INTERNAL"); //1 Month HC 
            WriteString("xmas13_snack"); //Snack 
            WriteString("deal_10bronzecoins"); //10 Credits  
            WriteString("xmas_c15_roof1"); //Roof Building 
            WriteString("xmas12_snack"); //Snack 
            WriteString("deal_10bronzecoins"); //10 Credits 
            WriteString("deal_10bronzecoins"); //10 Credits 
            WriteString("xmas_c15_buildmid1"); //Building 1 
            WriteString("xmas_c15_buildbase1"); //Building 2 
            WriteString("deal_10bronzecoins"); //10 Credits 
            WriteString("clothing_longscarf"); //Clothes Scarf 
            WriteString("deal_10bronzecoins"); //10 Credits 
            WriteString("deal_10bronzecoins"); //10 Credits 
            WriteString("BADGE"); // 
            WriteString("deal_10bronzecoins"); //10 Credits  
        }
    }
}