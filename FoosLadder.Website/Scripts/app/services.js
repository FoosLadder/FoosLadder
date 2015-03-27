
(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    foosLadderServices.factory('PlayerService',["$http", "$q", function($http, $q) {
        var playerService = {};

        //playerService.getAll = function() {
            
        //}
        
        var playersDefferred;
        playerService.GetAll = function() {
            if (playersDefferred === undefined) {
                playersDefferred = $http.get("http://localhost:48210/api/players");
            }
            return playersDefferred;
        };

        return playerService;
    }]);
})();