using System;
using System.Threading;

namespace DirectSp
{
    class UserSession
    {
        public UserSession(string authUserId, string audience, DirectSpAgentContext agentContext = null)
        {
            AuthUserId = authUserId;
            Audience = audience;
            AgentContext = agentContext ?? new DirectSpAgentContext();
        }

        private readonly object _lockObject = new object();
        private DateTime _lastRequestTime;
        public DateTime LastWriteTime
        {
            get
            {
                lock (_lockObject)
                    return _lastRequestTime;
            }
        }
        public void SetWriteMode(bool isWriteMode)
        {
            lock (_lockObject)
            {
                _requestCount++;
                if (isWriteMode)
                    _lastRequestTime = DateTime.Now;
            }
        }


        private DateTime _requestIntervalStartTime;
        public DateTime RequestIntervalStartTime
        {
            get
            {
                lock (_lockObject)
                    return _requestIntervalStartTime;
            }
        }

        private int _requestCount;
        public int RequestCount
        {
            get
            {
                lock (_lockObject)
                    return _requestCount;
            }
        }

        public void ResetRequestCount()
        {
            lock (_lockObject)
            {
                _requestIntervalStartTime = DateTime.Now;
                _requestCount = 0;
            }
        }

        private DirectSpAgentContext _agentContext;

        public string AuthUserId { get; }
        public string Audience { get; }

        public DirectSpAgentContext AgentContext
        {
            get
            {
                lock (_lockObject)
                    return _agentContext;
            }
            set
            {
                lock (_lockObject)
                    _agentContext = value;
            }
        }

    }
}
