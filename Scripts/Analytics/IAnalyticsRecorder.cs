namespace NS.Analytics
{
    public interface IAnalyticsRecorder
    {
        public void Start();
        public void Stop();
        public string Flush();
    }
}
