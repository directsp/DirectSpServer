using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DirectSp
{
    class UserSessionManager
    {
        public int SessionTimeout { get; }
        public UserSessionManager(int sessionTimeout)
        {
            SessionTimeout = sessionTimeout;
        }

        private ConcurrentDictionary<string, UserSession> UserSessions = new ConcurrentDictionary<string, UserSession>();
        public UserSession GetUserSession(string authUserId, string audience)
        {
            //create default audience
            if (audience == null)
                audience = "";

            //create empty session for anonymous
            if (string.IsNullOrEmpty(authUserId))
                return new UserSession(authUserId, audience);

            //try cleanup on each request
            CleanUp();

            //Create or get userSession
            var sessionKey = authUserId + "#" + audience ?? "";
            if (!UserSessions.TryGetValue(sessionKey, out UserSession userSession))
            {
                userSession = new UserSession(authUserId, audience);
                UserSessions[sessionKey] = userSession;
            }

            return userSession;
        }

        private readonly object _cleaningLock = new object();
        private DateTime _lastCleanupTime = DateTime.Now;

        private void CleanUp()
        {
            if (!Monitor.TryEnter(_cleaningLock))
                return;

            try
            {
                //Check the cleanup time
                if (_lastCleanupTime.AddSeconds(SessionTimeout) > DateTime.Now)
                    return;

                //find expired sessions
                var expired = new List<string>();
                foreach (var item in UserSessions)
                {
                    if (item.Value.LastWriteTime.AddSeconds(SessionTimeout) < DateTime.Now)
                        expired.Add(item.Key);
                }

                //clear expired sessions
                foreach (var item in expired)
                {
                    UserSessions.TryRemove(item, out UserSession userSession);
                }

                _lastCleanupTime = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(_cleaningLock);
            }
        }

    }
}
