import DirectSpHtmlStorageClass from "./DirectSpHtmlStorage"
import DirectSpErrorClass from "./DirectSpError"
import HtmlClass from "./util/Html"
import UriClass from "./util/Uri"
import ConvertClass from "./util/Convert"
import UtilityClass from "./util/Utility"

export namespace directSp {
  export const DirectSpHtmlStorage =  DirectSpHtmlStorageClass;
  export const DirectSpError  = DirectSpErrorClass;
  export const Html  = HtmlClass;
  export const Uri  = UriClass;
  export const Convert  = ConvertClass;
  export const Utility  = UtilityClass;
}

declare let window:any;
window.directSp = directSp;

