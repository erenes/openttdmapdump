namespace OpenOTTDMapDump
{
  public class ChunkHandler
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

    public static ChunkHandler FindChunkHandler(string sID)
    {
      switch (sID)
      {
        case "MAPS":
          return new MAPSHandler();
          break;
        case "MAPT":
          return new MAPTHandler();
          break;
        case "MAPH":
          return new MAPHHandler();
          break;
        default:
          return new IgnoreHandler();
          break;
      }
    }

    public virtual void HandleChunk(LZMALoadFilter lf, int sz)
    {

    }
    
  }

  internal class IgnoreHandler : ChunkHandler
  {
    public override void HandleChunk(LZMALoadFilter lf, int sz)
    {
      lf.reader.Seek(sz, System.IO.SeekOrigin.Current);
    }
  }

  internal class MAPSHandler : ChunkHandler
  {
    public override void HandleChunk(LZMALoadFilter lf, int sz)
    {
      Map._map_dim_x = Program.SlReadUint32(lf);
      Map._map_dim_y = Program.SlReadUint32(lf);
      Map.Tiles = new Tile[Map._map_dim_x * Map._map_dim_y];
    }
  }

  internal class MAPTHandler : ChunkHandler
  {
    public override void HandleChunk(LZMALoadFilter lf, int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        Map.Tiles[i].type = Program.SlReadByte(lf);
      }
    }
  }
  internal class MAPHHandler : ChunkHandler
  {
    public override void HandleChunk(LZMALoadFilter lf, int sz)
    {
      for (int i = 0; i < sz; i++)
      {
        Map.Tiles[i].height = Program.SlReadByte(lf);
        if (Map.Tiles[i].height > Map._max_height)
          Map._max_height = Map.Tiles[i].height; 
      }
    }
  }
}