using DirectSp.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSp.Core
{
    class UserSessionManager
    {
        public SpInvokerOptions Options { get; private set; }
        public UserSessionManager(SpInvokerOptions options)
        {
            Options = options;
        }

        private ConcurrentDictionary<string, UserSession> UserSessions = new ConcurrentDictionary<string, UserSession>();
        public UserSession GetUserSession(string appName, string userId, string audience)
        {
            //try cleanup on each request
            CleanUp();

            //Create or get userSession
            var sessionKey = userId + "#" + audience;
            if (!UserSessions.TryGetValue(sessionKey, out UserSession userSession))
            {
                userSession = new UserSession(new  SpContext(appName, userId, audience));
                UserSessions[sessionKey] = userSession;
            }

            return userSession;
        }

        private object CleaningLock = new object();
        private DateTime LastCleanupTime = DateTime.Now;
        private void CleanUp()
        {
            if (!Monitor.TryEnter(CleaningLock))
                return;

            try
            {
                //Check the cleanup time
                if (LastCleanupTime.AddSeconds(Options.SessionTimeout) > DateTime.Now)
                    return;

                //find expired sessions
                var expired = new List<string>();
                foreach (var item in UserSessions)
                {
                    if (item.Value.LastWriteTime.AddSeconds(Options.SessionTimeout) < DateTime.Now)
                        expired.Add(item.Key);
                }

                //clear expired sessions
                foreach (var item in expired)
                {
                    UserSessions.TryRemove(item, out UserSession userSession);
                }

                LastCleanupTime = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(CleaningLock);
            }
        }

    }
}
