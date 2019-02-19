namespace Oblivion.HabboHotel.Moderation
{
    internal class ModerationPreset
    {
        public ModerationPreset(int Id, string Type, string Message)
        {
            this.Id = Id;
            this.Type = Type;
            this.Message = Message;
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}