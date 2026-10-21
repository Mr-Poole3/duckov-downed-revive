namespace DuckovDownedRevive.Core
{
    public sealed class HostConfigSession
    {
        private DownedReviveConfig _pending;
        private DownedReviveConfig _locked;

        public HostConfigSession()
        {
            _pending = DownedReviveConfig.CreateDefault();
        }

        public DownedReviveConfig Pending
        {
            get { return _pending.Clone(); }
        }

        public DownedReviveConfig Locked
        {
            get { return _locked == null ? null : _locked.Clone(); }
        }

        public bool IsLocked
        {
            get { return _locked != null; }
        }

        public void SetPending(DownedReviveConfig config)
        {
            _pending = config == null
                ? DownedReviveConfig.CreateDefault()
                : config.Normalize();
        }

        public DownedReviveConfig LockForRaid()
        {
            _locked = _pending.Clone();
            return _locked.Clone();
        }

        public void EndRaid()
        {
            _locked = null;
        }

        public DownedReviveConfig EffectiveForCurrentRaid()
        {
            return _locked == null ? _pending.Clone() : _locked.Clone();
        }
    }
}
