using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZborMob
{
    public interface IFileService
    {
        void Save(string name, byte[] data);
    }
}
