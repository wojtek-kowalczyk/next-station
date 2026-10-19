using System;
using System.Text;
using MessagePack;

namespace NS
{
    [MessagePackObject]
    public readonly struct GameSave
    {
        [Key(0)] public readonly byte[] serializedModel;

        public GameSave(byte[] serializedModel)
        {
            this.serializedModel = serializedModel;
        }
    }

    [MessagePackObject]
    public readonly struct GameSaveMetadata
    {
        [Key(0)] public readonly DateTime saveWriteTime;
        [Key(1)] public readonly string saveFileNameExt;
        [Key(2)] public readonly string screenshotFileExt;
        [Key(3)] public readonly GameInfo gameInfo;

        public GameSaveMetadata(DateTime saveWriteTime, string saveFileNameExt, string screenshotFileExt, GameInfo gameInfo)
        {
            this.saveWriteTime = saveWriteTime;
            this.screenshotFileExt = screenshotFileExt;
            this.gameInfo = gameInfo;
            this.saveFileNameExt = saveFileNameExt;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Save metadata:");
            sb.AppendLine($"time: {saveWriteTime:yyyy-MM-dd--hh-mm-ss}");
            sb.AppendLine($"save file: {saveFileNameExt}");
            sb.AppendLine($"screenshotFile: {screenshotFileExt}");
            sb.AppendLine($"score: {gameInfo.score}");
            sb.AppendLine($"tilesLeft: {gameInfo.tilesLeft}");
            return sb.ToString();
        }
    }

    [MessagePackObject]
    public readonly struct GameInfo
    {
        [Key(0)] public readonly int score;
        [Key(1)] public readonly int tilesLeft;

        public GameInfo(int score, int tilesLeft)
        {
            this.score = score;
            this.tilesLeft = tilesLeft;
        }
    }
}
