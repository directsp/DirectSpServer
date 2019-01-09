namespace directSp {
    export class DirectSpHelp {
        public static help(systemApi: any, criteria: string | null = null): string {
            let result = "";

            // show help
            if (criteria != null) criteria = criteria.trim().toLowerCase();
            if (criteria == "") criteria = null;

            //find all proc that match
            let foundProc: any = [];
            for (var item in systemApi) {
                if (systemApi.hasOwnProperty(item)) {
                    let api = systemApi[item];
                    if (criteria == null || api.procedureName.toLowerCase().indexOf(criteria) != -1) {
                        foundProc.push(api);
                    }
                }
            }

            //show procedure if there is only one procedure
            if (foundProc.length == 0) {
                result += ("DirectSp: Nothing found!");
            } else {
                for (let i = 0; i < foundProc.length; i++) {
                    if (foundProc.length == 1)
                        result += this._helpImpl(foundProc[i]);
                    else
                        result += (foundProc[i].procedureName) + "\n";
                }
            }

            result += "\n" + "---------------";
            return result;
        };

        private static _helpImpl(procedureMetadata: any): string {

            // find max param length
            let maxParamNameLength: number = 0;
            let inputParams = [];
            for (let i = 0; i < procedureMetadata.params.length; i++) {
                let param = procedureMetadata.params[i];
                maxParamNameLength = Math.max(maxParamNameLength, param.paramName.length);
                if (!param.isOutput && param.paramName.toLowerCase() != "@context") {
                    inputParams.push(this._formatHelpParamName(param.paramName));
                }
            }

            //prepare input params
            let str: string = "";
            str += "\n" + "---------------";
            str += "\n" + "Method:";
            str +=
                "\n\t" +
                procedureMetadata.procedureName +
                " (" +
                inputParams.join(", ") +
                ")";
            str += "\n";
            str += "\n" + "Parameters:";
            for (let i = 0; i < procedureMetadata.params.length; i++) {
                let param = procedureMetadata.params[i];
                if (!param.isOutput && param.paramName.toLowerCase() != "@context") {
                    str +=
                        "\n\t" +
                        this._getHelpParam(procedureMetadata, param, maxParamNameLength);
                }
            }

            //prepare ouput params
            str += "\n";
            str += "\n" + "Returns:";
            for (let i = 0; i < procedureMetadata.params.length; i++) {
                let param = procedureMetadata.params[i];
                if (param.isOutput && param.paramName.toLowerCase() != "@context") {
                    str +=
                        "\n\t" +
                        this._getHelpParam(procedureMetadata, param, maxParamNameLength);
                }
            }
            str +=
                "\n\t" +
                this._formatHelpParam("returnValue", "integer", maxParamNameLength);
            str +=
                "\n\t" + this._formatHelpParam("recordset", "array", maxParamNameLength);

            //sample
            let sample = 'dspClient.invoke("$(procname)", { $(parameters) })';
            let sampleParam = [];
            for (let i = 0; i < inputParams.length; i++)
                sampleParam.push(inputParams[i] + ": " + "$" + inputParams[i]);
            sample = sample.replace("$(procname)", procedureMetadata.procedureName);
            sample = sample.replace("$(parameters)", sampleParam.join(", "));
            str += "\n";
            str += "\n" + "Sample:";
            str += "\n\t" + sample;

            return str;
        };

        private static _getHelpParam(procedureMetadata: any, param: any, maxParamNameLength: number) {
            let paramType = this._getHelpParamType(param);
            return this._formatHelpParam(param.paramName, paramType, maxParamNameLength);
        };

        private static _getHelpParamType(param: any): string {
            //check userTypeName
            let userTypeName =
                param.userTypeName != null ? param.userTypeName.toLowerCase() : "";
            if (userTypeName.indexOf("json") != -1) return "object";

            //check systemTypeName
            let paramType = param.systemTypeName.toLowerCase();
            if (paramType.indexOf("char") != -1) return "string";
            else if (paramType.indexOf("date") != -1) return "datetime";
            else if (paramType.indexOf("time") != -1) return "datetime";
            else if (paramType.indexOf("money") != -1) return "money";
            else if (paramType.indexOf("int") != -1) return "integer";
            else if (paramType.indexOf("float") != -1) return "float";
            else if (paramType.indexOf("decimal") != -1) return "float";
            else if (paramType.indexOf("bit") != -1) return "boolean";
            else if (paramType.indexOf("bool") != -1) return "boolean";
            else if (paramType.indexOf("string") != -1) return "string";
            return "object";
        };

        private static _formatHelpParamName(paramName: string): string {
            //remove extra characters
            if (paramName.length > 0 && paramName[0] == "@")
                paramName = paramName.substr(1);
            return Utility.toCamelcase(paramName);
        };

        private static _formatHelpParam(paramName: string, paramType: string, maxParamNameLength: number): string {
            let str = this._formatHelpParamName(paramName);

            //add spaces
            for (let i = str.length; i < maxParamNameLength + 2; i++)
                str += " ";

            return str + "(" + paramType + ")";
        };

    }
}