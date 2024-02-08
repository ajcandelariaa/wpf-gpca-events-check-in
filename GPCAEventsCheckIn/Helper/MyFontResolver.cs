using PdfSharp.Fonts;

namespace GPCAEventsCheckIn.Helper
{
    public class MyFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var name = familyName.ToLower().TrimEnd('#');

            switch (name)
            {
                case "arial":
                    if (isBold)
                    {
                        if (isItalic)
                            return new FontResolverInfo("Arial#bi");
                        return new FontResolverInfo("Arial#b");
                    }
                    if (isItalic)
                        return new FontResolverInfo("Arial#i");
                    return new FontResolverInfo("Arial#");
            }

            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Arial#":
                    return FontHelper.Arial;

                case "Arial#b":
                    return FontHelper.ArialBold;

                case "Arial#i":
                    return FontHelper.ArialItalic;

                case "Arial#bi":
                    return FontHelper.ArialBoldItalic;
            }

            return null;
        }


        internal static MyFontResolver OurGlobalFontResolver = null;

        internal static void Apply()
        {
            if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
            {
                if (OurGlobalFontResolver == null)
                    OurGlobalFontResolver = new MyFontResolver();

                GlobalFontSettings.FontResolver = OurGlobalFontResolver;
            }
        }
    }
}
