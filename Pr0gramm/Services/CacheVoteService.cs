using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Pr0gramm.Models;
using Pr0gramm.Models.Enums;

namespace Pr0gramm.Services
{
    public class CacheVoteService
    {
        private const string DbName = "Filename=Pr0gramm.db";

        public CacheVoteService()
        {
            PrepareDataBase();
        }

        private void PrepareDataBase()
        {
            using (SqliteConnection db = new SqliteConnection(DbName))
            {
                db.Open();
                String tableCommand =
                    "CREATE TABLE IF NOT EXISTS cached_vote (id INTEGER PRIMARY KEY, type TEXT, vote TEXT, item_id INTEGER)";
                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();
            }
        }

        public void ClearDataBase()
        {
            using (SqliteConnection db = new SqliteConnection(DbName))
            {
                db.Open();
                String tableCommand = "DELETE FROM cached_vote";
                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();

            }
        }

        public async void SaveVote(CacheVoteType voteActionType, Vote voteActionVote, int itemId)
        {
            using (SqliteConnection db = new SqliteConnection(DbName))
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;
                insertCommand.CommandText = " INSERT OR REPLACE INTO cached_vote VALUES (@id, @type, @vote, @item_id);";
                insertCommand.Parameters.AddWithValue("@id", VoteId(voteActionType, itemId));
                insertCommand.Parameters.AddWithValue("@item_id", itemId);
                insertCommand.Parameters.AddWithValue("@type", voteActionType.ToString());
                insertCommand.Parameters.AddWithValue("@vote", voteActionVote.ToString());
                await insertCommand.ExecuteReaderAsync();
                db.Close();
            }
        }

        public async Task<CachedVote> Find(CacheVoteType voteActionType, int itemId)
        {
            CachedVote cachedVote = null;
            using (SqliteConnection db = new SqliteConnection(DbName))
            {
                db.Open();
                SqliteCommand selectCommand =
                    new SqliteCommand("SELECT item_id, type, vote FROM cached_vote WHERE id=@id", db);
                selectCommand.Parameters.AddWithValue("@id", VoteId(voteActionType, itemId));
                var query = await selectCommand.ExecuteReaderAsync();
                while (await query.ReadAsync())
                {
                    cachedVote = new CachedVote(query.GetInt32(0), query.GetString(1), query.GetString(2));
                }

                db.Close();
            }
            return cachedVote;
        }


        private int VoteId(CacheVoteType voteActionType, int itemId)
        {
            return itemId * 10 + (int) voteActionType;
        }
    }
}
