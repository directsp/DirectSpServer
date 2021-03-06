﻿using DirectSp.ProcedureInfos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSp
{
    public class CommandParam
    {
        public SpParamInfo SpParam;
        public object Value = Undefined.Value;

    }

    public class CommandResultField
    {
        public string Name;
        public string TypeName;
    }

    public class CommandResultTable
    {
        public CommandResultField[] Fields;
        public object[][] Data;
    }

    public class CommandResult
    {
        public Dictionary<string, object> OutParams = new Dictionary<string, object>();
        public CommandResultTable Table;
        public DirectSpAgentContext AgentContext;
    }

    public interface ICommandProvider
    {
        Task<SpSystemApiInfo> GetSystemApi();
        Task<CommandResult> Execute(SpInfo procInfo, DirectSpContext context, IDictionary<string, object> callParams, bool isReadScale);
    }
}
