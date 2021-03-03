using CDTSharp.Core;

namespace CDTSharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = CDTReader.ReadCDTFile(args[0]);
        }
    }
}
