export * from "./DirectSpAjax";
export * from "./DirectSpStorage";
export * from "./DirectSpError";
export * from "./DirectSpClient";
export * from "./DirectSpUtil";


// config javascript raw directSp namespace
import { DirectSpHtmlStorage as DirectSpHtmlStorageClass } from "./DirectSpStorage"
import { DirectSpError as DirectSpErrorClass } from "./DirectSpError"
import { DirectSpClient as DirectSpClientClass } from "./DirectSpClient"
import { exceptions as exceptionsNS } from "./DirectSpError"
import { Html as HtmlClass, Uri as UriClass, Utility as UtilityClass, Convert as ConvertClass } from "./DirectSpUtil"
import { TestUtil as TestUtilClass } from "../test/TestUtil"

namespace directSp {
  export const DirectSpHtmlStorage = DirectSpHtmlStorageClass;
  export const DirectSpError = DirectSpErrorClass;
  export const DirectSpClient = DirectSpClientClass;
  export const Html = HtmlClass;
  export const Uri = UriClass;
  export const Convert = ConvertClass;
  export const Utility = UtilityClass;
  export const TestUtil = TestUtilClass;
  export const exceptions = exceptionsNS;
}


// global html object
declare let window: any;
window.directSp = directSp;



