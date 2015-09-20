using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenOTTDMapDump
{
  public enum Cheats
  {
    magic_bulldozer = 0,
    switch_company,
    money,
    crossing_tunnels,
    dummy1,
    no_jetcrash,
    dummy2,
    change_date,
    setup_prod,
    dummy3,
    edit_max_hl
  }

  public struct Cheat
  {
    public bool been_used;
    public bool value;

    public override string ToString()
    {
      return string.Format("Used: {0}; Value: {1}", been_used, value);
    }
  }

  public class Game
  {
    Map _m;
    SLVersion _ver;
    Cheat[] _cheats = new Cheat[Enum.GetNames(typeof(Cheats)).Length]; 

    public Game()
    {

    }

    public Cheat[] Cheats
    {
      get
      {
        return _cheats;
      }
    }

    public SLVersion Version
    {
      get
      {
        return _ver;
      }
      set
      {
        _ver = value;
      }
    }

    public void InitMap(uint xSize, uint ySize)
    {
      _m = new Map(xSize, ySize);
    }

    public Map map
    {
      get
      {
        return _m;
      }
    }
  }
}
