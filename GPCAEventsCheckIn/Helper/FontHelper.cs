using System.IO;
using System.Reflection;

namespace GPCAEventsCheckIn.Helper
{
    public static class FontHelper
    {
        public static byte[] Arial
        {
            get { return LoadFontData("GpcaEventsCheckIn.Assets.Fonts.Arial.arial.ttf"); }
        }

        public static byte[] ArialBold
        {
            get { return LoadFontData("GpcaEventsCheckIn.Assets.Fonts.Arial.arialbd.ttf"); }
        }

        public static byte[] ArialItalic
        {
            get { return LoadFontData("GpcaEventsCheckIn.Assets.Fonts.Arial.ariali.ttf"); }
        }

        public static byte[] ArialBoldItalic
        {
            get { return LoadFontData("GpcaEventsCheckIn.Assets.Fonts.Arial.arialbi.ttf"); }
        }

        static byte[] LoadFontData(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Test code to find the names of embedded fonts
            var ourResources = assembly.GetManifestResourceNames();

            Console.WriteLine(ourResources);

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }
}
