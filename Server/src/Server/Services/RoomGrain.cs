﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public class RoomGrain : Orleans.Grain, IRoomGrain
    {
        private readonly ILogger<RoomGrain> _logger;
        private List<PlayerInfo> _playerInfos;
        public RoomGrain(ILogger<RoomGrain> logger)
        {
            _logger = logger;
            _playerInfos = new();
        }
        async public override Task OnActivateAsync()
        {
            var roomManager = this.GrainFactory.GetGrain<IRoomManagerGrain>(0);
            await roomManager.CreateAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
            await base.OnActivateAsync();
        }
        async public override Task OnDeactivateAsync()
        {
            var roomManager = this.GrainFactory.GetGrain<IRoomManagerGrain>(0);
            await roomManager.DestroyAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
            await base.OnDeactivateAsync();
        }
        async ValueTask<bool> IRoomGrain.ChatAsync(string playerName, string message)
        {
            var player = _playerInfos.FirstOrDefault(t => t.Name == playerName);
            for (int i = 0; i < _playerInfos.Count; ++i)
            {
                var playerInfo = _playerInfos[i];
                if(player == playerInfo)
                {
                    continue;
                }
                await this.GrainFactory.GetGrain<IPlayerGrain>(playerInfo.Name)
                    .OnChat(player: player.Name, room: this.GrainReference.GrainIdentity.PrimaryKeyString, message: message);
            }
            return true;
        }

        async ValueTask IRoomGrain.LeaveAsync(string player)
        {
            var leaver = _playerInfos.FirstOrDefault(t => t.Name == player);
            if(leaver != null)
            {
                _playerInfos.RemoveAll(t => t.Name == player);
                for (int i = 0; i < _playerInfos.Count; ++i)
                {
                    var playerInfo = _playerInfos[i];
                    await this.GrainFactory.GetGrain<IPlayerGrain>(playerInfo.Name)
                        .OnLeave(player: leaver.Name, room: this.GrainReference.GrainIdentity.PrimaryKeyString);
                }
            }
        }

        async ValueTask<(bool ret, List<string> players)> IRoomGrain.JoinAsync(string player, string name)
        {
            if (_playerInfos.Exists(t => t.Name == player))
            {
                return (false,null);
            }
            List<string> players = new();
            for (int i=0;i<_playerInfos.Count;++i)
            {
                var playerInfo = _playerInfos[i];
                await this.GrainFactory.GetGrain<IPlayerGrain>(playerInfo.Name)
                    .OnJoin(player: name, room: this.GrainReference.GrainIdentity.PrimaryKeyString);
                players.Add(playerInfo.Name);
            }
            _playerInfos.Add(new (name));
            return new(true,players);
        }
    }

}
