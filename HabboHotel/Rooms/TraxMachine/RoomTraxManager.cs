#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Rooms.TraxMachine
{
    //TODO: REWRITE - this is really hardcoded
    public class RoomTraxManager
    {
        private DataTable _dataTable;
        public int Capacity = 10;

        public Room Room { get; set; }
        public List<Item> Playlist { get; set; }
        public bool IsPlaying { get; set; }
        public int StartedPlayTimestamp { get; private set; }
        public Item SelectedDiscItem { get; private set; }

        public TraxMusicData AnteriorMusic { get; private set; }
        public Item AnteriorItem { get; private set; }

        public int TimestampSinceStarted => (int) OblivionServer.GetUnixTimestamp() - StartedPlayTimestamp;

        public int TotalPlayListLength
            =>
                Playlist.Select(item => OblivionServer.GetGame().GetSoundManager().GetMusic(GetExtraData(item)))
                    .Where(music => music != null)
                    .Sum(music => music.Length);

        public Item ActualSongData
        {
            get
            {
                var line = GetPlayLine().Reverse();
                var now = TimestampSinceStarted;
                return now > TotalPlayListLength
                    ? null
                    : (from item in line where item.Key <= now select item.Value).FirstOrDefault();
            }
        }

        public int ActualSongTimePassed
        {
            get
            {
                var line = GetPlayLine();
                var indextime = 0;
                foreach (var music in line.Where(music => music.Value == ActualSongData))
                    indextime = music.Key;
                return TimestampSinceStarted - indextime;
            }
        }

        public int GetExtraData(Item item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return Convert.ToInt32(item.ExtraData);
            int extradata;
            var baseItem = item.BaseItem;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("SELECT extradata FROM catalog_items WHERE item_id = '" + baseItem + "' LIMIT 1");
                extradata = dbClient.getInteger();
                item.ExtraData = extradata.ToString();
            }

            return extradata;
        }

        public void Init(Room room)
        {
            IsPlaying = false;

            StartedPlayTimestamp = (int) OblivionServer.GetUnixTimestamp();

            Playlist = new List<Item>();

            SelectedDiscItem = null;
            Room = room;
        }

        public void LoadList(GameClient session) => Task.Factory.StartNew(() =>
        {
            var room = session.GetHabbo().CurrentRoom;
            var hastrax =
                room.GetRoomItemHandler()
                    .GetFloor.Any(item => item.GetBaseItem().InteractionType == InteractionType.Jukebox);
            if (!hastrax)
                return;

            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.RunQuery("SELECT item_id FROM room_jukebox_songs WHERE user_id = '" + session.GetHabbo().Id +
                              "'");
                _dataTable = adap.getTable();
            }
            foreach (var useritem in (from DataRow row in _dataTable.Rows
                select Convert.ToInt32(row["item_id"].ToString())
                into item
                from useritem in session.GetHabbo().GetInventoryComponent().GetItems
                where item == useritem.Id
                select useritem).Where(useritem => !Playlist.Contains(useritem)))
                Playlist.Add(useritem);
            session.SendMessage(new SetJukeboxNowPlayingComposer(Room));
        });


        public void OnCycle()
        {
            if (IsPlaying)
                if (ActualSongData != SelectedDiscItem)
                {
                    AnteriorItem = SelectedDiscItem;
                    AnteriorMusic = GetMusicByItem(SelectedDiscItem);
                    SelectedDiscItem = ActualSongData;
                    if (SelectedDiscItem == null)
                    {
                        StopPlayList();
                        PlayPlaylist();
                    }
                    Room.SendMessage(new SetJukeboxNowPlayingComposer(Room));
                }
        }

        public void ClearPlayList()
        {
            if (IsPlaying)
                StopPlayList();

            Playlist.Clear();
        }

        public Dictionary<int, Item> GetPlayLine()
        {
            var i = 0;
            var e = new Dictionary<int, Item>();
            foreach (var item in Playlist)
            {
                var music = GetMusicByItem(item);
                if (music == null)
                    continue;
                e.Add(i, item);
                i += music.Length;
            }
            return e;
        }

        public TraxMusicData GetMusicByItem(Item item)
            => item != null ? OblivionServer.GetGame().GetSoundManager().GetMusic(GetExtraData(item)) : null;

        public int GetMusicIndex(Item item)
        {
            for (var i = 0; i < Playlist.Count; i++)
                if (Playlist[i] == item)
                    return i;

            return 0;
        }

        public void PlayPlaylist()
        {
            if (Playlist.Count == 0)
                return;
            StartedPlayTimestamp = (int) OblivionServer.GetUnixTimestamp();
            SelectedDiscItem = null;
            IsPlaying = true;
            Room.SendMessage(new SetJukeboxNowPlayingComposer(Room));
            SetJukeboxesState();
        }

        public void StopPlayList()
        {
            IsPlaying = false;
            StartedPlayTimestamp = 0;
            SelectedDiscItem = null;
            Room.SendMessage(new SetJukeboxNowPlayingComposer(Room));
            SetJukeboxesState();
        }

        public void TriggerPlaylistState()
        {
            if (IsPlaying)
                StopPlayList();
            else
                PlayPlaylist();
        }

        public void SetJukeboxesState()
        {
            foreach (
                var item in
                Room.GetRoomItemHandler()
                    .GetFloor.Where(item => item.GetBaseItem().InteractionType == InteractionType.Jukebox))
            {
                item.ExtraData = IsPlaying ? "1" : "0";
                item.UpdateState();
            }
        }

        public bool AddDisc(Item item, GameClient Session)
        {
            if (item?.GetBaseItem().InteractionType != InteractionType.MusicDisc)
                return false;

            var musicId = GetExtraData(item);
            var music = OblivionServer.GetGame().GetSoundManager().GetMusic(musicId);

            if (music == null)
                return false;

            if (Playlist.Contains(item))
                return false;

            if (IsPlaying)
                return false;

            Task.Factory.StartNew(() =>
            {
                using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    adap.SetQuery("INSERT INTO room_jukebox_songs (user_id, item_id) VALUES (@user, @item)");
                    adap.AddParameter("user", Session.GetHabbo().Id);
                    adap.AddParameter("item", item.Id);
                    adap.RunQuery();
                }

                Playlist.Add(item);
                Room.SendMessage(new SetJukeboxPlayListComposer(Room));
                Room.SendMessage(new LoadJukeboxUserMusicItemsComposer(Session));
            });
            return true;
        }

        public bool RemoveDisc(Item item, GameClient Session)
        {
            if (item == null)
                return false;

            if (IsPlaying)
                return false;

            Task.Factory.StartNew(() =>
            {
                using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    adap.RunQuery("DELETE FROM room_jukebox_songs WHERE item_id = '" + item.Id + "'");
                }
                Playlist.Remove(item);
            });

            Room.SendMessage(new SetJukeboxPlayListComposer(Room));
            Room.SendMessage(new LoadJukeboxUserMusicItemsComposer(Session));

            return true;
        }
        
        public List<Item> GetUserSongs(GameClient Session)
            =>
                Session.GetHabbo().GetInventoryComponent().GetItems.Where(
                        item =>
                            item.GetBaseItem().InteractionType == InteractionType.MusicDisc && !Playlist.Contains(item))
                    .ToList();

        public Item GetDiscItem(int id) => Playlist.FirstOrDefault(item => item.Id == id);
    }
}