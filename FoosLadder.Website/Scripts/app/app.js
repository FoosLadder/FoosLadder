(function () {
    'use strict';
    var foosLadderApp = angular.module('foosLadderApp', [
        'ngRoute',
        'LocalStorageModule',
        'angular-loading-bar',
        'foosLadderApp.Services',
        'foosLadderApp.Controllers'

    ]);

    foosLadderApp.config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider.when("/login", {
                templateUrl: "views/login.html",
                controller: "loginController"
            });

            $routeProvider.when("/signup", {
                templateUrl: "views/signup.html",
                controller: "signupController"
            });

            $routeProvider.when("/orders", {
                templateUrl: "views/orders.html",
                controller: "ordersController"
            });

            $routeProvider.when('/players', {
                templateUrl: 'views/playerList.html',
                controller: 'PlayerListController'
            });

            $routeProvider.when('/scores/submit', {
                templateUrl: 'views/submitScores.html',
                controller: 'SubmitScoresController'
            });

            $routeProvider.otherwise({
                redirectTo: '/players'
            });
        }
    ]);

    foosLadderApp.run(['AuthService', function (authService) {
        authService.fillAuthData();
    }]);
})();