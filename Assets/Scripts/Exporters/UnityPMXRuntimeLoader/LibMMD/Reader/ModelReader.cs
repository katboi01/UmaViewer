using System.IO;
using LibMMD.Model;

namespace LibMMD.Reader
{
    public abstract class ModelReader
    {      
        public RawMMDModel Read(string path, ModelConfig config)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                using (var bufferedStream = new BufferedStream(fileStream)) {
                    using (var binaryReader = new BinaryReader(bufferedStream))
                    {
                        return Read(binaryReader, config);
                    }
                }
            }
        }

        public abstract RawMMDModel Read(BinaryReader reader, ModelConfig config);
        
        public static RawMMDModel LoadMmdModel(string path, ModelConfig config)
        {
            var fileExt = new FileInfo(path).Extension.ToLower();
            if (".pmd".Equals(fileExt))
            {
                return new PMDReader().Read(path, config);
            }
            if (".pmx".Equals(fileExt))
            {
                return new PMXReader().Read(path, config);
            }
            throw new MMDFileParseException("File " + path +
                                            " is not a MMD model file. File name should ends with \"pmd\" or \"pmx\".");
        }

    }
}