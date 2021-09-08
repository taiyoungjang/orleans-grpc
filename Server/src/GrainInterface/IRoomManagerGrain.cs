using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IRoomManagerGrain : IGrainWithIntegerKey
{
    ValueTask CreateAsync(string room);
    ValueTask DestroyAsync(string room);
    ValueTask<ImmutableList<game.Room>> GetListAsync();

}
