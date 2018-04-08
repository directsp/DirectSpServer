using System;
using System.Linq;
using System.Security.Claims;

namespace DirectSp.Core
{
    class UserSession
    {
        public string UserId { get; private set; }
        public string Audience { get; private set; }
        public string AppName { get; private set; }

        public UserSession(string appName, string userId, string audience)
        {
            Audience = audience;
            AppName = appName;
            UserId = userId;
            SpContext = new SpContext(appName, userId, audience);
        }

        private object LockObject = new object();
        private DateTime _LastRequestTime;
        public DateTime LastWriteTime
        {
            get
            {
                lock (LockObject)
                    return _LastRequestTime;
            }
        }
        public void SetCurrentRequestMode(bool writeMode)
        {
            lock (LockObject)
            {
                _RequestCount++;
                if (writeMode)
                    _LastRequestTime = DateTime.Now;
            }
        }


        private DateTime _RequestIntervalStartTime;
        public DateTime RequestIntervalStartTime
        {
            get
            {
                lock (LockObject)
                    return _RequestIntervalStartTime;
            }
        }

        private int _RequestCount;
        public int RequestCount
        {
            get
            {
                lock (LockObject)
                    return _RequestCount;
            }
        }

        public void ResetRequestCount()
        {
            lock (LockObject)
            {
                _RequestIntervalStartTime = DateTime.Now;
                _RequestCount = 0;
            }
        }

        private SpContext _SpContext;
        public SpContext SpContext
        {
            get
            {
                lock (LockObject)
                    return _SpContext;
            }
            set
            {
                lock (LockObject)
                    _SpContext = value;
            }
        }

    }
}
