﻿namespace Backend.Game
{
	public class GamePreferences
	{
		public enum GameTypePolicy { limit, no_limit, pot_limit };
		public GameTypePolicy GamePolicy { get; }
		public int BuyInPolicy { get; }
		public int StartingChipsAmount { get; }
		public int MinimalBet { get; }
		public int MinPlayers { get; }
		public int MaxPlayers { get; }
		public bool IsSpectatingAllowed { get; }

		public GamePreferences(GameTypePolicy gamePolicy, int BuyInPolicy, int startingChipsAmount, int minimalBet, int minPlayers, int maxPlayers, bool isSpectatingAllowed)
		{
			this.GamePolicy = gamePolicy;
			this.BuyInPolicy = BuyInPolicy;
			this.StartingChipsAmount = startingChipsAmount;
			this.MinimalBet = minimalBet;
			this.MinPlayers = minPlayers;
			this.MaxPlayers = maxPlayers;
			this.IsSpectatingAllowed = isSpectatingAllowed;
		}

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(GamePreferences))
                return false;
            GamePreferences pref = (GamePreferences)obj;
            return (GamePolicy == pref.GamePolicy && BuyInPolicy == pref.BuyInPolicy && StartingChipsAmount == pref.StartingChipsAmount && MinimalBet == pref.MinimalBet
                && MinPlayers == pref.MinPlayers && MaxPlayers == pref.MaxPlayers && IsSpectatingAllowed == pref.IsSpectatingAllowed);
        }
    }
}