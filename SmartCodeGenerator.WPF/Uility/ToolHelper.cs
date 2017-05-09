using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCodeGenerator.Uility
{
    public class ToolHelper
    {
        public  static string GetHighlightingByExtension(string ext)
        {
            switch (ext.Trim().ToLower())
            {
                case "cs":
                case ".cs":
                    return "C#";
                case "vb":
                case ".vb":
                    return "VB";
                case "xml":
                case ".xml":
                case "xaml":
                    return "XML";
            }
            return "XML";
        }
    }
}
