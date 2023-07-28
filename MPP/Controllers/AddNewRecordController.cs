using Microsoft.AspNetCore.Mvc;
using MPP.Filter;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    [SessionTimeoutEntity]
    public class AddNewRecordController : IDisposable
    {
        void IDisposable.Dispose()
        {
            
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SaveRecord(FormCollection form, string Command)
        {
            try
            {
                if(Command =="Save")
                {
                    int entityTypeId = Convert.ToInt32(Session["EntityTypeID"]);
                }
            }
        }
    }
}
