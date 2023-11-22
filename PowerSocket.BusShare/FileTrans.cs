using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket
{
    internal class FileTrans
    {
        public byte[] Body { get; set; }

        public string FileName { get; set; }

        public long FileLength { get; set; }

        public string FilePath { get; set; }
    }
}
