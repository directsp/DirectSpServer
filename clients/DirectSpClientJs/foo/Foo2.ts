export class Foo1 {
    prop1: number = 1;
}

export class Foo2  extends Foo1 {
    prop1: number;
    constructor() {
        super();
        this.prop1 = 1;
        //this.prop1 = new Foo1();
    }
}
