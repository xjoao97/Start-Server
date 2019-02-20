namespace Oblivion.HabboHotel.Catalog.Vouchers
{
    public class Voucher
    {
        public Voucher(string Code, string Type, int Value, int CurrentUses, int MaxUses)
        {
            this.Code = Code;
            this.Type = VoucherUtility.GetType(Type);
            this.Value = Value;
            this.CurrentUses = CurrentUses;
            this.MaxUses = MaxUses;
        }

        public string Code { get; set; }

        public VoucherType Type { get; set; }

        public int Value { get; set; }

        public int CurrentUses { get; set; }

        public int MaxUses { get; set; }

        public void UpdateUses()
        {
            CurrentUses += 1;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery(
                    "UPDATE `catalog_vouchers` SET `current_uses` = `current_uses` + '1' WHERE `voucher` = '" + Code +
                    "' LIMIT 1");
            }
        }
    }
}