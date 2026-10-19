using System;
using JetBrains.Annotations;

namespace NS.Core
{
    public interface ITileStack
    {
        int Count { get; }

        [CanBeNull]
        HexTileData PeekTile();

        [CanBeNull]
        HexTileData PopTile();

        Span<HexTileData> PeekTiles();

        void AddTiles(int count);

        void PushNextTile(HexTileData tile);
    }
}
