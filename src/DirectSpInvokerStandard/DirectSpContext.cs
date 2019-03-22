using System.Threading;

namespace DirectSp
{
    public class DirectSpContext
    {
        public string AppName { get; internal set; }
        public string AppVersion { get; internal set; }
        public string AuthUserId { get; internal set; }
        public string Audience { get; internal set; }
        public bool IsBatch { get; internal set; }
        public bool IsCaptcha { get; internal set; }
        public int? RecordIndex { get; internal set; }
        public int? RecordCount { get; internal set; }
        public string ClientVersion { get; internal set; }
        public string RemoteIp { get; internal set; }
        public bool IsReadonlyIntent { get; internal set; }
        public DirectSpAgentContext AgentContext { get; internal set; }

        public static DirectSpContext Current
        {
            get
            {
                var nameSlot = Thread.GetNamedDataSlot("DirectSpContext");
                return nameSlot!=null ? (DirectSpContext)Thread.GetData(nameSlot) : null;

            }
        }
    }
}
