using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain
{
    public class GameEntity
    {
        [BsonElement]
        private readonly List<Player> players;

        public GameEntity(int turnsCount)
            : this(Guid.Empty, GameStatus.WaitingToStart, turnsCount, 0, new List<Player>())
        {
        }

        [BsonConstructor]
        public GameEntity(Guid id, GameStatus status, int turnsCount, int currentTurnIndex, List<Player> players)
        {
            Id = id;
            Status = status;
            TurnsCount = turnsCount;
            CurrentTurnIndex = currentTurnIndex;
            this.players = players;
        }

        public Guid Id
        {
            get;
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local For MongoDB
            private set;
        }

        public IReadOnlyList<Player> Players => players.AsReadOnly();

        public int TurnsCount { get; }

        public int CurrentTurnIndex { get; private set; }

        public GameStatus Status { get; private set; }

        public void AddPlayer(UserEntity user)
        {
            if (Status != GameStatus.WaitingToStart)
                throw new ArgumentException(Status.ToString());
            players.Add(new Player(user.Id, user.Login));
            if (Players.Count == 2)
                Status = GameStatus.Playing;
        }

        public bool IsFinished()
        {
            return CurrentTurnIndex >= TurnsCount
                   || Status == GameStatus.Finished
                   || Status == GameStatus.Canceled;
        }

        public void Cancel()
        {
            if (!IsFinished())
                Status = GameStatus.Canceled;
        }

        public bool HaveDecisionOfEveryPlayer => Players.All(p => p.Decision.HasValue);

        public void SetPlayerDecision(Guid userId, PlayerDecision decision)
        {
            if (Status != GameStatus.Playing)
                throw new InvalidOperationException(Status.ToString());
            foreach (var player in Players.Where(p => p.UserId == userId))
            {
                if (player.Decision.HasValue)
                    throw new InvalidOperationException(player.Decision.ToString());
                player.Decision = decision;
            }
        }

        public GameTurnEntity FinishTurn()
        {
            var winnerId = Guid.Empty;
            for (int i = 0; i < 2; i++)
            {
                var player = Players[i];
                var opponent = Players[1 - i];
                if (!player.Decision.HasValue || !opponent.Decision.HasValue)
                    throw new InvalidOperationException();
                if (player.Decision.Value.Beats(opponent.Decision.Value))
                {
                    player.Score++;
                    winnerId = player.UserId;
                }
            }
            //TODO Заполнить все внутри GameTurnEntity, в том числе winnerId
            var result = new GameTurnEntity();
            // Это должно быть после создания GameTurnEntity
            foreach (var player in Players)
                player.Decision = null;
            CurrentTurnIndex++;
            if (CurrentTurnIndex == TurnsCount)
                Status = GameStatus.Finished;
            return result;
        }
    }
}