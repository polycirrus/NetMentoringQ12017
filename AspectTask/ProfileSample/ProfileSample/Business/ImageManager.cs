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
        public async Task<IEnumerable<int>> GetIds()
        {
            var context = new ProfileSampleEntities();
            return await context.ImgSources.Take(20).Select(source => source.Id).ToListAsync();
        }

        public async Task<ImgSource> GetImage(int id)
        {
            var context = new ProfileSampleEntities();
            return await context.ImgSources.FindAsync(id);
        }
    }
}