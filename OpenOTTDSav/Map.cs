using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenOTTDMapDump
{
  /**
 * Data that is stored per tile. Also used TileExtended for this.
 * Look at docs/landscape.html for the exact meaning of the members.
 */
  public struct Tile
  {
    public byte type;        //< The type (bits 4..7), bridges (2..3), rainforest/desert (0..1)
    public byte height;      //< The height of the northern corner.
    public UInt16 m2;        //< Primarily used for indices to towns, industries and stations
    public byte m1;          //< Primarily used for ownership information
    public byte m3;          //< General purpose
    public byte m4;          //< General purpose
    public byte m5;          //< General purpose
  };

  public static class Map
  {
    internal static uint _map_dim_x;
    internal static uint _map_dim_y;
    internal static int _max_height; 
    internal static Tile[] Tiles;

    internal static void ToBitmap(string sPath)
    {
      Bitmap b = new Bitmap((int)_map_dim_x, (int)_map_dim_y);
      Color[] colors = new Color[_max_height + 1];
      int iStep = 0xFF / colors.Length;
      for (int i = 0; i < colors.Length; i++)
      {
        int iCurrent = i * iStep;
        colors[i] = Color.FromArgb(iCurrent, iCurrent, iCurrent);
      }

      for (int i = 0; i < Tiles.Length; i++)
      {
        b.SetPixel((int) (i / _map_dim_x), (int) (i % _map_dim_y), colors[Tiles[i].height]);
      }

      b.Save(sPath);
    }
  }
}
