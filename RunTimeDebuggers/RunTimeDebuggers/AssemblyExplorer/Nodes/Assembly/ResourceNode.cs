using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;
using System.IO;
using System.Drawing;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ResourceNode : AbstractAssemblyNode
    {
        private string resource;
        private Assembly ass;

        public ResourceNode(Assembly ass, string resource)
        {
            this.ass = ass;
            this.resource = resource;

            UpdateText(false);
            int iconIdx = (int)IconEnum.Resource;
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
        }

        public override void UpdateText(bool recursive)
        {
            Text = resource;
            this.StatusText = resource;
        }

        public override string Visualization
        {
            get
            {
                //if (IsImage())
                //{
                //    using (Stream s = ass.GetManifestResourceStream(resource))
                //    {
                //        return MethodNode.RTFHeader.Replace("@BODY@", GetImage(s, 100, 100));
                //    }
                //}
                return this.Text;
            }
        }

        public Stream GetStream()
        {
            return ass.GetManifestResourceStream(resource);
        }

        //public string GetImage(Stream s, int width, int height)
        //{
        //    Image img = Bitmap.FromStream(s);

        //    using (Bitmap bmp = new Bitmap(img))
        //    {

        //        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
        //        System.Drawing.Imaging.ImageLockMode.ReadOnly,
        //        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //        byte[] bytes = new byte[data.Stride * data.Height];
        //        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, data.Stride * data.Height);
        //        bmp.UnlockBits(data);

        //        string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty);

        //        string mpic = @"{\pict" + @"\" + "wbitmap0" + @"\picw" +
        //            img.Width.ToString() + @"\pich" + img.Height.ToString() +
        //            @"\picwgoal" + width.ToString() + @"\pichgoal" + height.ToString() +
        //            @"\hex " + str + "}";

        //        img.Dispose();
        //        return mpic;
        //    }
        //}


        private bool IsImage()
        {
            return resource.EndsWith(".bmp") || resource.EndsWith(".png") || resource.EndsWith("gif");
        }


        public string ResourceName { get { return resource; } }
    }
}
