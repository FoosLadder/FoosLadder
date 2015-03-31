//TODO organise angular namespaces
(function () {
    'use strict';
    var foosLadderApp = angular.module('foosLadderApp', [
        'ngRoute',
        'foosLadderApp.Services',
        'foosLadderApp.Controllers'

    ]);

    foosLadderApp.config([
        '$routeProvider',
        function($routeProvider) {
            $routeProvider
                .when('/players', {
                    templateUrl: 'views/playerList.html',
                    controller: 'PlayerListController'
                })
                .when('/scores/submit', {
                    templateUrl: 'views/submitScores.html',
                    controller: 'SubmitScoresController'
                })
                .otherwise({
                    redirectTo: '/players'
                });
        }
    ]);
})();