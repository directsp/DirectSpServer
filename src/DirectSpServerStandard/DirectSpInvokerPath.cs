using DirectSp.Entities;
using System;
using System.IO;

namespace DirectSp
{
    public class DirectSpInvokerPath
    {
        public DirectSpInvokerPath(string workspaceFolder)
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
