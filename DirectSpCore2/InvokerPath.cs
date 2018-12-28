using DirectSp.Core.Entities;
using System;
using System.IO;

namespace DirectSp.Core
{
    public class InvokerPath
    {
        public InvokerPath(string workspaceFolder)
        {
            if (string.IsNullOrEmpty(workspaceFolder))
                throw new ArgumentNullException(nameof(workspaceFolder));

            WorkspaceFolder = workspaceFolder;

            Directory.CreateDirectory(WorkspaceFolder);
            Directory.CreateDirectory(RecordsetsFolder);
        }

        public string WorkspaceFolder { get; }

        public string RecordsetsFolder => Path.Combine(WorkspaceFolder, "Recordsets");
    }
}
