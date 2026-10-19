using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

namespace NS.Analytics
{
    public class AnalyticsManager
    {
        private const int VERSION = 1;

        private readonly List<IAnalyticsRecorder> recorders = new();
        private readonly string dataPath;
        private readonly string discordUrl;

        private Thread fileThread;
        private UnityWebRequest webRequest;
        private UnityWebRequestAsyncOperation webAsyncOp;
        private bool isDirty;

        public AnalyticsManager()
        {
            dataPath = Application.persistentDataPath;

            Application.quitting += OnApplicationQuitting;

            discordUrl = Resources.Load<TextAsset>("discord_url").text;

            recorders.Add(new ScoreOverTimeRecorder());
            recorders.Add(new GameRecorder());

            Debug.Log("[Analytics] Initialized");
        }

        public void StartRecording()
        {
            foreach (IAnalyticsRecorder recorder in recorders)
            {
                recorder.Start();
            }

            isDirty = true;
        }

        public void StopRecording()
        {
            foreach (IAnalyticsRecorder recorder in recorders)
            {
                recorder.Stop();
            }
        }

        public void Flush(string reason)
        {
            if (recorders.Count == 0)
            {
                Debug.LogWarning("[Analytics] Flushing with no recorders.");
                return;
            }

            if (isDirty == false)
            {
                Debug.Log("[Analytics] Flushing not required");
                return;
            }

            isDirty = false;

            StringBuilder sb = new();
            sb.AppendLine($"{reason}\n");
            foreach (IAnalyticsRecorder recorder in recorders)
            {
                sb.Append(recorder.Flush());
                sb.AppendLine("\n");
            }

            Debug.Log("[Analytics] Flushing analytics");

            string data = sb.ToString();

            WriteToFile(data);
#if !UNITY_EDITOR
            SendToDiscordChannel(data);
#endif
        }

        private void OnApplicationQuitting()
        {
            Application.quitting -= OnApplicationQuitting;

            Flush("Application quitting");

            WaitForFileWriteToFinish();
            WaitForDiscordSendToFinish();
        }

        private void WaitForFileWriteToFinish()
        {
            if (fileThread == null)
            {
                return;
            }

            Debug.Log("[Analytics] Waiting for file write...");

            fileThread.Join();
            fileThread = null;

            Debug.Log("[Analytics] Done waiting for file write");

        }

        private void WaitForDiscordSendToFinish()
        {
            if (webRequest == null || webAsyncOp == null)
            {
                return;
            }

            Debug.Log("[Analytics] Waiting for discord send...");

            while (!webAsyncOp.isDone)
            {
                Thread.Sleep(50);
            }

            webRequest.Dispose();
            webRequest = null;
            webAsyncOp = null;

            Debug.Log("[Analytics] Done waiting for discord send discord");
        }

        private void WriteToFile(string data)
        {
            WaitForFileWriteToFinish();

            fileThread = new Thread(() =>
            {
                Debug.Log("[Analytics] Writing analytics to file...");

                string dir = MakeDir();
                string path = $"{dir}/{MakeFileName()}";

                Directory.CreateDirectory(dir);
                File.WriteAllText(path, data);

                Debug.Log($"[Analytics] Done writing analytics to {path}");
            });

            fileThread.Start();
        }

        // ReSharper disable once UnusedMember.Local
        private void SendToDiscordChannel(string analyticsData)
        {
            if (string.IsNullOrEmpty(discordUrl))
            {
                return;
            }

            Debug.Log("[Analytics] Sending analytics to discord...");

            WaitForDiscordSendToFinish();

            const string contentType = "application/json";

            // must be created on main thread
            string message = DecorateWithVersion(analyticsData);
            string encoded = HttpUtility.JavaScriptStringEncode(message); // escape newlines, etc.
            string discordFormatted = FormatMessageForDiscord(encoded);
            webRequest = UnityWebRequest.Post(discordUrl, discordFormatted, contentType);
            webAsyncOp = webRequest.SendWebRequest();

            webAsyncOp.completed += _ =>
            {
                if (webRequest == null)
                {
                    return;
                }

                Debug.Log($"[Analytics] Done sending analytics to discord. Result: {webRequest.result}, error: {webRequest.error}");
            };
        }

        private string MakeDir()
        {
            return $"{dataPath}/analytics";
        }

        private string MakeFileName()
        {
            string time = DateTime.Now.ToString("yyyy_dd_M_HH_mm_ss");
            return $"{time}.csv";
        }

        private static string DecorateWithVersion(string analyticsData)
        {
            return $"After game summary, version {VERSION}\n```{analyticsData}```";

        }
        private static string FormatMessageForDiscord(string message)
        {
            return $"{{ \"content\": \"{message}\" }}";
        }
    }
}
