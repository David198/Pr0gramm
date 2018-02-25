using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.HockeyApp;
using Pr0gramm.Helpers;
using Pr0gramm.Models.Enums;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Services
{
    public class UserSyncService
    {
        private readonly IProgrammApi _programmApi;
        private readonly SettingsService _settingsService;
        private readonly CacheVoteService _voteService;
        private const string UserSyncOffsetKey = "UserSyncOffset";

        public UserSyncService(IProgrammApi programmApi, SettingsService settingsService, CacheVoteService voteService)
        {
            _programmApi = programmApi;
            _settingsService = settingsService;
            _voteService = voteService;
            LoadUserSyncOffsetAsync();
        }

        public async Task Sync()
        {
            var usersync  = await _programmApi.UserSync(UserSyncOffset);
            if (string.IsNullOrEmpty(usersync.Log)) return;
            byte[] data = Convert.FromBase64String(usersync.Log);
            if (data.Length % 5 == 0)
            {
                    var logByteList = data.ToList();
                    for (int i = 0; i < logByteList.Count; i += 5)
                    {
                        int id = BitConverter.ToInt32(new[] { logByteList[i], logByteList[i + 1], logByteList[i + 2], logByteList[i + 3] }, 0);
                        var type = Convert.ToInt32(logByteList[i + 4]);
                        VoteAction voteAction = MapLogItemToVoteAction(type);
                       _voteService.SaveVote(voteAction.Type, voteAction.Vote, id);
                    }
                SaveUserSyncOffsetAsync(usersync.LogLength);
                HockeyClient.Current.TrackException(new Exception("Length of vote log must be a multiple of 5"));
            }
        }

        private VoteAction MapLogItemToVoteAction(int type)
        {
            switch (type)
            {
                case 0:
                    return new VoteAction(CacheVoteType.Item, Vote.Down);
                case 1:
                    return new VoteAction(CacheVoteType.Item, Vote.Neutral);
                case 2:
                    return new VoteAction(CacheVoteType.Item, Vote.Up);
                case 3:
                    return new VoteAction(CacheVoteType.Comment, Vote.Down);
                case 4:
                    return new VoteAction(CacheVoteType.Comment, Vote.Down);
                case 5:
                    return new VoteAction(CacheVoteType.Comment, Vote.Neutral);
                case 6:
                    return new VoteAction(CacheVoteType.Item, Vote.Up);
                case 7:
                    return new VoteAction(CacheVoteType.Tag, Vote.Down);
                case 8:
                    return new VoteAction(CacheVoteType.Tag, Vote.Neutral);
                case 9:
                    return new VoteAction(CacheVoteType.Tag, Vote.Up);
                case 10:
                    return new VoteAction(CacheVoteType.Item, Vote.Favorite);
                default:
                    break;
            }
            return null;
        }

        public int UserSyncOffset { get; set; }

        private async void LoadUserSyncOffsetAsync()
        {
            string userSyncOffSetTempStr =
                await ApplicationData.Current.LocalSettings.ReadAsync<string>(UserSyncOffsetKey);
            if (!string.IsNullOrEmpty(userSyncOffSetTempStr))
            {
                if (int.TryParse(userSyncOffSetTempStr, out int userSyncOffsetKey))
                    UserSyncOffset = userSyncOffsetKey;
            }
        }

        public async void SaveUserSyncOffsetAsync(int newOffsett)
        {
            UserSyncOffset = newOffsett;
            await ApplicationData.Current.LocalSettings.SaveAsync(UserSyncOffsetKey, UserSyncOffset);
        }

        private class VoteAction
        {
            public CacheVoteType Type { get; }
            public Vote Vote { get; }

            public VoteAction(CacheVoteType type, Vote vote)
            {
                Type = type;
                Vote = vote;
            }
        }

        public void ResetOffset()
        {
             SaveUserSyncOffsetAsync(0);
             _voteService.ClearDataBase();
        }
    }
}
