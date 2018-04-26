
#DirectSp framework let you call SQL store procedures directly from front-end and eliminating the need of any ORM!

You will interest in DirectSp if:
* Your application does not need high calculating such as RealTime games
* You are using SQL server
* You don't think OOP can not achieved by 
* You have good designer

You will not interest in DirectSp if:
* You stick to common programming such as C#, Java or PHP

Let see samples:
Just create an API in SQL server like this:

Call your API via your SPA (Single Page Application) like this:
dspClient.invoke("UserCreate", {UserName:"UserName", FirstName: "Elizabete", LastName: "Tailor", GenderId: 2, Email: "eli@mail.com"}).then( result=>
{
       let userId = result.userId;
}



## Features
* Authentication via JWT Brearer
* Load Balancing via SQL Server High-Avalibility group
* Alternative Calander
* Anti XSS
* Batch execution
* CORS Friendly
* Download as TSV (Tab Seperated Values) with compression

## Client JavaScript 
* Login by password
* Login by user consent (implicit login)
* Pure javascript
* Automatic sign-in
* Smart Paginator with cache
* Global Error Handler
* Captcha Handler
* Easy Api call

## DirectSp .NET Client
* Login by Password
* Easy Api call

## DirectSp Authentication Server
* Using OpenID Connect standard
* Refresh-Token
* Profile Info
* Captcha Support

