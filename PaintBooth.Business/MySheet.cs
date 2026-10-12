using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Business
{
    public class MySheet : Sheet
    {
        public MySheet(Sheet another, string partName) : base(another)
        {
            CreateTitleBlock(2, Color.White, (float)0.15);
            GetLogo();
            parts = partName;
            //DriveType = Drive;

        }

        public double attributeHeight { get; set; }
        static string DriveType = string.Empty;

        static string parts = string.Empty;
        protected override Entity[] CreateTitleBlock(double borderWidth, Color color, float lineWeight = 0.15F)
        {
            List<Entity> ents = new List<Entity>();
           


            return ents.ToArray();
        }
        public Entity sc;
        public double tableheight(double[] rowsHeights, double prevtableht)
        {
            double tableheight = prevtableht;
            double tableheight1 = 0;
            for (int rowht = 0; rowht < rowsHeights.Length; rowht++)
            {
                if (rowht == 0)
                    tableheight = tableheight + rowsHeights[rowht];
                else
                    tableheight = rowsHeights[rowht];
                tableheight1 = tableheight1 + tableheight;

            }
            return tableheight1;
        }
        public double tablewidth(double[] columnsWidths)
        {
            double tableheight = 0;
            double tableheight1 = 0;
            for (int colwidth = 0; colwidth < columnsWidths.Length; colwidth++)
            {
                tableheight = columnsWidths[colwidth];
                tableheight1 = tableheight1 + tableheight;

            }
            return tableheight1;
        }
      
        protected override Entity[] GetLogo()
        {
            return null;
        }

        public static explicit operator MySheet(DesignDocument v)
        {
            throw new NotImplementedException();
        }
    }
}


