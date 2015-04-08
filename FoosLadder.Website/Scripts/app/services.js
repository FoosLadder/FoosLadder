

(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    foosLadderServices.factory('PlayerService', ["$http", function ($http) {
        var playerService = {};
        var players = [];
        playerService.GetAll = function (callback) {
            if (players.length === 0) {
                $http.get("http://localhost:48210/api/players").success(function (data) {
                    players = data;
                    callback(players);
                });
            } else {
                callback(players);
            }
        };

        return playerService;
    }]);

    foosLadderServices.factory('ScoreService', ["$http", function ($http) {
        var service = {};
        var players = [];
        service.postCompletedMatch = function (data) {
            return $http.post("http://localhost:48210/api/matches/completed", data, { headers: { 'Content-Type': 'application/json' } });
        };

        return service;
    }]);
})();