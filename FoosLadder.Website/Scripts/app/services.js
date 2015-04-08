(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    foosLadderServices.factory('PlayerService', ["$http", "$location", function ($http, $location) {
        var playerService = {};

        playerService.GetAllPlayers = (function () {
            var playersDefferred;
            return function () {
                if (playersDefferred === undefined) {
                    playersDefferred = $http.get("https://localhost:44301/api/players");
                }
                return playersDefferred;
            };
        })();

        return playerService;
    }]);
})();