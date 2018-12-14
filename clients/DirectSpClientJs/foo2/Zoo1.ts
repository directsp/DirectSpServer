namespace directSp {

    export class Zoo0 {
        v0: number;
        constructor(v0: number) {
            this.v0 = v0;
        }
    }

    export class Zoo1 extends Zoo0 {
        v: number;
        constructor(v: number) {
            super(v)
            this.v = v;

        }
        vv: number = 1;
    }
}