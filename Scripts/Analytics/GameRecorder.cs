using System.Collections.Generic;
using MessagePack;
using NS.Core;
using NS.Utility;
using UnityEngine;

namespace NS.Analytics
{
    public class GameRecorder : IAnalyticsRecorder
    {
        // In theory this is all the data needed to replay a game.
        // It may not work across version if we change wrights, distributions, amount of decorations, etc.
        // much lighter than full game model serialization, hence suitable for analytics.
        [MessagePackObject]
        public class ReplayData
        {
            [Key(0)] public string gameVersion = Application.version;
            [Key(1)] public int gameSeed;
            [Key(2)] public List<Vector2Int> placementCellsInOrder = new();
        }

        private ReplayData data;

        public void Start()
        {
            Game.Instance.TilePlaced += OnTilePlaced;

            data = new ReplayData
            {
                gameSeed = Game.Instance.StartingSeed
            };
        }

        private void OnTilePlaced(TilePlacementData placementData)
        {
            data.placementCellsInOrder.Add(placementData.cell);
        }

        public void Stop()
        {
            Game.Instance.TilePlaced -= OnTilePlaced;
        }

        public string Flush()
        {
            string base64 = Utils.ToBase64(data);
            data = null;
            return base64;
        }
    }
}
