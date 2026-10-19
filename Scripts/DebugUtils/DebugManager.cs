using NS.Core;

namespace NS.DebugUtils
{
    public class DebugState
    {
        public bool overlayVisible = true;
        public bool decorationsVisible = true;
    }

    public class DebugManager : LocalSingleton<DebugManager>
    {
        public DebugState State { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            State = new DebugState();
        }
    }
}
