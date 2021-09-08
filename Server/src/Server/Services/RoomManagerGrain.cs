using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using game;
using Microsoft.Extensions.Logging;

namespace Server.Services
{
    public class RoomManagerGrain : Orleans.Grain, IRoomManagerGrain
    {
        private readonly ILogger<RoomManagerGrain> _logger;
        private List<game.Room> _rooms;
        public RoomManagerGrain(ILogger<RoomManagerGrain> logger)
        {
            _logger = logger;
            _rooms = new();
        }
        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }
        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }
        

        ValueTask IRoomManagerGrain.CreateAsync(string room)
        {
            _rooms.Add(new() { Name = room });
            return ValueTask.CompletedTask;
        }

        ValueTask IRoomManagerGrain.DestroyAsync(string room)
        {
            _rooms.RemoveAll(t => t.Name == room);
            return ValueTask.CompletedTask;
        }

        ValueTask<ImmutableList<game.Room>> IRoomManagerGrain.GetListAsync()
        {
            return ValueTask.FromResult( _rooms.ToImmutableList());
        }
    }

}
