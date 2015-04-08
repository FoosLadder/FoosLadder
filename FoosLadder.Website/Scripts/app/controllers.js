
(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers',[]);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.GetAllPlayers().success(function (data) {
                $scope.players = data;
                $scope.orderProp = '-TotalMatchesWon';
            });
        }
    ]);
})();