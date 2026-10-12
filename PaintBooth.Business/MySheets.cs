using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Business
{
    public class MySheets : Sheet
    {
        public MySheets(linearUnitsType units, double width, double height,
            string name, angleProjectionType projectionType) : base(units, width, height, name, projectionType)
        {

        }

        protected override Entity[] CreateTitleBlock(double borderWidth,
            Color color, float lineWeight = 0.15F)
        {
            const int row = 2, col = 5;
            const double textHeight = 2;
            const string tag = "Rating";

            var entitiesArray = base.CreateTitleBlock(
                borderWidth, color, lineWeight);
            var entities = new List<Entity>(entitiesArray);
            var table = entities.OfType<Table>().FirstOrDefault();

            if (table == null)
                return entities.ToArray();

            table.MergeCells(row, col, row, col + 1);
            table.SetTextString(row, col, tag.ToUpper() + ":");
            table.SetAlignment(row, col, devDept.Eyeshot.Entities.Text.alignmentType.TopLeft);
            entities.Add(new devDept.Eyeshot.Entities.Attribute(
                table.GetCenter(row, col),
                tag,
                string.Empty,
                textHeight)
            {
                Alignment = devDept.Eyeshot.Entities.Text.alignmentType.MiddleCenter
            });

            return entities.ToArray();
        }
    }
}
