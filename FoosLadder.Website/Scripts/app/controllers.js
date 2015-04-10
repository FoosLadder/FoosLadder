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
        '$scope', '$location', 'ScoreService', 'UserAccountService',
        function ($scope, $location, scoreService, userAccountService) {
            $scope.submit = submit;
            $scope.hasErrorOccured = false;

            activate();

            function activate() {

                $scope.playerA = { name: "", id: null };
                $scope.playerB = { name: "", id: 3 };

                $scope.games = [1, 2, 3, 4, 5].map(function(index) {
                    return { index: index, caption: 'Game ' + index, playerA: null, playerB: null };
                });

                userAccountService.currentUserDetails().then(function(userDetails) {
                    $scope.playerA = { name: userDetails.fullName, id: userDetails.userId };
                });
            }

            function convertToUserAction(player, timestamp) {
                return { by: player.id, at: timestamp }
            }

            function buildMatchRecord(playerA, playerB, winner, loser, matchDate) {
                return {
                    playerA: playerA.id,
                    playerB: playerB.id,
                    matchDate: matchDate,
                    challenged: convertToUserAction(playerA, matchDate),
                    accepted: convertToUserAction(playerA, matchDate),
                    submitted: convertToUserAction(playerA, matchDate),
                    verified: convertToUserAction(playerA, matchDate),
                    gameResults: $scope.games.map(function (game) {
                        return { index: game.index, playerA: game.playerA, playerB: game.playerB };
                    }),
                    winner: winner.id,
                    loser: loser.id
                };
            }

            function submit() {
                $scope.hasErrorOccured = false;

                var timestamp = new Date();

                var winner = $scope.playerA;
                var loser = $scope.playerB;

                var matchResults = buildMatchRecord($scope.playerA, $scope.playerB, winner, loser, timestamp);

                scoreService.postCompletedMatch(matchResults).success(function (data, status) {
                    $location.path('/');
                }).error(function (data, status) {
                    $scope.hasErrorOccured = true;
                });
            }
        }
    ]);
})();