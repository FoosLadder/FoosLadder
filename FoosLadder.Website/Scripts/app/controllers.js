(function () {
    'use strict';
    var foosLadderControllers = angular.module('foosLadderApp.Controllers', []);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.GetAll().then(function(data) {
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
                $scope.playerA = { name: "Your name", id: 2 };
                $scope.playerB = { name: "", id: 3 };

                $scope.games = [1, 2, 3, 4, 5].map(function(index) {
                    return { index: index, caption: 'Game ' + index, playerA: null, playerB: null };
                });
            }

            function submit() {
                var timestamp = new Date();
                var challengedBy = 2;
                var acceptedBy = 2;

                var winner = $scope.playerA.id;
                var loser = $scope.playerB.id;

                var result = {
                    playerA: $scope.playerA.id,
                    playerB: $scope.playerB.id,
                    matchDate: timestamp,
                    challenged: { by: challengedBy, at: timestamp },
                    accepted: { by: acceptedBy, at: timestamp },
                    gameResults: $scope.games.map(function(game) {
                        return { index: game.index, playerA: game.playerA, playerB: game.playerB };
                    }),
                    submitted: { by: $scope.playerA.id, at: timestamp },
                    verified: { by: $scope.playerA.id, at: timestamp },
                    winner: winner,
                    loser: loser,
                    id: 11

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