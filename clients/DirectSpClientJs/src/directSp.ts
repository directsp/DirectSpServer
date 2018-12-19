// global html object
if (typeof window !="undefined") (<any>window).directSp = directSp;

// create exports
declare var module:any;
if (typeof module !="undefined") module.exports = {directSp};
