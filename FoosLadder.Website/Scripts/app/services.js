(function () {
    'use strict';
    var foosLadderServices = angular.module('foosLadderApp.Services', []);

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

    foosLadderServices.factory('PlayerService', ['$http', '$q', 'appSettings', function ($http, $q, appSettings) {
        var playerService = {};
        var players = [];
        playerService.getAll = function () {
            var deferred = $q.defer();
            if (players.length === 0) {
                $http.get(appSettings.apiBaseUrl + 'players').success(function (data) {
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

    foosLadderServices.factory('ScoreService', ['$http', 'appSettings', function ($http, appSettings) {
        var scoreService = {};
        scoreService.postCompletedMatch = function (data) {
            return $http.post(appSettings.apiBaseUrl + 'matches/completed', data, { headers: { 'Content-Type': 'application/json' } });
        };

        return scoreService;
    }]);

    foosLadderServices.factory('OrdersService', ['$http', 'appSettings', function ($http, appSettings) {
        function getOrders() {
            return $http.get(appSettings.apiBaseUrl, 'orders').then(function (results) {
                return results;
            });
        }

        return {
            getOrders : getOrders
        }
    }]);

    foosLadderServices.factory('AuthService', ['$http', '$q', 'localStorageService', 'appSettings', function ($http, $q, localStorageService, appSettings) {
        var authServiceFactory = {};

        var _authentication = {
            isAuth: false,
            userName: "",
            useRefreshTokens: false
        };

        var _externalAuthData = {
            provider: "",
            userName: "",
            externalAccessToken: ""
        };

        var _saveRegistration = function (registration) {

            _logOut();

            return $http.post(appSettings.apiBaseUrl + 'account/register', registration).then(function (response) {
                return response;
            });

        };

        var _login = function (loginData) {

            var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

            if (loginData.useRefreshTokens) {
                data = data + "&client_id=" + appSettings.clientId;
            }

            var deferred = $q.defer();

            $http.post(appSettings.apiBaseUrl + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                if (loginData.useRefreshTokens) {
                    localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, refreshToken: response.refresh_token, useRefreshTokens: true });
                }
                else {
                    localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, refreshToken: "", useRefreshTokens: false });
                }
                _authentication.isAuth = true;
                _authentication.userName = loginData.userName;
                _authentication.useRefreshTokens = loginData.useRefreshTokens;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        var _logOut = function () {

            localStorageService.remove('authorizationData');

            _authentication.isAuth = false;
            _authentication.userName = "";
            _authentication.useRefreshTokens = false;

        };

        var _fillAuthData = function () {

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                _authentication.isAuth = true;
                _authentication.userName = authData.userName;
                _authentication.useRefreshTokens = authData.useRefreshTokens;
            }

        };

        var _refreshToken = function () {
            var deferred = $q.defer();

            var authData = localStorageService.get('authorizationData');

            if (authData) {

                if (authData.useRefreshTokens) {

                    var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + appSettings.clientId;

                    localStorageService.remove('authorizationData');

                    $http.post(appSettings.apiBaseUrl + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                        localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });

                        deferred.resolve(response);

                    }).error(function (err, status) {
                        _logOut();
                        deferred.reject(err);
                    });
                }
            }

            return deferred.promise;
        };

        var _obtainAccessToken = function (externalData) {

            var deferred = $q.defer();

            $http.get(appSettings.apiBaseUrl + 'account/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        var _registerExternal = function (registerExternalData) {

            var deferred = $q.defer();

            $http.post(appSettings.apiBaseUrl + 'account/registerexternal', registerExternalData).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        authServiceFactory.saveRegistration = _saveRegistration;
        authServiceFactory.login = _login;
        authServiceFactory.logOut = _logOut;
        authServiceFactory.fillAuthData = _fillAuthData;
        authServiceFactory.authentication = _authentication;
        authServiceFactory.refreshToken = _refreshToken;

        authServiceFactory.obtainAccessToken = _obtainAccessToken;
        authServiceFactory.externalAuthData = _externalAuthData;
        authServiceFactory.registerExternal = _registerExternal;

        return authServiceFactory;
    }]);

    foosLadderServices.factory('AuthInterceptorService', ['$q', '$injector', '$location', 'localStorageService', function ($q, $injector, $location, localStorageService) {

        var authInterceptorServiceFactory = {};

        var _request = function (config) {

            config.headers = config.headers || {};

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                config.headers.Authorization = 'Bearer ' + authData.token;
            }

            return config;
        }

        var _responseError = function (rejection) {
            if (rejection.status === 401) {
                var authService = $injector.get('authService');
                var authData = localStorageService.get('authorizationData');

                if (authData) {
                    if (authData.useRefreshTokens) {
                        $location.path('/refresh');
                        return $q.reject(rejection);
                    }
                }
                authService.logOut();
                $location.path('/login');
            }
            return $q.reject(rejection);
        }

        authInterceptorServiceFactory.request = _request;
        authInterceptorServiceFactory.responseError = _responseError;

        return authInterceptorServiceFactory;
    }]);


    foosLadderServices.factory('TokensManagerService', ['$http', 'appSettings', function ($http, appSettings) {

        var tokenManagerServiceFactory = {};

        var _getRefreshTokens = function () {

            return $http.get(appSettings.apiBaseUrl + 'refreshtokens').then(function (results) {
                return results;
            });
        };

        var _deleteRefreshTokens = function (tokenid) {

            return $http.delete(appSettings.apiBaseUrl + 'refreshtokens/?tokenid=' + tokenid).then(function (results) {
                return results;
            });
        };

        tokenManagerServiceFactory.deleteRefreshTokens = _deleteRefreshTokens;
        tokenManagerServiceFactory.getRefreshTokens = _getRefreshTokens;

        return tokenManagerServiceFactory;

    }]);


})();