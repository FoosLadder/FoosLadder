
//TODO Switch from callbacks to promises
(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers',[]);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function($scope, playerService) {
            playerService.GetAll(function(data) {
                $scope.players = data;
                $scope.orderProp = '-TotalMatchesWon';
            });
        }
    ]);
})();