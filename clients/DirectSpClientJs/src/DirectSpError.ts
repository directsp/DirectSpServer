namespace directSp {

    export class DirectSpError extends Error {
        public errorType: string | null = null;
        public errorName: string | null = null;
        public errorNumber: number | null = null;
        public errorMessage: string | null = null;
        public errorDescription: string | null = null;
        public errorProcName: string | null = null;
        public status: number | null = null;
        public statusText: string | null = null;
        public innerError: Error | null = null;
        public errorData: any = null;

        constructor(message: string) {
            super(message);
            this.errorMessage = message;
        }

        static create(data: any): DirectSpError {
            // already converted
            if (data instanceof DirectSpError) return data;

            // create error
            let error = new DirectSpError("");

            //casting data
            if (data == null) {
                error.errorName = "unknown";
            } else if (data == "number") {
                error.errorType = "number";
                error.errorNumber = data;
            } else if (typeof data == "string") {
                error.errorType = "string";
                error.errorName = data;
            } else if (typeof data != "object") {
                error.errorName = data;
            } else if (
                data.errorName != null ||
                data.errorNumber != null ||
                data.error != null ||
                data.error_description != null
            ) {
                //copy all data
                error.errorName = Utility.checkUndefined(data.errorName, null);
                error.errorNumber = Utility.checkUndefined(data.errorNumber, null);
                error.errorMessage = Utility.checkUndefined(data.errorMessage, null);
                error.errorType = Utility.checkUndefined(data.errorType, null);
                error.errorDescription = Utility.checkUndefined(data.errorDescription, null);
                error.errorProcName = Utility.checkUndefined(data.errorProcName, null);
                error.status = Utility.checkUndefined(data.status, null);
                error.statusText = Utility.checkUndefined(data.statusText, null);
                error.innerError = Utility.checkUndefined(data.innerError, null);

                //try to convert error
                if (data.error) {
                    error.errorType = "OpenIdConnect";
                    error.errorName = data.error;
                }

                //try to convert error_description
                if (data.error_description) {
                    error.errorDescription = data.error_description;
                }

            } else {
                error.errorName = "unknown";
                error.errorDescription = data.toString();
                error.innerError = data;
            }

            //try to reconver actual data from error_description
            if (error.errorDescription) {
                try {
                    let obj = JSON.parse(error.errorDescription);
                    if (obj.errorName || obj.errorNumber)
                        return DirectSpError.create(obj);
                } catch (e) { }
            }

            //fix error message
            if (error.errorName)
                error.message = error.errorName;
            if (error.errorMessage)
                error.message = error.errorMessage;

            return error;
        }
    }


    export namespace exceptions {
        export class NotSupportedException extends DirectSpError {
            constructor(message?: string) {
                super("The operation is not supported!" + message ? " " + message : "");
                this.errorType = "Client";
                this.errorName = "NotSupportedException";
            }
        }

        export class NotImplementedException extends DirectSpError {
            constructor(message?: string) {
                super("The method is not implemeneted!" + message ? " " + message : "");
                this.errorType = "Client";
                this.errorName = "NotImplementedException";
            }
        }
    }
}
