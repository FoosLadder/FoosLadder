(function () {
    'use strict';
    //TODO rename new controllers
    var foosLadderControllers = angular.module('foosLadderApp.Controllers', []);

    foosLadderControllers.controller('PlayerListController', [
        '$scope', 'PlayerService',
        function ($scope, playerService) {
            playerService.getAll().then(function (data) {
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

                $scope.games = [1, 2, 3, 4, 5].map(function (index) {
                    return { index: index, caption: 'Game ' + index, playerA: null, playerB: null };
                });

                userAccountService.currentUserDetails().then(function (userDetails) {
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

    foosLadderControllers.controller('AssociateController', ['$scope', '$location', '$timeout', 'AuthService', function ($scope, $location, $timeout, authService) {

        $scope.savedSuccessfully = false;
        $scope.message = "";

        $scope.registerData = {
            userName: authService.externalAuthData.userName,
            provider: authService.externalAuthData.provider,
            externalAccessToken: authService.externalAuthData.externalAccessToken
        };

        $scope.registerExternal = function () {

            authService.registerExternal($scope.registerData).then(function (response) {

                $scope.savedSuccessfully = true;
                $scope.message = "User has been registered successfully, you will be redicted to orders page in 2 seconds.";
                startTimer();

            },
              function (response) {
                  var errors = [];
                  for (var key in response.modelState) {
                      errors.push(response.modelState[key]);
                  }
                  $scope.message = "Failed to register user due to:" + errors.join(' ');
              });
        };

        var startTimer = function () {
            var timer = $timeout(function () {
                $timeout.cancel(timer);
                $location.path('/orders');
            }, 2000);
        }

    }]);

    foosLadderControllers.controller('IndexController', ['$scope', '$location', "AuthService", function ($scope, $location, authService) {

        $scope.logOut = function () {
            authService.logOut();
            $location.path('/players');
        }

        $scope.authentication = authService.authentication;
    }]);

    foosLadderControllers.controller('LoginController', ['$scope', '$location', 'AuthService', 'appSettings', function ($scope, $location, authService, appSettings) {

        $scope.loginData = {
            userName: "",
            password: "",
            useRefreshTokens: false
        };

        $scope.message = "";

        $scope.login = function () {

            authService.login($scope.loginData).then(function (response) {

                $location.path('/orders');

            },
             function (err) {
                 $scope.message = err.error_description;
             });
        };

        $scope.authExternalProvider = function (provider) {

            var redirectUri = location.protocol + '//' + location.host + '/authcomplete.html';

            var externalProviderUrl = appSettings.apiBaseUrl + "account/ExternalLogin?provider=" + provider
                                                                        + "&response_type=token&client_id=" + appSettings.clientId
                                                                        + "&redirect_uri=" + redirectUri;
            window.$windowScope = $scope;

            var oauthWindow = window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
        };

        $scope.authCompletedCB = function (fragment) {

            $scope.$apply(function () {

                if (fragment.haslocalaccount == 'False') {

                    authService.logOut();

                    authService.externalAuthData = {
                        provider: fragment.provider,
                        userName: fragment.external_user_name,
                        externalAccessToken: fragment.external_access_token
                    };

                    $location.path('/associate');

                }
                else {
                    //Obtain access token and redirect to orders
                    var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                    authService.obtainAccessToken(externalData).then(function (response) {

                        $location.path('/orders');

                    },
                 function (err) {
                     $scope.message = err.error_description;
                 });
                }

            });
        }
    }]);

    foosLadderControllers.controller('OrdersController', ['$scope', 'OrdersService', function ($scope, ordersService) {

        $scope.orders = [];

        ordersService.getOrders().then(function (results) {

            $scope.orders = results.data;

        }, function (error) {
            //alert(error.data.message);
        });

    }]);

    foosLadderControllers.controller('RefreshController', ['$scope', '$location', 'AuthService', function ($scope, $location, authService) {

        $scope.authentication = authService.authentication;
        $scope.tokenRefreshed = false;
        $scope.tokenResponse = null;

        $scope.refreshToken = function () {

            authService.refreshToken().then(function (response) {
                $scope.tokenRefreshed = true;
                $scope.tokenResponse = response;
            },
             function (err) {
                 $location.path('/login');
             });
        };

    }]);

    foosLadderControllers.controller('SignupController', ['$scope', '$location', '$timeout', 'AuthService', function ($scope, $location, $timeout, authService) {

        $scope.savedSuccessfully = false;
        $scope.message = "";

        $scope.registration = {
            userName: "",
            password: "",
            confirmPassword: ""
        };

        $scope.signUp = function () {

            authService.saveRegistration($scope.registration).then(function (response) {

                $scope.savedSuccessfully = true;
                $scope.message = "User has been registered successfully, you will be redicted to login page in 2 seconds.";
                startTimer();

            },
             function (response) {
                 var errors = [];
                 for (var key in response.data.modelState) {
                     for (var i = 0; i < response.data.modelState[key].length; i++) {
                         errors.push(response.data.modelState[key][i]);
                     }
                 }
                 $scope.message = "Failed to register user due to:" + errors.join(' ');
             });
        };

        var startTimer = function () {
            var timer = $timeout(function () {
                $timeout.cancel(timer);
                $location.path('/login');
            }, 2000);
        }
    }]);

    foosLadderControllers.controller('TokensManagerController', ['$scope', 'TokensManagerService', function ($scope, tokensManagerService) {

        $scope.refreshTokens = [];

        tokensManagerService.getRefreshTokens().then(function (results) {

            $scope.refreshTokens = results.data;

        }, function (error) {
            alert(error.data.message);
        });

        $scope.deleteRefreshTokens = function (index, tokenid) {

            tokenid = window.encodeURIComponent(tokenid);

            tokensManagerService.deleteRefreshTokens(tokenid).then(function (results) {

                $scope.refreshTokens.splice(index, 1);

            }, function (error) {
                alert(error.data.message);
            });
        }

    }]);



    
})();