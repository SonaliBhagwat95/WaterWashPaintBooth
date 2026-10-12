using PaintBooth.Database;
using PaintBooth.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Repositories.Repositories
{
    public class ExceptionRepository(AppDbContext _db): IExceptionRepository
    {
        public void SaveException(string Classname, string methodName, string Error,int userId)
        {
            ExceptionHandler obj = new ExceptionHandler();
            obj.Classname = Classname;
            obj.Methodname = methodName;
            obj.Error = Error;
            obj.CreatedDate = DateTime.Now;
            obj.UserId = userId;
            _db.ExceptionHandler.Add(obj);
           _db.SaveChanges();
           
        }
    }
}
