function PathBuilder() {
    var result = "";

    var lastCoordinate;

    this.spathTo = function (points, r) {

        var that = this;
        var allButLast = points.slice(0, -1);

        var tempX = 0, tempY = 0;
        var first = true;

        for (var i in allButLast) {

            var index = parseInt(i);

            var point = points[index];
            var nextPoint = points[index + 1];

            var deltaX = nextPoint[0] - point[0];
            var dirX = deltaX != 0 ? Math.abs(deltaX) / deltaX : 0;

            var deltaY = nextPoint[1] - point[1];
            var dirY = deltaY != 0 ? Math.abs(deltaY) / deltaY : 0;

            if (first) {
                first = false;
            } else {
                that.scurveTo(tempX, tempY, point[0] + (r * dirX), point[1] + (r * dirY));
            }

            tempX = nextPoint[0];
            tempY = nextPoint[1];

            that.lineTo(nextPoint[0] - (r * dirX), nextPoint[1] - (r * dirY));
        }

        var lastPoint = points[points.length - 1];
        this.lineTo(lastPoint[0], lastPoint[1]);
    }

    this.moveTo = function (x, y) {
        lastCoordinate = [x, y];
        result += 'M' + x + ' ' + y + ' ';
        return this;
    };

    this.lineTo = function (x, y) {
        lastCoordinate = [x, y];
        result += 'L' + x + ' ' + y + ' ';
        return this;
    };

    this.hlineTo = function (x) {
        lastCoordinate = [x, lastCoordinate[1]];
        result += 'H' + x + ' ';
        return this;
    };

    this.vlineTo = function (y) {
        lastCoordinate = [lastCoordinate[0], y];
        result += 'V' + y + ' ';
        return this;
    };

    this.scurveTo = function (x2, y2, x, y) {
        lastCoordinate = [x, y];
        result += 'S' + x2 + ' ' + y2 + ' ' + x + ' ' + y + ' ';
        return this;
    };

    this.close = function () {
        result += 'Z';
        return this;
    };

    this.getPath = function () {
        return result;
    }
}
