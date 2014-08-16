
function Calendar(options) {

    var that = this;

    options = options || {};

    var dayStartsAt = options.dayStartsAt || 8;
    var dayEndsAt = options.dayStartsAt || 16;
    var hoursPerDay = options.hoursPerDay || 8;
    var daysPerMonth = options.daysPerMonth || 20;

    var hoursBetweenDays = 24 - (dayEndsAt - dayStartsAt);
    var actualHoursPerDay = dayEndsAt - dayStartsAt;

    var SEC_PER_DAY = 60 * 60 * 24;
    var MSEC_PER_DAY = SEC_PER_DAY * 1000;

    function skipWeekend(date, sign) {

        var daysToSkip = 8 - date.isoWeekday();
        if (daysToSkip < 3) {
            if (sign < 0) {
                daysToSkip = daysToSkip - 3;
            }
            date.add('day', daysToSkip);
        }
        return date;
    }

    this.skipOfftime = function (date, sign, last) {

        var hour = date.hour();
        if (sign == 1 && (hour > dayEndsAt || (!last && hour >= dayEndsAt))) {
            date.add('hour', hoursBetweenDays);
        } else if (sign == -1 && (hour < dayStartsAt || (!last && hour <= dayStartsAt))) {
            date.subtract('hour', hoursBetweenDays);
        }

        return skipWeekend(date, sign);
    };

    this.getDifferenceSeconds = function (startDate, endDate, skipOfftime) {

        var adjustedStartDate = moment(startDate).hour(8).minute(0).second(0).milliseconds(0);
        var startDifference = startDate.diff(adjustedStartDate);

        var adjustedEndDate = moment(endDate).hour(8).minute(0).second(0).milliseconds(0);
        var endDifference = endDate.diff(adjustedEndDate);

        var completeDays = adjustedEndDate.diff(adjustedStartDate) / MSEC_PER_DAY;
        var workSeconds = 0;

        var adjustedHoursPerDay = skipOfftime ? hoursPerDay : actualHoursPerDay;


        for (var i = 0; i < completeDays; ++i) {
            adjustedStartDate.add('day', 1);
            if (isWorktime(adjustedStartDate)) {
                workSeconds += 3600 * adjustedHoursPerDay;
            }
        }

        var difference = (startDifference + endDifference) / 1000;
        var restHours = Math.floor(difference / 3600);
        for (i = 0; i < restHours; ++i) {
            if (isWorktime(adjustedStartDate)) {
                workSeconds += 3600;
            }
            adjustedStartDate.add('hour', 1);
        }

        difference -= 3600 * restHours;

        var restMinutes = Math.floor(difference / 60);
        for (i = 0; i < restMinutes; ++i) {
            if (isWorktime(adjustedStartDate)) {
                workSeconds += 60;
            }
            adjustedStartDate.add('minute', 1);
        }

        difference -= 60 * restMinutes;

        for (i = 0; i < difference; ++i) {
            if (isWorktime(adjustedStartDate)) {
                workSeconds += 1;
            }
        }

        return workSeconds;
    }

    function isWorktime(date) {
        return date.isoWeekday() < 6 && date.hour() >= 8 && date.hour() < 16;
    }

    this.getDuration = function (startDate, endDate, skipOfftime) {
        var seconds = this.getDifferenceSeconds(startDate, endDate, skipOfftime);
        var duration = seconds / (3600 * (skipOfftime ? hoursPerDay : actualHoursPerDay));

        var difference = Math.abs(Math.round(duration) - duration);

        if (difference == 0) {
            return duration + 'd';
        } else {
            return duration.toPrecision(2) + 'd';
        }
    }

    this.add = function (date, duration, skipOfftime) {

        var completeDays = 0;
        var hours = 0;

        var parsedDuration = this.parseDuration(duration);
        var value = Math.abs(parsedDuration.value);
        var sign = value / parsedDuration.value;

        var adjustedHoursPerDay = skipOfftime ? hoursPerDay : actualHoursPerDay;

        switch (parsedDuration.modifier) {
            case 'mo':
                var days = value * daysPerMonth;
                completeDays = Math.floor(days);
                hours = (days - completeDays) * adjustedHoursPerDay;
                break;

            case 'w':
                days = value * 5;
                completeDays = Math.floor(days);
                hours = (days - completeDays) * adjustedHoursPerDay;
                break;

            case 'd':
                completeDays = Math.floor(value);
                hours = (value - completeDays) * adjustedHoursPerDay;
                break;

            case 'h':
                completeDays = Math.floor(value / adjustedHoursPerDay);
                hours = (value - (completeDays * adjustedHoursPerDay));
                break;
        }

        addDays(date, completeDays * sign, skipOfftime);
        addHours(date, hours * sign, skipOfftime);

        return date;
    };

    function addDays(date, days, skipOfftime) {

        var realDays = Math.abs(days);
        var sign = realDays / days;

        if (skipOfftime && days > 0) {
            date = that.skipOfftime(date, sign);
        }

        for (var d = 1; d <= realDays; ++d) {
            date.add('hour', actualHoursPerDay * sign);

            if (skipOfftime) {
                date = that.skipOfftime(date, sign, d == realDays);
            }
        }

        return date;
    }

    function addHours(date, hours, skipOfftime) {

        var sign = Math.abs(hours) / hours;

        if (skipOfftime && hours > 0) {
            date = that.skipOfftime(date, sign);
        }

        date.add('hour', hours * sign);

        if (skipOfftime) {
            date = that.skipOfftime(date, sign);
        }
        return date;
    }

    this.parseDuration = function (duration) {
        var result = duration.match(/(-?\d+(?:[.,]\d+)?)\s*(mo|w|d|h|m)/);

        if (result.length == 3) {
            return {
                value: parseFloat(result[1].replace(',', '.')),
                modifier: result[2],
                expression: result[0]
            };
        }

        return null;
    };
}