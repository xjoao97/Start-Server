﻿namespace Oblivion.HabboHotel.Moderation
{
    internal class ModerationPresetActionCategories
    {
        public ModerationPresetActionCategories(int Id, string Caption)
        {
            this.Id = Id;
            this.Caption = Caption;
        }

        public int Id { get; set; }
        public string Caption { get; set; }
    }
}