namespace DirectSp.Client
{
    internal class SpCallError
    {
        public string errorType { get; set; }
        public int errorNumber { get; set; }
        public string errorName { get; set; }
        public string errorMessage { get; set; }
        public string errorDescription { get; set; }
        public string errorProcName { get; set; }
        public object errorData { get; set; }
    }
}
