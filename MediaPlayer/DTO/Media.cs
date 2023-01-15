using System.IO;

namespace MediaPlayer.DTO
{
    public class Media
    {
        public string FilePath { get; set; } = string.Empty;

        public string Name => Path.GetFileName(FilePath);
    }
}