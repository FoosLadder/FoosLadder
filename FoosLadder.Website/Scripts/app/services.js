

(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    foosLadderServices.factory('PlayerService',["$http", function($http) {
        var playerService = {};
        var players = [];
        playerService.GetAll = function(callback) {
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
})();