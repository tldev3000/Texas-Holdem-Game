﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL;
using System.Collections.Generic;
using Moq;
using DAL;
using Backend.User;
using ApplicationFacade;
using Backend.Game;
using Backend.Game.DecoratorPreferences;
using static Backend.Game.DecoratorPreferences.GamePolicyDecPref;
using Backend;
using System;

namespace TestProject.UnitTest
{
    [TestClass]
    public class LogOutTest
    {
        private SLInterface sl;
        private GameCenter center;

        [TestInitialize]
        public void SetUp()
        {
            var userList = new List<SystemUser>
            {
                new SystemUser("Hadas", "Aa123456", "email0", "image0", 1000),
                new SystemUser("Gili", "123123", "email1", "image1", 0),
                new SystemUser("Or", "111111", "email2", "image2", 700),
                new SystemUser("Aviv", "Aa123456", "email3", "image3", 1500)
            };

            center = GameCenter.getGameCenter();

            //set users ranks.
            userList[0].rank = 10;
            userList[1].rank = 15;
            userList[2].rank = 20;
            userList[3].rank = 25;

            for (int i = 0; i < 4; i++)
            {
                userList[i].id = i;
                center.loggedInUsers.Add(userList[i]);
                //center.login(userList[i].name, userList[i].password);
            }

            //set the leagues
            center.maintainLeagues(userList);

            //get the league of user 3
            League l = center.getUserLeague(userList[3]);

            //setting the games
            //pref order: mustpref(spectate, league)->game type , buy in policy, starting chips, minimal bet, minimum players, maximum players.
            var gamesList = new List<TexasHoldemGame>
                         {
                //regular games
                new TexasHoldemGame(userList[0],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (9,null) ))))),true)),
                new TexasHoldemGame(userList[0],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (9,null) ))))),false)),
                new TexasHoldemGame(userList[1],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),true)),
                new TexasHoldemGame(userList[1],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false)),
                new TexasHoldemGame(userList[2],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false)),
                new TexasHoldemGame(userList[2],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false)),
                //league games
                new TexasHoldemGame(userList[3],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false,l.minRank,l.maxRank)),
                new TexasHoldemGame(userList[3],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false,l.minRank,l.maxRank))
            //new TexasHoldemGame(userList[0], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 9, true)),
            //    new TexasHoldemGame(userList[0], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 9, false)),
            //    new TexasHoldemGame(userList[1], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, true)),
            //    new TexasHoldemGame(userList[1], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[2], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[2], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[3], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false, 0, 1000)),
            //    new TexasHoldemGame(userList[3], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false, 1000, 2000))
                         };

            for (int i = 0; i < gamesList.Count; i++)
            {
                gamesList[i].gameId = i;
                center.texasHoldemGames.Add(gamesList[i]);
            }

            Mock<DALInterface> dalMock = new Mock<DALInterface>();
            dalMock.Setup(x => x.getAllGames()).Returns(gamesList);
            dalMock.Setup(x => x.getUserById(It.IsAny<int>())).Returns((int i) => userList[i]);
            dalMock.Setup(x => x.getGameById(It.IsAny<int>())).Returns((int i) => gamesList[i]);
            sl = new SLImpl();
        }

        [TestMethod]
        public void successTest()
        {
            SystemUser user = center.getUserById(2);
            sl.leaveGame(user, 4);
            sl.leaveGame(user, 5);
            object o = sl.Logout(user.id);
            Assert.IsInstanceOfType(o, typeof(ReturnMessage));
            Assert.IsTrue(((ReturnMessage)o).success);
        }

        [TestMethod]
        public void notLoggedInTest()
        {
            object m = sl.Logout(10);
            Assert.IsInstanceOfType(m, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)m).success);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The user is already logged in")]
        public void logOutTwiceTest()
        {
            SystemUser user = new SystemUser("rick roll", "never gonna give you up", "never@gonna.let.you.down", "a picture of something completly not related to rick roll", 200);
            user.id = 7;
            sl.Register("rick roll", "never gonna give you up", "never@gonna.let.you.down", "a picture of something completly not related to rick roll");
            sl.Login("rick roll", "never gonna give you up");
            sl.Logout(7);
            object m = sl.Logout(7);
            Assert.IsInstanceOfType(m, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)m).success);
        }

        [TestCleanup]
        public void TearDown()
        {
            center.shutDown();
        }
    }
}