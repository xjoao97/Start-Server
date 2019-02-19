#region

using System.Linq;
using Oblivion.HabboHotel.Rooms.Trading;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingUpdateComposer : ServerPacket
    {
        public TradingUpdateComposer(Trade Trade)
            : base(ServerPacketHeader.TradingUpdateMessageComposer)
        {
            if (Trade.Users.Length < 2)
                return;


            var User1 = Trade.Users.First();
            var User2 = Trade.Users.Last();


            WriteInteger(User1.GetClient().GetHabbo().Id);
            SerializeUserItems(User1);


            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(1);


            SerializeUserItems(User2);


            WriteInteger(0);
            WriteInteger(0);


/*base.WriteInteger(User.GetClient().GetHabbo().Id);
                        base.WriteInteger(User.OfferedItems.Count);
            
            
                        foreach (Item Item in User.OfferedItems.ToList())
                        {
                            base.WriteInteger(Item.Id);
                           base.WriteString(Item.GetBaseItem().Type.ToString().ToLower());
                            base.WriteInteger(Item.Id);
                            base.WriteInteger(Item.Data.SpriteId);
                            base.WriteInteger(0);//Not sure.
                            if (Item.LimitedNo > 0)
                            {
                                base.WriteBoolean(false);//Stackable
                                base.WriteInteger(256);
                               base.WriteString("");
                                base.WriteInteger(Item.LimitedNo);
                                base.WriteInteger(Item.LimitedTot);
                            }
                            else
                            {
                                base.WriteBoolean(true);//Stackable
                                base.WriteInteger(0);
                               base.WriteString("");
                            }
            
            
                            base.WriteInteger(0);
                            base.WriteInteger(0);
                            base.WriteInteger(0);
            
            
                            if (Item.GetBaseItem().Type == 's')
                                base.WriteInteger(0);
            
            
                            base.WriteInteger(0);
                            base.WriteInteger(0);
                            base.WriteInteger(-1);*/
        }

        private void SerializeUserItems(TradeUser User)
        {
            WriteInteger(User.OfferedItems.Count); //While
            foreach (var Item in User.OfferedItems.ToList())
            {
                WriteInteger(Item.Id);
                WriteString(Item.Data.Type.ToString().ToUpper());
                WriteInteger(Item.Id);
                WriteInteger(Item.Data.SpriteId);
                WriteInteger(0);
                WriteBoolean(true);


                //Func called _SafeStr_15990
                WriteInteger(0);
                WriteString("");


                //end Func called
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
                if (Item.Data.Type.ToString().ToUpper() == "S")
                    WriteInteger(0);
            }
            //End of while
        }
    }
}