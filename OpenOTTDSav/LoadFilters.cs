using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XZ.NET;

namespace OpenOTTDMapDump
{
  public class LZMALoadFilter
  {
    byte[] fread_buf = new byte[131072]; // Buffer for reading from the file
    public MemoryStream reader = new MemoryStream();
    public LZMALoadFilter(Stream s)
    {
      XZInputStream xz = new XZInputStream(s);
      int i = fread_buf.Length;
      while (i == fread_buf.Length)
      {
        i = xz.Read(fread_buf, 0, fread_buf.Length);
        reader.Write(fread_buf, 0, i);
      }
      reader.Seek(0, SeekOrigin.Begin);
    }
  }
}
