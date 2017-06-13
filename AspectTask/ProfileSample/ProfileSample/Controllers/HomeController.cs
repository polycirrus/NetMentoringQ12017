using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ProfileSample.Business;
using ProfileSample.DAL;
using ProfileSample.Models;

namespace ProfileSample.Controllers
{
    public class HomeController : Controller
    {
        private IImageManager imageManager;

        public HomeController(IImageManager imageManager)
        {
            this.imageManager = imageManager;
        }

        public ActionResult Index()
        {
            var imageIds = imageManager.GetIds();

            return View(imageIds);
        }

        public ActionResult GetImage(int id)
        {
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
            HttpContext.Response.Cache.SetMaxAge(new TimeSpan(1, 0, 0));

            string rawIfModifiedSince = HttpContext.Request.Headers.Get("If-Modified-Since");
            if (string.IsNullOrEmpty(rawIfModifiedSince))
            {
                // Set Last Modified time
                HttpContext.Response.Cache.SetLastModified(DateTime.Now.AddYears(-1));

                var image = imageManager.GetImage(id);
                if (image == null)
                    return HttpNotFound();
                return File(image.Data, "image/jpg");
            }

            return new HttpStatusCodeResult(304, "Not Modified");
        }

        public ActionResult Convert()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Content/Img"), "*.jpg");

            using (var context = new ProfileSampleEntities())
            {
                foreach (var file in files)
                {
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        byte[] buff = new byte[stream.Length];

                        stream.Read(buff, 0, (int) stream.Length);

                        var entity = new ImgSource()
                        {
                            Name = Path.GetFileName(file),
                            Data = buff,
                        };

                        context.ImgSources.Add(entity);
                        context.SaveChanges();
                    }
                } 
            }

            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}