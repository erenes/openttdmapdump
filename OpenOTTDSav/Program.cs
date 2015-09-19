using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenOTTDMapDump
{
  class Program
  {

    static void Main(string[] args)
    {
      /*if (args.Length == 0)
        return;
      string sFile = args[0];*/
      string sFile = @"C:\Users\e.renes\Documents\OpenTTD\save\test3.sav";
      SL.DoLoad(sFile);
      Map.ToBitmap(sFile.Replace(".sav", ".png"));

      Console.ReadKey();
    }

  }
}
