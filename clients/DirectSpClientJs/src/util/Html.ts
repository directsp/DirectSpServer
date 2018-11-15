
export default class Html {
    
    public static submit(url: string, params: any) {
        let method = "post";

        let form = document.createElement("form");
        form.setAttribute("method", method);
        form.setAttribute("action", url);

        for (let key in params) {
            if (params.hasOwnProperty(key)) {
                let hiddenField = document.createElement("input");
                hiddenField.setAttribute("type", "hidden");
                hiddenField.setAttribute("name", key);
                hiddenField.setAttribute("value", params[key]);
                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();
    }
}