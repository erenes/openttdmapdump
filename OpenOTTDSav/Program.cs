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
    struct SLVersion
    {
      public UInt16 major;
      public byte minor;

      public override string ToString()
      {
        return string.Format("{0}.{1}", major, minor);
      }
    }

    enum SLFormat
    {
      /// <summary>
      /// LZO, Roughly 75% larger than zlib level 6 at only ~7% of the CPU usage
      /// </summary>
      OTTD = 1330926660,
      /// <summary>
      /// None, Roughly 5 times larger at only 1% of the CPU usage over zlib level 6.
      /// </summary>
      OTTN = 1330926670,
      /// <summary>
      /// Zlib, After level 6 the speed reduction is significant (1.5x to 2.5x slower per level), but the reduction in filesize is
      ///  fairly insignificant (~1% for each step). Lower levels become ~5-10% bigger by each level than level 6 while level
      ///  1 is "only" 3 times as fast.Level 0 results in uncompressed savegames at about 8 times the cost of "none"
      /// </summary>
      OTTZ = 1330926682,
      /// <summary>LZMA, Level 2 compression is speed wise as fast as zlib level 6 compression (old default), but results in ~10% smaller saves.
      /// Higher compression levels are possible, and might improve savegame size by up to 25%, but are also up to 10 times slower.
      /// The next significant reduction in file size is at level 4, but that is already 4 times slower. Level 3 is primarily 50%
      /// slower while not improving the filesize, while level 0 and 1 are faster, but don't reduce savegame size much.
      /// It's OTTX and not e.g. OTTL because liblzma is part of xz-utils and .tar.xz is preferred over .tar.lzma. </summary>
      OTTX = 1330926680
    }

    static void Main(string[] args)
    {
      string sFile = @"C:\Users\e.renes\Documents\OpenTTD\save\test3.sav";
      DoLoad(sFile);
      Map.ToBitmap(sFile.Replace(".sav", ".png"));
    }

    private static void DoLoad(string sFileName)
    {
      if (!File.Exists(sFileName)) return;
      Stream s = File.OpenRead(sFileName);

      byte[] buf = new byte[8]; // uint32 * 2 
      s.Read(buf, 0, buf.GetLength(0));
      UInt32[] hdr = new UInt32[2];
      hdr[0] = SwapBytes(BitConverter.ToUInt32(buf, 0));
      hdr[1] = SwapBytes(BitConverter.ToUInt32(buf, 4));

      SLVersion ver;
      ver.major = (UInt16)(hdr[1] >> 16);
      ver.minor = (byte)((hdr[1] >> 8) & 0xFF);

      Console.WriteLine("Loading savegame version {0}", ver);

      switch((SLFormat)hdr[0])
      {
        case SLFormat.OTTX:
          LZMALoadFilter lf = new LZMALoadFilter(s);
          LoadChunks(lf);
          break;
        default:
          // Not implemented. 
          break;
      }

    }

    private static void LoadChunks(LZMALoadFilter lf)
    {
      uint id;
      ChunkHandler ch; 

      for(id = SlReadUint32(lf); id != 0; id = SlReadUint32(lf))
      {
        string chunkId = new string(new char[] { (char)(id >> 24), (char)((id >> 16) & 0xFF), (char)((id >> 8) & 0xFF), (char)(id & 0xFF) });
        Console.WriteLine("Loading chunk {0}", chunkId);
        ch = ChunkHandler.FindChunkHandler(chunkId);
        if (ch is IgnoreHandler) Console.WriteLine("Unknown chunk type");
        SlLoadChunk(ch, lf);
      }

    }

    private static void SlLoadChunk(ChunkHandler ch, LZMALoadFilter lf)
    {
      byte m = SlReadByte(lf);
      int len;

      switch ((ChunkHandler.ChunkType)m)
      {
        case ChunkHandler.ChunkType.CH_ARRAY:
        case ChunkHandler.ChunkType.CH_SPARSE_ARRAY:
          // unknown; 
          break;
        default: 
          if ((((int)m) & 0xF) == (int) ChunkHandler.ChunkType.CH_RIFF)
          {
            len = (SlReadByte(lf) << 16) | ((m >> 4) << 24);
            len += SlReadUint16(lf);

            ch.HandleChunk(lf, len);

          }
          else
          {
            Console.WriteLine("Invalid chunk type");
          }
          break;
      }

    }
    

    public static uint SwapBytes(uint word)
    {
      return ((word >> 24) & 0x000000FF) | ((word >> 8) & 0x0000FF00) | ((word << 8) & 0x00FF0000) | ((word << 24) & 0xFF000000);
    }

    public static UInt64 SlReadUint64(LZMALoadFilter lf)
    {
      uint x = SlReadUint32(lf);
      uint y = SlReadUint32(lf);
      return (UInt64)x << 32 | y;
    }

    public static uint SlReadUint32(LZMALoadFilter lf)
    {
      uint x = (uint) SlReadUint16(lf) << 16;
      return (uint)x | SlReadUint16(lf);
    }

    public static UInt16 SlReadUint16(LZMALoadFilter lf)
    {
      UInt16 x = (UInt16)(SlReadByte(lf) << 8);
      return (UInt16) (x | SlReadByte(lf));
    }

    public static byte SlReadByte(LZMALoadFilter lf)
    {
      return (byte)lf.reader.ReadByte();
    }
  }
}
