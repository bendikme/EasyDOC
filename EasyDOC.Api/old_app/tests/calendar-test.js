///<reference path="~/Scripts/moment.js"/>
///<reference path="~/Scripts/jasmine.js"/>
///<reference path="~/app/components/gantt-calendar.js"/>

describe("Calendar", function () {
    var calendar = new Calendar();

    it("should skip non-working hours", function () {
        var date = moment().isoWeekday(1).hour(16);
        date = calendar.add(date, '2h', true);
        console.log(date.hour());

        expect(date.isoWeekday()).toBe(2);
        expect(date.hour()).toBe(10);
    });

    it("should stop at end of working day when adding days", function () {
        var date = moment().isoWeekday(5).hour(8);
        date = calendar.add(date, '1d');

        expect(date.isoWeekday()).toBe(5);
        expect(date.hour()).toBe(16);
    });

    it("should skip weekends when adding more than one day", function () {
        var date = moment().isoWeekday(5).hour(8);
        date = calendar.add(date, '1,1d', true);

        expect(date.isoWeekday()).toBe(1);
        expect(date.hour()).toBe(8);
    });

    it("should skip weekends when adding two days", function () {
        var date = moment().isoWeekday(5).hour(8);
        date = calendar.add(date, '2d', true);

        expect(date.isoWeekday()).toBe(1);
        expect(date.hour()).toBe(16);
    });

    it("should skip weekends when subtracting one day", function () {
        var date = moment().isoWeekday(1).hour(9);
        date = calendar.add(date, '-1d', true);

        expect(date.isoWeekday()).toBe(5);
        expect(date.hour()).toBe(9);
    });

    it("should return correct seconds between dates", function () {
        var startDate = moment("2014-04-07 08:00:00");
        var endDate = moment("2014-04-10 16:00:00");

        var seconds = calendar.getDifferenceSeconds(startDate, endDate);

        expect(seconds).toBe(115200);
    });

    it("should return correct seconds between dates 2", function () {
        var startDate = moment("2014-04-07 08:00:00");
        var endDate = moment("2014-04-08 09:23:23");

        var seconds = calendar.getDifferenceSeconds(startDate, endDate);

        expect(seconds).toBe(3600 + 1380 + 23 + 28800);
    });

    it("should return correct duration", function () {
        var startDate = moment("2014-04-07 08:00:00");
        var endDate = moment("2014-04-07 16:00:00");

        var seconds = calendar.getDuration(startDate, endDate);

        expect(seconds).toBe('1d');
    });

    it("should return correct duration 2", function () {
        var startDate = moment("2014-01-01 08:00:00");
        var endDate = moment("2014-01-01 16:00:00");

        var seconds = calendar.getDuration(startDate, endDate);

        expect(seconds).toBe('1d');
    });

});