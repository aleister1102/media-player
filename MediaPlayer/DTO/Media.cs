using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.DTO
{
    public class Media : ICloneable
    {
        public string FilePath { get; set; } = string.Empty;

        public string Name => Path.GetFileName(FilePath);

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}