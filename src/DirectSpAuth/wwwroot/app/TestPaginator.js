/**
 * Created by madnik7 on 5/20/2017.
 */

"use strict";

//namespace
if (typeof directSp == "undefined") var directSp = {};

//dspClient
directSp.TestPaginator = function () {
    this.paginator = null;
};

directSp.TestPaginator.prototype.checkExpected = function (param, expect, calleName) {
    var msg = null;
    var paginator = this.paginator;
    if (expect.hasNextPage != undefined && expect.hasNextPage != paginator.hasNextPage) msg = "hasNextPage";
    if (expect.pageCount != undefined && expect.pageCount != paginator.pageCount) msg = "pageCount";
    if (expect.recordset != undefined && JSON.stringify(expect.recordset) != JSON.stringify(paginator.recordset)) msg = "recordset";
    if (expect.pageCountMin != undefined && expect.pageCountMin != paginator.pageCountMin) msg = "pageCountMin";
    if (expect.pageCountMax != undefined && expect.pageCountMax != paginator.pageCountMax) msg = "pageCountMax";
    if (expect.pageIndex != undefined && expect.pageIndex != paginator.pageIndex) msg = "pageIndex";
    if (expect.isCacheUsed != undefined && expect.isCacheUsed != paginator.isCacheUsed) msg = "isCacheUsed";
    if (expect.isCacheInvalidated != undefined && expect.isCacheInvalidated != paginator.isCacheInvalidated) msg = "isCacheInvalidated";
    if (expect.isInvoked != undefined && expect.isInvoked != paginator.isInvoked) msg = "isInvoked";
    if (msg != null) {
        msg = msg + " is not matched";
        console.error(calleName, msg, param, paginator);
        throw msg;
    }
};

directSp.TestPaginator.prototype.checkByInvoke = function (param, expect) {
    this.paginator = dspClient.invoke("RequesetRecords", null, { pageSize: param.pageSize, pageCacheCount: param.pageCacheCount, delay: 1000 });
    return this.paginator.goPage(param.pageIndex)
        .then(result => {
            //wait next page to be completed for testing in sync mode
            return Promise.all(this.paginator._pagePromises);
        }).then(result => {
            this.checkExpected(param, expect, "checkByInvoke");
            return result;
        });
};

directSp.TestPaginator.prototype.checkByPage = function (pageNo, expect) {
    return this.paginator.goPage(pageNo)
        .then(result => {
            //wait next page to be completed for testing in sync mode
            return Promise.all(this.paginator._pagePromises);
        }).then(result => {
            this.checkExpected({}, expect, "checkByPage");
            return result;
        });

};

directSp.TestPaginator.prototype.start = function () {
    var data = { recordset: [] };
    dspClient.resourceApiUri = "http://mock";
    dspClient.apiHook = function (method, params, options) {
        return data;
    }

    //check zero result
    console.log("Test 1 (check zero result) ...");
    data.recordset = [];
    this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 })
        .then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 0 })
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {

            console.log("Test 2 ...");
            data.recordset = [0];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 0 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 1 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 1, pageCacheCount: 2 }, { hasNextPage: false, recordset: [0], pageCount: 1, pageCountMin: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: 1, pageCountMin: 1, pageIndex: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 2 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageIndex: 9 });
        }).then(result => {

            console.log("Test 3 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: true, recordset: [0, 1, 2, 3, 4], pageCount: null, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [5, 6, 7, 8, 9], pageCount: 2, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 2, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: 2, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: true, recordset: [0, 1, 2, 3, 4], pageCount: 2, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [5, 6, 7, 8, 9], pageCount: 2, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 9, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1 });
        }).then(result => {

            console.log("Test 4 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: true, recordset: [0, 1, 2, 3, 4], pageCount: null, pageCountMin: 2 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: true, recordset: [5, 6, 7, 8, 9], pageCount: null, pageCountMin: 3 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 2, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [10], pageCount: 3, pageCountMin: 3 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: true, recordset: [0, 1, 2, 3, 4], pageCount: null, pageCountMin: 3 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 1, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: true, recordset: [5, 6, 7, 8, 9], pageCount: null, pageCountMin: 3 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 3, pageSize: 5, pageCacheCount: 0 }, { hasNextPage: false, recordset: [], pageCount: null, pageCountMin: 1, pageCountMax: 3 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 3, pageSize: 5, pageCacheCount: 1 }, { hasNextPage: false, recordset: [], pageCount: 3, pageCountMin: 3 });
        }).then(result => {

            //Check pagination
            console.log("Test 5 (pagination) ...");
            data.recordset = [0];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 0 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0 });
        }).then(result => {
            return this.checkByPage(0, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0, pageCountMax: 1 });
        }).then(result => {
            return this.checkByPage(1, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0, pageCountMax: 1 });
        }).then(result => {
            return this.checkByPage(10, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0, pageCountMax: 1 });
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0 });
        }).then(result => {
            return this.checkByPage(0, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByPage(1, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0 });
        }).then(result => {
            return this.checkByPage(10, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageIndex: 0 });
        }).then(result => {

            console.log("Test 6 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: null });
        }).then(result => {
            return this.checkByPage(0, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: null });
        }).then(result => {
            return this.checkByPage(1, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: 3 });
        }).then(result => {

            console.log("Test 7 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: null });
        }).then(result => {
            return this.checkByPage(10, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: 9 });
        }).then(result => {
            return this.checkByPage(10, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: 7 });
        }).then(result => {
            return this.checkByPage(2, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: 4 });
        }).then(result => {
            return this.checkByPage(1, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: 4 });
        }).then(result => {
            return this.checkByPage(3, { isInvoked: 0, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: 4 });
        }).then(result => {

            console.log("Test 8 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: null });
        }).then(result => {
            return this.checkByPage(2, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: null, pageCount: null });
        }).then(result => {
            return this.checkByPage(3, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: null, pageCount: null });
        }).then(result => {

            console.log("Test 9 ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21];
        }).then(result => {
            return this.checkByInvoke({ pageIndex: 0, pageSize: 5, pageCacheCount: 1 }, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 0, pageCountMax: null });
        }).then(result => {
            return this.checkByPage(1, { isInvoked: 1, isCacheInvalidated: 0, isCacheUsed: 1, pageCountMax: null, pageCount: null });
        }).then(result => {

            console.log("Test 10 ...");
            data.recordset[14] = 0;
            return this.checkByPage(3, { isInvoked: 1, isCacheInvalidated: 1, isCacheUsed: 0, pageCountMax: null, pageCount: null });
        }).then(result => {

            console.log("Test 11 ...");
            data.recordset[15] = 0;
            return this.checkByPage(1, { isInvoked: 1, isCacheInvalidated: 1, isCacheUsed: 0, pageCountMax: null, pageCount: null });
        }).then(result => {

            // check for last page
            console.log("Test 12 (last page) ...");
            data.recordset = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
            return this.checkByInvoke({ pageIndex: 0, pageSize: 4 }, {});
        }).then(result => {
            return this.checkByPage(0, { hasNextPage: true });
        }).then(result => {
            return this.checkByPage(1, { hasNextPage: true });
        }).then(result => {
            return this.checkByPage(2, { hasNextPage: false });
        }).then(result => {
            return this.checkByPage(3, { hasNextPage: false });
        });
};


