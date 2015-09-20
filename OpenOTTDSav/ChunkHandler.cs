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
      uint x = SL.SlReadUint32();
      uint y = SL.SlReadUint32();
      SL.game.InitMap(x, y);
    }
    public static void HandleMAPT(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].type = SL.SlReadByte();
      }
    }

    public static void HandleMAPH(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].height = SL.SlReadByte();
        if (SL.game.map.Tiles[i].height > SL.game.map._max_height)
          SL.game.map._max_height = SL.game.map.Tiles[i].height;
      }
    }

    public static void HandleMAPO(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m1 = SL.SlReadByte();
      }
    }

    public static void HandleMAP2(int sz)
    {
      // Size is in bytes, and we're reading Uint16 (2 bytes) so we should only do half
      sz /= 2;
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m2 = SL.SlReadUint16();
      }
    }

    public static void HandleM3LO(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m3 = SL.SlReadByte();
      }
    }
    public static void HandleM3HI(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m4 = SL.SlReadByte();
      }
    }

    public static void HandleMAP5(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m5 = SL.SlReadByte();
      }
    }

    public static void HandleMAPE(int sz)
    {
      if (SL.IsSavegameVersionBefore(42))
      {
        Console.WriteLine("Support for savegame version {0} is not implemented", SL.game.Version);
      }
      else
      {
        for (int i = 0; i < sz; i++)
        {
          SL.game.map.Tiles[i].m6 = SL.SlReadByte();
        }
      }
    }

    public static void HandleMAP7(int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        SL.game.map.Tiles[i].m6 = SL.SlReadByte();
      }
    }

    public static void HandleDATE(int sz)
    {
      // Date starts counting from 1-1-0000, but mindate is 1-1-0001. So we deduct the leap year 0 from the amount of days to add.
      DateTime gamedate = DateTime.MinValue.AddDays((int)SL.SlReadUint32() -366);
      SL.SlReadUint32(); // 4 bytes of Null
      if (SL.IsSavegameVersionBefore(156)) SL.SlReadUint16();
      if (SL.IsSavegameVersionBefore(161)) SL.SlReadByte();
      if (SL.IsSavegameVersionBefore(45)) SL.SlReadByte();
      uint _cur_tileloop_tile = SL.IsSavegameVersionBefore(5) ? SL.SlReadByte() : SL.SlReadUint32();
      UInt16 _disaster_delay = SL.SlReadUint16(); // _disaster delay
      if (SL.IsSavegameVersionBefore(119)) SL.SlReadUint16();
      uint random_state0 = SL.SlReadUint32(); // _random.state[0]
      uint random_state1 = SL.SlReadUint32(); // _random.state[1]
      if (SL.IsSavegameVersionBefore(9)) SL.SlReadByte();
      if (SL.SlIsObjectValidInSavegame(10, 119)) SL.SlReadUint32();
      byte _cur_company_tick_index= SL.SlReadByte();  // _cur_company_tick_index
      uint _next_competitor_start =  (SL.IsSavegameVersionBefore(108)) ? SL.SlReadUint16() : SL.SlReadUint32(); // _next_competitor_start
      byte _trees_tick_ctr = SL.SlReadByte();  // _trees_tick_ctr
      byte _pause_mode = SL.SlReadByte(); // _pause_mode
      if (SL.SlIsObjectValidInSavegame(11, 119)) SL.SlReadUint32();
    }
    public static void HandleVIEW(int sz)
    {
      uint _saved_scrollpos_x = SL.SlReadUint32();
      uint _saved_scrollpos_y = SL.SlReadUint32();
      byte _saved_scrollpos_zoom = SL.SlReadByte();
    }

    public static void HandleCHTS(int sz)
    {
      int cheats = sz / 2;
      if (cheats > Enum.GetNames(typeof(Cheats)).Length)
      {
        Console.WriteLine("Too many cheats!");
        HandleIgnore(sz);
      }
      else
      {
        for (int i = 0; i < cheats; i++)
        {
          SL.game.Cheats[i].been_used = SL.SlReadByte() != 0;
          SL.game.Cheats[i].value     = SL.SlReadByte() != 0;
        }
      }
    }
    public static void HandlePATS(int sz)
    {
      // This loads all settings. 
      // TODO: Implement. Not really relevant in our case given that we are not actually OpenTTD ;-)
      HandleIgnore(sz);
    }

  }
}