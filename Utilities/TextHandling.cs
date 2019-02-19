namespace Oblivion.Utilities
{
    public class TextHandling
    {
        public static string GetString(double k) => k.ToString(OblivionServer.CultureInfo);
    }
}