using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bullows.Business
{
    public class MyDrawing: devDept.Eyeshot.DrawingDocument
    {

        //public D Helper { get; private set; }

        /// <summary>
        /// Indicate whether the original scene is modified or not.
        /// </summary>
        public bool IsModified = true;

        /// <summary>
        /// Indicate if the current scene has been imported or not.
        /// </summary>
        /// <remarks>When imported, some buttons of the sample are disabled.</remarks>
        public bool IsImported = false;

        /// <summary>
        /// Indicate if the current drawings must be reloaded.
        /// </summary>
        public bool IsToReload = true;

        public MyDrawing()
        {
           // Helper = new DrawingHelper(this);
        }
    }
}
