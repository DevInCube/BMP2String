using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMP2String
{

    public static class Ex
    {
        public static bool Is(this Color pix, String hex)
        {
            return pix.Hex().Equals(hex);
        }

        public static String Hex(this Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }

    class Program
    {

        struct ColSym
        {
            public string hex;
            public char sym;

            public ColSym(string hex, char sym)
            {
                this.hex = hex;
                this.sym = sym;
            }
        }

        static List<ColSym> colors = new List<ColSym>();
        static string boundColor;
        static string transparentColor;
        static char transparentSym;
        static char defaultSym;
        static string endString;

        static char ToSymbol(Color pix)
        {
            if ((transparentColor == null && pix.A == 0)
                || (transparentColor != null && pix.Is(transparentColor)))
                return transparentSym;
            else
            {
                foreach (var colsym in colors)
                    if (pix.Is(colsym.hex))
                        return colsym.sym;
                return defaultSym;
            }
        }

        static string FromCrop(Bitmap bmp, int x, int y, int r, int b)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = y; i <= b; i++)
            {
                if (i > y && i < b)
                    sb.Append("'");
                for (var j = x; j <= r; j++)
                {
                    var pix = bmp.GetPixel(j, i);
                    if (pix.Is(boundColor))
                    {
                        bmp.SetPixel(j, i, Color.Transparent);
                    }
                    else
                    {
                        if (j > x && j < r)
                            sb.Append(ToSymbol(pix));
                    }
                }
                if (i > y && i < b)
                {
                    if (i != b - 1)
                        sb.Append("\\n'+");
                    else
                        sb.Append("'" + endString);
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }

        static Program()
        {
            string[] pairs = "#000000:0;#FFFFFF:w".Split(';');
            boundColor = "#FF0101";
            transparentColor = "#000000";
            defaultSym = ' ';
            transparentSym = ' ';
            string src = @"C:\Users\user\Desktop\04b_190.bmp";
            string dest = @"C:\Users\user\Desktop\crops.txt";
        }

        static Dictionary<string, string> ReadConfig(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] props = File.ReadAllLines("config.ini");
            foreach (var prop in props)
            {
                string[] pair = prop.Split('=');
                string name = pair[0].Trim();
                string value = pair[1].Trim();
                dict.Add(name, value);
            }
            return dict;
        }
       
        static void Main(string[] args)
        {
            Dictionary<string, string> dict = ReadConfig("config.ini");
            string[] pairs = dict["colors"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            boundColor = dict["boundColor"];
            transparentColor = dict["transparentColor"];
            defaultSym = dict["defaultSym"].ElementAt(1);
            transparentSym = dict["transparentSym"].ElementAt(1);
            endString = dict["end"];
            string src = dict["src"];
            string dest = dict["dest"];
           
            foreach (var pair in pairs)
            {
                string[] parts = pair.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries);
                colors.Add(new ColSym(parts[0].Trim(), parts[1].Trim().ElementAt(0)));
            }
            
            byte[] bytes = File.ReadAllBytes(src);
            Bitmap bmp = (Bitmap)Bitmap.FromStream(new MemoryStream(bytes));
            List<String> crops = new List<String>();
            for (var i = 0; i < bmp.Height; i++)
            {
                for (var j = 0; j < bmp.Width; j++)
                {
                    var pix = bmp.GetPixel(j, i);
                    if (pix.Is(boundColor))
                    {
                        int x = j;
                        int y = i;
                        int r = j;
                        int b = i;
                        for(r = j; r < bmp.Width; r++)
                        {
                            var pix2 = bmp.GetPixel(r, i);
                            if (!pix2.Is(boundColor)) break;
                        }
                        for (b = i; b < bmp.Height; b++)
                        {
                            var pix2 = bmp.GetPixel(j, b);
                            if (!pix2.Is(boundColor)) break;
                        }
                        crops.Add(FromCrop(bmp, x, y, r-1, b-1));
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (string crop in crops)
                sb.Append(crop + "\r\n");
            File.WriteAllText(dest, sb.ToString());
        }
    }
}
