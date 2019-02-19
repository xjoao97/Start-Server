#region

using System;
using System.Threading.Tasks;
using ConnectionManager;
using Oblivion.Communication.Packets;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.GameClients;
using Oblivion.Utilities;
using SharedPacketLib;

#endregion

namespace Oblivion.Net
{
    public class GamePacketParser : IDataParser
    {
        public delegate void HandlePacket(ClientPacket message);

//        private static readonly ILog log = LogManager.GetLogger("Oblivion.Net.GamePacketParser");

        private readonly GameClient _currentClient;
//        private bool _deciphered;
//        private byte[] _halfData;

//        private bool _halfDataRecieved;
        // private ConnectionInformation con;

        public GamePacketParser(GameClient me)
        {
            _currentClient = me;
        }

        /*  public void HandlePacketData(byte[] Data)
          {
              try
              { 
                  if (_halfDataRecieved)
                  {
                      var FullDataRcv = new byte[_halfData.Length + Data.Length];
                      Buffer.BlockCopy(_halfData, 0, FullDataRcv, 0, _halfData.Length);
                      Buffer.BlockCopy(Data, 0, FullDataRcv, _halfData.Length, Data.Length);

                      _halfDataRecieved = false; // mark done this round
                      handlePacketData(FullDataRcv); // repeat now we have the combined array
                      return;
                  }

                  using (var Reader = new BinaryReader(new MemoryStream(Data)))
                  {
                      if (Data.Length < 4)
                          return;

                      var MsgLen = HabboEncoding.DecodeInt32(Reader.ReadBytes(4));
                      if (Reader.BaseStream.Length - 4 < MsgLen)
                      {
                          _halfData = Data;
                          _halfDataRecieved = true;
                          return;
                      }
                      if (MsgLen < 0 || MsgLen > 5120) //TODO: Const somewhere.
                          return;

                      var Packet = Reader.ReadBytes(MsgLen);

                      using (var R = new BinaryReader(new MemoryStream(Packet)))
                      {
                          var Header = HabboEncoding.DecodeInt16(R.ReadBytes(2));

                          var Content = new byte[Packet.Length - 2];
                          Buffer.BlockCopy(Packet, 2, Content, 0, Packet.Length - 2);

                          var Message = new ClientPacket(Header, Content);
                          onNewPacket.Invoke(Message);

  //                        _deciphered = false;
                      }

                      if (Reader.BaseStream.Length - 4 > MsgLen)
                      {
                          var Extra = new byte[Reader.BaseStream.Length - Reader.BaseStream.Position];
                          Buffer.BlockCopy(Data, (int) Reader.BaseStream.Position, Extra, 0,
                              (int) (Reader.BaseStream.Length - Reader.BaseStream.Position));

  //                        _deciphered = true;
                          handlePacketData(Extra);
                      }
                  }
              }
              catch (Exception e)
              {
                  log.Error("Packet Error!", e);
              }
          }*/

        public void handlePacketData(byte[] data)
        {
            var pos = 0;
            while (pos < data.Length)
                try
                {
                    var messageLength =
                        HabboEncoding.DecodeInt32(new[] {data[pos++], data[pos++], data[pos++], data[pos++]});
                    if (messageLength < 2 || messageLength > 1024)
                        continue;

                    var messageId = HabboEncoding.DecodeInt16(new[] {data[pos++], data[pos++]});
                    var content = new byte[messageLength - 2];

                    Buffer.BlockCopy(data, pos, content, 0, messageLength - 2);
                    pos += messageLength - 2;

                    if (onNewPacket == null) continue;

                    using (var message = ClientMessageFactory.GetClientMessage(messageId, content))
                    {
                        if (_currentClient.GetHabbo() == null)
                        {
                            onNewPacket.Invoke(message);
                        }
                        else
                        {
                            Task task;
                            switch (messageId)
                            {
                                case ClientPacketHeader.HabboSearchMessageEvent:
                                case ClientPacketHeader.NewNavigatorSearchMessageEvent:
                                case ClientPacketHeader.UpdateFigureDataMessageEvent:
                                case ClientPacketHeader.ChangeNameMessageEvent:
                                case ClientPacketHeader.AcceptBuddyMessageEvent:
                                case ClientPacketHeader.DeclineBuddyMessageEvent:
                                case ClientPacketHeader.RequestBuddyMessageEvent:
                                case ClientPacketHeader.RemoveBuddyMessageEvent:
                                case ClientPacketHeader.PurchaseFromCatalogMessageEvent:
                                case ClientPacketHeader.PurchaseFromCatalogAsGiftMessageEvent:
                                case ClientPacketHeader.OpenGiftMessageEvent:
                                case ClientPacketHeader.CreditFurniRedeemMessageEvent:
                                case ClientPacketHeader.GetRelationshipsMessageEvent:
                                case ClientPacketHeader.DeleteStickyNoteMessageEvent:
                                case ClientPacketHeader.SaveRoomSettingsMessageEvent:
                                case ClientPacketHeader.DeleteRoomMessageEvent:
                                case ClientPacketHeader.SetRelationshipMessageEvent:
                                case ClientPacketHeader.RequestFurniInventoryMessageEvent:
                                case ClientPacketHeader.MoveObjectMessageEvent:
                                case ClientPacketHeader.SetObjectDataMessageEvent:
                                    task = OblivionServer.QueueHeavyPackets.StartNew(() => onNewPacket.Invoke(message));

                                    task.Wait();
                                    break;

                                case ClientPacketHeader.SendMsgMessageEvent:
                                case ClientPacketHeader.MoveAvatarMessageEvent:
                                case ClientPacketHeader.ShoutMessageEvent:
                                case ClientPacketHeader.DanceMessageEvent:
                                case ClientPacketHeader.ActionMessageEvent:
                                case ClientPacketHeader.ApplySignMessageEvent:
                                case ClientPacketHeader.SitMessageEvent:
                                case ClientPacketHeader.CancelTypingMessageEvent:
                                case ClientPacketHeader.StartTypingMessageEvent:
                                case ClientPacketHeader.WhisperMessageEvent:
                                case ClientPacketHeader.ThrowDiceMessageEvent:
                                case ClientPacketHeader.DiceOffMessageEvent:

                                    task = OblivionServer.QueuePriorityPackets.StartNew(() =>
                                    {
                                        if (message != null)
                                            onNewPacket.Invoke(message);
                                    });

                                    task.Wait();
                                    break;

                                default:

                                    task = OblivionServer.QueueLowPackets.StartNew(() =>
                                    {
                                        if (message != null)
                                            onNewPacket.Invoke(message);
                                    });

                                    task.Wait();
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
        }

        public void Dispose()
        {
            onNewPacket = null;
            GC.SuppressFinalize(this);
        }

        public object Clone() => new GamePacketParser(_currentClient);

        public event HandlePacket onNewPacket;

        public void SetConnection(ConnectionInformation con) => onNewPacket = null;
    }
}