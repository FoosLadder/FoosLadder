(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

    function buildUrl(baseUrl, path) {
        return baseUrl + path;
    }
    //TODO Decide on structure of services and align all of them. 
    //TODO align casing I.e. its hard to tell if what you are calling is variable or method if they have the same casing
    foosLadderServices.factory('UserAccountService', ['$http', '$q', function ($http, $q) {
        var service = {
            currentUserDetails: currentUserDetails
        };

        function currentUserDetails() {
            var deferred = $q.defer();
            var userDetails = {
                fullName: 'My name',
                userName: 'MyUserName',
                userId: 1
            };
            deferred.resolve(userDetails);

            return deferred.promise;
        };

        return service;
    }]);

    foosLadderServices.factory('PlayerService', ['$http', '$q', 'apiBaseUrl', function ($http, $q, apiBaseUrl) {
        var playerService = {};
        var players = [];
        playerService.getAll = function () {
            var deferred = $q.defer();
            if (players.length === 0) {
                $http.get(buildUrl(apiBaseUrl, 'players')).success(function(data) {
                    players = data;
                    deferred.resolve(players);
                });
            } else {
                deferred.resolve(players);
            }
            return deferred.promise;
        };

        return playerService;
    }]);

    foosLadderServices.factory('ScoreService', ['$http', 'apiBaseUrl', function ($http, apiBaseUrl) {
        var scoreService = {};
        scoreService.postCompletedMatch = function (data) {
            return $http.post(buildUrl(apiBaseUrl, 'matches/completed'), data, { headers: { 'Content-Type': 'application/json' } });
        };

        return scoreService;
    }]);

    foosLadderServices.factory('AuthService', ['$http', '$q', 'localStorageService', 'apiBaseUrl', function ($http, $q, localStorageService, apiBaseUrl) {
        var authentication = {
            isAuth: false,
            userName: ""
        }

        function logOut() {
            localStorageService.remove('authorizationData');
            authentication.isAuth = false;
            authentication.userName = "";
        }

        function login(loginData) {
            var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

            var deferred = $q.defer();

            $http.post(buildUrl(apiBaseUrl, 'token'), data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName });

                authentication.isAuth = true;
                authentication.userName = loginData.userName;

                deferred.resolve(response);

            }).error(function (err, status) {
                logOut();
                deferred.reject(err);
            });

            return deferred.promise;
        }

        function saveRegistration(registration) {
            logOut();
            return $http.post(buildUrl(apiBaseUrl + "account/register", registration)).then(function(response) {
                return response;
            });
        }

        function fillAuthData () {

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                authentication.isAuth = true;
                authentication.userName = authData.userName;
            }

        }

        return {
            saveRegistration: saveRegistration,
            login: login,
            logOut: logOut,
            fillAuthData: fillAuthData,
            authentication: authentication
        }

    }]);
})();