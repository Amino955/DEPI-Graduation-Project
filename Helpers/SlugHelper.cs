namespace TellaStore.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var arabicMap = new Dictionary<char, string>
        {
            {'ا',"a"},{'أ',"a"},{'إ',"e"},{'آ',"a"},{'ب',"b"},{'ت',"t"},{'ث',"th"},
            {'ج',"g"},{'ح',"h"},{'خ',"kh"},{'د',"d"},{'ذ',"z"},{'ر',"r"},{'ز',"z"},
            {'س',"s"},{'ش',"sh"},{'ص',"s"},{'ض',"d"},{'ط',"t"},{'ظ',"z"},{'ع',"a"},
            {'غ',"gh"},{'ف',"f"},{'ق',"q"},{'ك',"k"},{'ل',"l"},{'م',"m"},{'ن',"n"},
            {'ه',"h"},{'و',"w"},{'ي',"y"},{'ى',"a"},{'ة',"h"},{'ء',""},{'ئ',"y"},
            {'ؤ',"w"},{'ً',"-"},{'ٌ',"-"},{'ٍ',"-"},{'َ',""},{'ُ',""},
            {'ِ',""},{'ّ',""},{'ْ',""}
        };

        var result = new System.Text.StringBuilder();
        foreach (var c in text.ToLower())
        {
            if (arabicMap.TryGetValue(c, out var replacement))
                result.Append(replacement);
            else if (char.IsLetterOrDigit(c))
                result.Append(c);
            else if (c == ' ' || c == '-' || c == '_')
                result.Append('-');
        }

        var slug = result.ToString().Trim('-');
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug;
    }
}
