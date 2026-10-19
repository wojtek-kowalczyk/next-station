using System;
using System.Text;
using MessagePack;
using NS.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace NS.Core.Model
{
    public class GameValidator
    {
        public void ValidateSerialization(byte[] oldModel, GameModel model, TilePlacementData placementData)
        {
            var bytes = MessagePackSerializer.Serialize(model);
            var deserialized = MessagePackSerializer.Deserialize<GameModel>(bytes);

            try
            {
                Assert.IsTrue(deserialized != null);
                Assert.IsTrue(model.Equals(deserialized));
            }
            catch (AssertionException)
            {
                string tilePlacementDataBase64 = Utils.ToBase64(placementData);
                string lastValidModelBase64 = Convert.ToBase64String(oldModel);

                StringBuilder sb = new();
                sb.Append("[GameValidator] ");
                sb.AppendLine("Model serialization test failed. This model didn't serialize properly.");
                sb.AppendLine($"Model state before failure, in base64 encoding:\n\n{lastValidModelBase64}\n");
                sb.AppendLine($"Tile placement data that caused invalid state:\n\n{tilePlacementDataBase64}\n");
                sb.AppendLine("It is advised to use this data to construct an automated test case.");
                sb.AppendLine("Rethrowing...");

                Debug.LogError(sb.ToString());

                throw;
            }
        }

        public void ValidateUndo(Game game, TilePlacementData placementData)
        {
            byte[] model = MessagePackSerializer.Serialize(game.Model);

            if (game.TryUndoTilePlacement(silent: true) == false)
            {
                return;
            }

            bool success = game.TryRedoTilePlacement(silent: true);
            Assert.IsTrue(success);
            var deserialized = MessagePackSerializer.Deserialize<GameModel>(model);

            try
            {
                Assert.IsTrue(deserialized.Equals(game.Model));
            }
            catch (AssertionException)
            {
                string startedWith = Convert.ToBase64String(model);
                string finishedWith = Utils.ToBase64(deserialized);
                string placementThatWasUndone = Utils.ToBase64(placementData);

                StringBuilder sb = new();
                sb.Append("[GameValidator] ");
                sb.AppendLine("Model undo/redo failed. Redone model is different.");
                sb.AppendLine($"Started with:\n\n{startedWith}\n");
                sb.AppendLine($"Finished with:\n\n{finishedWith}\n");
                sb.AppendLine($"Placement that was undone and redone:\n\n{placementThatWasUndone}\n");
                sb.AppendLine("It is advised to use this data to construct an automated test case.");

                Debug.LogError(sb.ToString());

                const string info = "The game detected invalid state. Please send the log file to the developers.";
                Utils.ReportCriticalFailure(info, false);

                throw;
            }
        }
    }
}
