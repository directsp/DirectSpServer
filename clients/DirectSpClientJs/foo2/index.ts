namespace directSp {
    
    export class Foo1 {
        prop1: number = 1;
    }


    export class Foo2 extends Foo1 {
        prop1: number;
        zoo1: Zoo1;
        constructor() {
            super();
            this.prop1 = 1;
            this.zoo1 = new Zoo1(1);
        }
    }
}

// global html object
declare let module:any;
if (module)
    module.exports = { directSp: directSp };

if (window)
    (<any>window).directSp = directSp;
