define(function() {

    var ConvertAppToKnockoutApp = function(dto) {
        return mapToObservable(dto);
    };

    var model = {
        ConvertAppToKnockoutApp: ConvertAppToKnockoutApp
    };

    return model;

    function mapToObservable(dto) {
        var mapped = {};
        for (prop in dto) {
            if (dto.hasOwnProperty(prop)) {
                mapped[prop] = ko.observable(dto[prop]);
            }
        }
        return mapped;
    }
});