using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfileSample.DAL;

namespace ProfileSample.Business
{
    public interface IImageManager
    {
        Task<IEnumerable<int>> GetIds();
        Task<ImgSource> GetImage(int id);
    }
}
