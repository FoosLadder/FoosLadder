
(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers',[]);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.GetAll().success(function(data) {
                $scope.players = data;
                $scope.orderProp = '-TotalMatchesWon';
            });
        }
    ]);

    foosLadderControllers.controller('PlayerRegistrationController'[
        '$scope', 'PlayerService',
        function ($scope, playerService) {
        }
    ]);
})();