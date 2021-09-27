using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
public interface IOtpGrain : IGrainWithGuidKey
{
    ValueTask<OtpData> GetOtpAsync();
    ValueTask<OtpData> SetAccountGuidAsync(Guid accountGuid);
    ValueTask<OtpData> SetPlayerGuidAsync(long regionIndex, Guid playerGuid, string playerName);
}
