﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

#endregion

namespace Oblivion.HabboHotel.Items.Data.Moodlight
{
    public class MoodlightData
    {
        public int CurrentPreset;
        public bool Enabled;
        public int ItemId;

        public List<MoodlightPreset> Presets;

        public MoodlightData(int ItemId)
        {
            this.ItemId = ItemId;

            DataRow Row = null;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM room_items_moodlight WHERE item_id = '" +
                    ItemId + "' LIMIT 1");
                Row = dbClient.GetRow();
            }

            if (Row == null)
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery(
                        "INSERT INTO `room_items_moodlight` (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES (" +
                        ItemId + ",0,1,'#000000,255,0','#000000,255,0','#000000,255,0')");
                    dbClient.SetQuery(
                        "SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM room_items_moodlight WHERE item_id=" +
                        ItemId + " LIMIT 1");
                    Row = dbClient.GetRow();
                }

            Enabled = OblivionServer.EnumToBool(Row["enabled"].ToString());
            CurrentPreset = Convert.ToInt32(Row["current_preset"]);
            Presets = new List<MoodlightPreset>();

            Presets.Add(GeneratePreset(Convert.ToString(Row["preset_one"])));
            Presets.Add(GeneratePreset(Convert.ToString(Row["preset_two"])));
            Presets.Add(GeneratePreset(Convert.ToString(Row["preset_three"])));
        }

        public void Enable()
        {
            Enabled = true;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE room_items_moodlight SET enabled = 1 WHERE item_id = '" + ItemId + "' LIMIT 1");
            }
        }

        public void Disable()
        {
            Enabled = false;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE room_items_moodlight SET enabled = 0 WHERE item_id = '" + ItemId + "' LIMIT 1");
            }
        }

        public void UpdatePreset(int Preset, string Color, int Intensity, bool BgOnly, bool Hax = false)
        {
            if (!IsValidColor(Color) || !IsValidIntensity(Intensity) && !Hax)
                return;

            string Pr;

            switch (Preset)
            {
                case 3:

                    Pr = "three";
                    break;

                case 2:

                    Pr = "two";
                    break;

                case 1:
                default:

                    Pr = "one";
                    break;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE room_items_moodlight SET preset_" + Pr + " = '@color," + Intensity + "," +
                                  OblivionServer.BoolToEnum(BgOnly) + "' WHERE item_id = '" + ItemId + "' LIMIT 1");
                dbClient.AddParameter("color", Color);
                dbClient.RunQuery();
            }

            GetPreset(Preset).ColorCode = Color;
            GetPreset(Preset).ColorIntensity = Intensity;
            GetPreset(Preset).BackgroundOnly = BgOnly;
        }

        public static MoodlightPreset GeneratePreset(string Data)
        {
            var Bits = Data.Split(',');

            if (!IsValidColor(Bits[0]))
                Bits[0] = "#000000";

            return new MoodlightPreset(Bits[0], int.Parse(Bits[1]), OblivionServer.EnumToBool(Bits[2]));
        }

        public MoodlightPreset GetPreset(int i)
        {
            i--;

            if (Presets[i] != null)
                return Presets[i];

            return new MoodlightPreset("#000000", 255, false);
        }

        public static bool IsValidColor(string ColorCode)
        {
            switch (ColorCode)
            {
                case "#000000":
                case "#0053F7":
                case "#EA4532":
                case "#82F349":
                case "#74F5F5":
                case "#E759DE":
                case "#F2F851":

                    return true;

                default:

                    return false;
            }
        }

        public static bool IsValidIntensity(int Intensity)
        {
            if (Intensity < 0 || Intensity > 255)
                return false;

            return true;
        }

        public string GenerateExtraData()
        {
            var Preset = GetPreset(CurrentPreset);
            var SB = new StringBuilder();

            SB.Append(Enabled ? 2 : 1);

            SB.Append(",");
            SB.Append(CurrentPreset);
            SB.Append(",");

            SB.Append(Preset.BackgroundOnly ? 2 : 1);

            SB.Append(",");
            SB.Append(Preset.ColorCode);
            SB.Append(",");
            SB.Append(Preset.ColorIntensity);
            return SB.ToString();
        }
    }
}