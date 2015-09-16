using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Net
{
    public interface ICrawlerRepository
    {
        void Save(string url, string value);
    }
}
