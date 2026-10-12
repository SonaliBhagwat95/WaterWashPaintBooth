using PaintBooth.Database;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Repositories.Interface
{
    public interface IExceptionRepository
    {
        public void SaveException(string Classname, string methodName, string Error, int userId);
    }
}
