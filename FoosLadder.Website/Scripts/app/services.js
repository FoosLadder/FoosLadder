(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    function buildUrl(baseUrl, path) {
        return baseUrl + path;
    }

    foosLadderServices.factory('UserAccountService', ['$http', '$q', function ($http, $q) {
        var service = {
            currentUserDetails: currentUserDetails
        };

        function currentUserDetails() {
            var deferred = $q.defer();
            var userDetails = {
                fullName: 'My name',
                userName: 'MyUserName',
                userId: 1
            };
            deferred.resolve(userDetails);

            return deferred.promise;
        };

        return service;
    }]);

    foosLadderServices.factory('PlayerService', ['$http', '$q', 'apiBaseUrl', function ($http, $q, apiBaseUrl) {
        var playerService = {};
        var players = [];
        playerService.GetAll = function () {
            var deferred = $q.defer();
            if (players.length === 0) {
                $http.get(buildUrl(apiBaseUrl, 'players')).success(function(data) {
                    players = data;
                    deferred.resolve(players);
                });
            } else {
                deferred.resolve(players);
            }
            return deferred.promise;
        };

        return playerService;
    }]);

    foosLadderServices.factory('ScoreService', ['$http', 'apiBaseUrl', function ($http, apiBaseUrl) {
        var scoreService = {};
        scoreService.postCompletedMatch = function (data) {
            return $http.post(buildUrl(apiBaseUrl, 'matches/completed'), data, { headers: { 'Content-Type': 'application/json' } });
        };

        return scoreService;
    }]);
})();