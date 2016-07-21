/// <reference path="angular.min.js" />

// This is the main angularjs module and controller code.
// BarChartCtrl is referenced in the index.html file for all values.

var myApp = angular
    .module("app", ['chart.js','ui-listView'])
    .controller("BarChartCtrl", function ($scope, $http, $log) {

    var successCallback = function (response) { // function called when GET succeeds
        $scope.numPeopleInSample = response.data.NumPeopleInSample
        $scope.data = response.data.NumLivesList;
        $scope.maxYearValue = response.data.MaxLifeYearBinValue;
        $scope.labels = response.data.YearList;
        $scope.highestLifeCountList = response.data.YearsWithHighestLifeCountList;
        $log.info(response); // log response to the console
    };

    var errorCallback = function (reason) { // function called if error happened
        $scope.error = reason.data;
        $log.info(reason);
    };

    // Call GetLifeData to get the data for the chart
    $http({ method: 'GET', url: "LifeDataWS.asmx/GetLifeData" })
            .then(successCallback, errorCallback); // 'then' func called when GET succeeds

});