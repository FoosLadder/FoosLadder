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
                controller: "LoginController"
            });

            $routeProvider.when("/signup", {
                templateUrl: "views/signup.html",
                controller: "SignupController"
            });

            $routeProvider.when("/orders", {
                templateUrl: "views/orders.html",
                controller: "OrdersController"
            });

            $routeProvider.when('/players', {
                templateUrl: 'views/playerList.html',
                controller: 'PlayerListController'
            });

            $routeProvider.when('/scores/submit', {
                templateUrl: 'views/submitScores.html',
                controller: 'SubmitScoresController'
            });

            //$routeProvider.when("/refresh", {
            //    templateUrl: "/app/views/refresh.html",
            //    controller: "refreshController"
            //});

            //$routeProvider.when("/tokens", {
            //    templateUrl: "/app/views/tokens.html",
            //    controller: "tokensManagerController"
            //});


            $routeProvider.otherwise({
                redirectTo: '/players'
            });
        }
    ]);

    foosLadderApp.config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('AuthInterceptorService');
    }])

    foosLadderApp.run(['AuthService', function (authService) {
        authService.fillAuthData();
    }]);
})();