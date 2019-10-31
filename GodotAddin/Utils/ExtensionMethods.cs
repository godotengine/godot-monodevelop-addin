using System.Collections.Generic;
using System.IO;

namespace GodotAddin.Utils
{
    public static class ExtensionMethods
    {
        public static IEnumerable<string> EnumerateLines(this TextReader textReader)
        {
            string line;
            while ((line = textReader.ReadLine()) != null)
                yield return line;
        }
    }
}
