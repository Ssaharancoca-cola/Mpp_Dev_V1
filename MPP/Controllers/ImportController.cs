using DAL.Common;
using Microsoft.AspNetCore.Mvc;
using Model;
using MPP.Filter;
using MPP.ViewModel;
using System.Data;
using System.Text;

namespace MPP.Controllers
{
    [SessionTimeoutDimension]
    [SessionTimeoutEntity]
    public class ImportController: Controller
    {
        public ActionResult Index() { return View(); }

        
    }
}
