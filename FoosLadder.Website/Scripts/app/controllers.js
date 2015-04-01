
//TODO Switch from callbacks to promises
(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers', []);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.GetAll(function (data) {
                $scope.scores = data;
                $scope.orderProp = '-TotalMatchesWon';
            });
        }
    ]);

    foosLadderControllers.controller('SubmitScoresController', [
        '$scope', 'ScoreService',
        function ($scope, scoreService) {
            $scope.submit = submit;

            activate();

            function activate() {
                $scope.teamA = { name: "Your name" };
                $scope.teamB = { name: "" };

                $scope.games = [1, 2, 3, 4, 5].map(function(index) {
                    return { gameIndex: index, caption: 'Game ' + index, scores: { teamA: null, teamB: null } };
                });
            }

            function submit() {
                var result = { otherPlayer: $scope.teamB.name, scores: $scope.games };
                scoreService.post({
                    otherPlayer: $scope.teamB.name,
                    scores: $scope.games
                }).success(function (data, status) {
                    reset();
                }).error(function (data, status) {
                    alert("Failed!");
                });
            }
        }
    ]);
})();