using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenOTTDMapDump
{
  public enum ChunkType
  {
    CH_RIFF = 0,
    CH_ARRAY = 1,
    CH_SPARSE_ARRAY = 2,
    CH_TYPE_MASK = 3,
    CH_LAST = 8, // Last chunk in this array.
    CH_AUTO_LENGTH = 16,
    CH_UNKNOWN = 32
  };

  public class ChunkHandlers
  {
    public delegate void ChunkHandler(int sz);
    private static Dictionary<string, ChunkHandler> _dic = new Dictionary<string, ChunkHandler>(); 

    static ChunkHandlers()
    {
      Type tCHs = typeof(ChunkHandlers);
      MethodInfo[] mis = tCHs.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
      foreach (var mi in mis)
      {
        string sName = mi.Name;
        if (sName.IndexOf("Handle") == 0)
        {
          sName = sName.Substring(6); 
          _dic[sName] = (ChunkHandler)Delegate.CreateDelegate(typeof(ChunkHandler), mi);
        }
      }
    }

    public static ChunkHandler FindChunkHandler(string sID)
    {
      if (_dic.ContainsKey(sID))
        return _dic[sID];
      else
        return HandleIgnore;
    }

    public static void HandleIgnore(int sz)
    {
      SL.IgnoreBytes(sz);
    }
    public static void HandleMAPS(int sz)
    {
      Map._map_dim_x = SL.SlReadUint32();
      Map._map_dim_y = SL.SlReadUint32();
      Map.Tiles = new Tile[Map._map_dim_x * Map._map_dim_y];
    }
    public static void HandleMAPT(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        Map.Tiles[i].type = SL.SlReadByte();
      }
    }

    public static void HandleMAPH(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        Map.Tiles[i].height = SL.SlReadByte();
        if (Map.Tiles[i].height > Map._max_height)
          Map._max_height = Map.Tiles[i].height;
      }
    }
  }
}