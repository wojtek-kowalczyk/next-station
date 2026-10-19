using System.Collections.Generic;
using MessagePack;
using NS.Core;
using NS.Utility;

namespace NS.Analytics
{
    public class ScoreOverTimeRecorder : IAnalyticsRecorder
    {
        [MessagePackObject]
        public class Data
        {
            [Key(0)] public readonly List<int> scoresAfterPlacement = new();
        }

        private const int MIN_COUNT_TO_REPORT = 1;
        private Data data;

        public void Start()
        {
            Game.Instance.TilePlaced += OnTilePlaced;
            data = new Data();
        }

        public void Stop()
        {
            Game.Instance.TilePlaced -= OnTilePlaced;
        }

        private void OnTilePlaced(TilePlacementData tpd)
        {
            if (tpd.wasPlacedByPlayer == false)
            {
                return;
            }

            data.scoresAfterPlacement.Add(Game.Instance.Model.Points);
        }

        public string Flush()
        {
            if (data.scoresAfterPlacement.Count < MIN_COUNT_TO_REPORT)
            {
                return string.Empty;
            }

            return Utils.ToBase64(data);
        }
    }
}
