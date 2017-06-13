using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ProfileSample.DAL;

namespace ProfileSample.Business
{
    public class ImageManager : IImageManager
    {
        //[LoggingAspect]
        public IEnumerable<int> GetIds()
        {
            var context = new ProfileSampleEntities();
            return context.ImgSources.Take(20).Select(source => source.Id).ToList();
        }

        //[LoggingAspect]
        public ImgSource GetImage(int id)
        {
            var context = new ProfileSampleEntities();
            return context.ImgSources.Find(id);
        }
    }
}