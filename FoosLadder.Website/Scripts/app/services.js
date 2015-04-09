(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

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

    foosLadderServices.factory('PlayerService', ['$http', '$q', function ($http, $q) {
        var playerService = {};
        var players = [];
        playerService.GetAll = function () {
            var deferred = $q.defer();
            if (players.length === 0) {
                $http.get('http://localhost:48210/api/players').success(function(data) {
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

    foosLadderServices.factory('ScoreService', ['$http', function ($http) {
        var scoreService = {};
        scoreService.postCompletedMatch = function (data) {
            return $http.post('http://localhost:48210/api/matches/completed', data, { headers: { 'Content-Type': 'application/json' } });
        };

        return scoreService;
    }]);
})();