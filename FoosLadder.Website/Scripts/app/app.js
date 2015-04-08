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
                .otherwise({
                    redirectTo: '/', //Note I am explictly choosing the playerlist as the home page, but we probably want to make a proper decision about this. 
                    templateUrl: 'views/playerList.html',
                    controller: 'PlayerListController'
                });
        }
    ]);
})();