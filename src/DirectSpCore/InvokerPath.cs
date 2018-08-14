using DirectSp.Core.Entities;
using System;
using System.IO;

namespace DirectSp.Core
{
    public class InvokerPath
    {
        public InvokerPath(string workingFolder)
        {
            if (string.IsNullOrEmpty(workingFolder))
                throw new ArgumentNullException(nameof(workingFolder));

            WorkingFolder = workingFolder;

            Directory.CreateDirectory(WorkingFolder);
            Directory.CreateDirectory(RecordsetsFolder);
        }

        public string WorkingFolder { get; }

        public string RecordsetsFolder => Path.Combine(WorkingFolder, "Recordsets");
    }
}
