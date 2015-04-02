
//TODO Switch from callbacks to promises
(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers', []);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.GetAll(function (data) {
                $scope.players = data;
                $scope.orderProp = '-totalMatchesWon';
            });
        }
    ]);

    foosLadderControllers.controller('SubmitScoresController', [
        '$scope', 'ScoreService',
        function ($scope, scoreService) {
            $scope.submit = submit;

            activate();

            function activate() {
                $scope.teamA = { name: "Your name", id: 2 };
                $scope.teamB = { name: "", id: 3 };

                $scope.games = [1, 2, 3, 4, 5].map(function(index) {
                    return { index: index, caption: 'Game ' + index, teamA: null, teamB: null };
                });
            }

            function submit() {
                var timestamp = new Date();
                var challengedBy = 2;
                var acceptedBy = 2;

                var winner = $scope.teamA.id;
                var loser = $scope.teamB.id;

                var result = {
                    playerA: $scope.teamA.id,
                    playerBs: $scope.teamB.id,
                    matchDate: timestamp,
                    challenged: { by: challengedBy, at: timestamp },
                    accepted: { by: acceptedBy, at: timestamp },
                    gameResults: $scope.games,
                    submitted: { by: $scope.teamA.id, at: timestamp },
                    verified: { by: $scope.teamA.id, at: timestamp },
                    winner: winner,
                    loser: loser

                }


                scoreService.postCompletedMatch(result).success(function (data, status) {
                    debugger;
                }).error(function (data, status) {
                    alert("Failed!");
                });
            }
        }
    ]);
})();