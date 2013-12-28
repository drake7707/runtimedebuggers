using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace RunTimeDebuggers.LocalsDebugger
{
    class ControlIconManager
    {

        private static Dictionary<Type, Image> controlImages = new Dictionary<Type, Image>();

        public static Image GetImage(object c)
        {
            if (c == null)
                throw new ArgumentNullException("c");

            Image bmp;
            
            Type t = c.GetType();

            while (t.Namespace != "System.Windows.Forms" && t.BaseType != null)
                t = t.BaseType;
            if (!controlImages.TryGetValue(t, out bmp))
            {
                if (t.Namespace == "System.Windows.Forms")
                {
                    var stream = t.Assembly.GetManifestResourceStream(t.FullName + ".bmp");
                    if (stream != null)
                    {
                        bmp = Image.FromStream(stream);

                        if (t.Name == "UserControl")
                        {
                            // sigh annoying green is annoying
                            Bitmap edited = new Bitmap(16, 16);
                            using (Graphics g = Graphics.FromImage(edited))
                            {
                                g.Clear(Color.Magenta);
                                ImageAttributes ia = new ImageAttributes();
                                ia.SetColorKey(Color.FromArgb(0, 255, 0), Color.FromArgb(0, 255, 0));
                                g.DrawImage(bmp, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, ia);
                            }
                            bmp.Dispose();
                            bmp = edited;
                        }
                        controlImages[t] = bmp;
                    }
                }
            }

            return bmp;
        }

    }
}
