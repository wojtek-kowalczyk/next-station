using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace NS
{
    public class SaveManager
    {
        private const string SAVE_FILE_EXTENSION_NO_DOT = "nssave";
        private const string SAVE_METADATA_FILE_EXTENSION_NO_DOT = "nssavemeta";

        private string savesDir;
        private string SavesDir => savesDir ??= $"{Application.persistentDataPath}/saves";

        public List<GameSaveMetadata> GetSaves()
        {
            List<GameSaveMetadata> results = new();

            if (Directory.Exists(SavesDir) == false)
            {
                return results;
            }

            string[] files = Directory.GetFiles(SavesDir);
            foreach (string file in files)
            {
                if (file.EndsWith($".{SAVE_METADATA_FILE_EXTENSION_NO_DOT}") == false)
                {
                    continue;
                }

                var metadata = ReadAndDeserialize<GameSaveMetadata?>(file);
                if (metadata.HasValue)
                {
                    results.Add(metadata.Value);
                }
            }

            return results;
        }

        public GameSaveMetadata? GetLastSaveMetadata()
        {
            List<GameSaveMetadata> saves = GetSaves();
            if (saves.Count == 0)
            {
                return null;
            }

            // newest save file
            return saves.OrderByDescending(x => x.saveWriteTime).FirstOrDefault();
        }

        public GameSave? LoadSaveFile(GameSaveMetadata metadata)
        {
            return ReadAndDeserialize<GameSave?>($"{SavesDir}/{metadata.saveFileNameExt}");
        }

        public bool TryWriteSaveFile(string saveName, GameSave save, GameInfo gameInfo)
        {
            DateTime now = DateTime.Now;

            string saveFilenameNoExt = SanitizeSaveName(saveName);
            string saveFileNameExt = $"{saveFilenameNoExt}.{SAVE_FILE_EXTENSION_NO_DOT}";
            string saveMetadataFileName = $"{saveFilenameNoExt}.{SAVE_METADATA_FILE_EXTENSION_NO_DOT}";

            string saveFilePath = Path.Combine(SavesDir, saveFileNameExt);
            string saveMetadataFilePath = Path.Combine(SavesDir, saveMetadataFileName);

            bool success = true;

            success &= TrySerializeAndWrite(saveFilePath, save);

            if (success)
            {
                var metadata = new GameSaveMetadata(now, saveFileNameExt, "TODO", gameInfo);
                success &= TrySerializeAndWrite(saveMetadataFilePath, metadata);

                if (!success)
                {
                    Debug.LogError("[SaveManager] Failed to write metadata, save file cannot exist on its own, deleting...");
                    File.Delete(saveFilePath);
                }
            }

            if (success)
            {
                Debug.Log($"[SaveManager] Save written to: {saveFilePath}");
                Debug.Log($"[SaveManager] Save metadata written to: {saveMetadataFilePath}");
            }

            return success;
        }

        public static string SanitizeSaveName(string saveName)
        {
            return saveName.Split(".")[0]; // drop extension, if any
        }

        private string GetSaveFileNameNoExt()
        {
            return Guid.NewGuid().ToString();
        }

        private static T ReadAndDeserialize<T>(string filePath)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                return MessagePackSerializer.Deserialize<T>(bytes);
            }
            catch (MessagePackSerializationException e)
            {
                Debug.LogError("[SaveManager] Failed to deserialize save metadata, see exception below");
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveManager] Failed to load save metadata, see exception below");
                Debug.LogException(e);
            }

            return default;
        }

        private static bool TrySerializeAndWrite<T>(string filePath, T obj)
        {
            try
            {
                byte[] bytes = MessagePackSerializer.Serialize(obj);
                string dir = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(dir!);
                File.WriteAllBytes(filePath, bytes);
                return true;
            }
            catch (MessagePackSerializationException e)
            {
                Debug.LogError("[SaveManager] Failed to deserialize save metadata, see exception below");
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveManager] Failed to load save metadata, see exception below");
                Debug.LogException(e);
            }

            return false;
        }

        public bool TryDeleteSaveFile(GameSaveMetadata saveMetadata)
        {
            string saveFileNameExt = saveMetadata.saveFileNameExt;
            string saveFileNameNoExt = saveFileNameExt.Split(".")[0];
            string saveMetadataFileName = $"{saveFileNameNoExt}.{SAVE_METADATA_FILE_EXTENSION_NO_DOT}";

            string saveFilePath = Path.Combine(SavesDir, saveFileNameExt);
            string saveMetadataFilePath = Path.Combine(SavesDir, saveMetadataFileName);

            try
            {
                File.Delete(saveFilePath);
                File.Delete(saveMetadataFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveManager] Error deleting save file. See exception below");
                Debug.LogException(e);
                return false;
            }

            return true;
        }
    }
}
