﻿using FootballServerCapstone.Core.Interfaces.DAL;
using FootballServerCapstone.Core.DTOs;
using FootballServerCapstone.Core;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FootballServerCapstone.DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private DbFactory _db;
        public ReportRepository(DbFactory db)
        {
            _db = db;
        }
        public Response<List<ClubRecord>> getClubRecords()
        {
            Response<List<ClubRecord>> result = new Response<List<ClubRecord>>();
            result.Message = new List<string>();

            using (var conn = new SqlConnection(_db.GetConnectionString()))
            {
                //Set up SQL commands
                SqlCommand getWins = new SqlCommand("ClubWins", conn);
                getWins.CommandType = CommandType.StoredProcedure;
                getWins.Parameters.Add("@ClubId", SqlDbType.Int);
                SqlCommand getLosses = new SqlCommand("ClubLosses", conn);
                getLosses.CommandType = CommandType.StoredProcedure;
                getLosses.Parameters.Add("@ClubId", SqlDbType.Int);
                SqlCommand getTies = new SqlCommand("ClubTies", conn);
                getTies.CommandType = CommandType.StoredProcedure;
                getTies.Parameters.Add("@ClubId", SqlDbType.Int);
                SqlCommand getClubs = new SqlCommand("SELECT ClubId, [Name] FROM Club", conn);


                try
                {
                    conn.Open();
                }

                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message.Add(ex.Message);
                    return result;
                }

                result.Data = new List<ClubRecord>();
                //Get all clubs
                using (var reader = getClubs.ExecuteReader()) {
                    while (reader.Read())
                    {
                        ClubRecord temp = new ClubRecord();
                        temp.ClubId = (int)reader["ClubId"];
                        temp.Name = reader["Name"].ToString();

                        result.Data.Add(temp);
                    }
                }

                //Get Record Data for all clubs
                for(int i=0; i<result.Data.Count; i++)
                {
                    getWins.Parameters["@ClubId"].Value = result.Data[i].ClubId;
                    getLosses.Parameters["@ClubId"].Value = result.Data[i].ClubId;
                    getTies.Parameters["@ClubId"].Value = result.Data[i].ClubId;

                    using (var reader = getWins.ExecuteReader())
                    {
                        if (!reader.Read()) {
                            result.Success = false;
                            result.Message.Add("Unable to read data");
                            return result;

                        }
                        if (reader.IsDBNull(0)) result.Data[i].Wins = 0;
                        else result.Data[i].Wins = (int)reader[0];
                    }
                    using (var reader = getLosses.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            result.Success = false;
                            result.Message.Add("Unable to read data");
                            return result;
                        }
                        if (reader.IsDBNull(0)) result.Data[i].Losses = 0;
                        else result.Data[i].Losses = (int)reader[0];
                    }
                    using (var reader = getTies.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            result.Success = false;
                            result.Message.Add("Unable to read data");
                            return result;

                        }
                        if (reader.IsDBNull(0)) result.Data[i].Draws = 0;
                        else result.Data[i].Draws = (int)reader[0];
                    }

                    result.Data[i].Points = result.Data[i].Wins * 3 + result.Data[i].Draws;
                    
                }
                result.Data = result.Data.OrderByDescending(x => x.Points).ToList();
            }

            result.Success = true;
            return result;
        }

        public Response<List<MostCleanSheets>> getMostCleanSheets(int SeasonId)
        {
            Response<List<MostCleanSheets>> result = new();
            result.Message = new List<string>();

            using (var conn = new SqlConnection(_db.GetConnectionString()))
            {
                var cmd = new SqlCommand("MostCleanSheetsForSeason", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@seasonId", SeasonId);
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message.Add(ex.Message);
                    return result;
                }

                result.Data = new List<MostCleanSheets>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MostCleanSheets record = new MostCleanSheets();
                        record.SeasonId = (int)reader["SeasonId"];
                        record.ClubName =  reader["ClubName"].ToString();
                        record.PlayerName = reader["PlayerName"].ToString();
                        record.TotalCleanSheets = (int)reader["TotalCleanSheet"];

                        result.Data.Add(record);
                    };
                }
            }
            result.Success = true;
            return result;
        }

        public Response<PlayerStatistics> getPlayerStatistics(int PlayerId, int SeasonId)
        {
            Response<PlayerStatistics> result = new Response<PlayerStatistics>();
            result.Message = new List<string>();

            using (var conn = new SqlConnection(_db.GetConnectionString()))
            {
                var cmd = new SqlCommand("PlayerStatsBySeason", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@seasonId", SeasonId);
                cmd.Parameters.AddWithValue("@playerId", PlayerId);
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message.Add(ex.Message);
                    return result;
                }
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result.Data = new PlayerStatistics(
                            (int)reader["TotalShots"],
                            (int)reader["TotalShotsOnTarget"],
                            (int)reader["TotalFouls"],
                            (int)reader["TotalGoals"],
                            (int)reader["TotalAssists"],
                            (int)reader["TotalSaves"],
                            (int)reader["TotalPasses"],
                            (int)reader["TotalPassesCompleted"],
                            (int)reader["TotalDribbles"],
                            (int)reader["TotalDribblesSucceeded"],
                            (int)reader["TotalTackles"],
                            (int)reader["TotalTackledSucceeded"],
                            (int)reader["TotalCleanSheet"]
                        );
                    }
                    else
                    {
                        result.Data = new PlayerStatistics(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    }
                }
            }
            result.Success = true;
            return result;
        }

        public Response<List<TopAssists>> getTopAssists(int SeasonId)
        {
            Response<List<TopAssists>> result = new();
            result.Message = new List<string>();

            using (var conn = new SqlConnection(_db.GetConnectionString()))
            {
                var cmd = new SqlCommand("TopAssistsForSeason", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@seasonId", SeasonId);
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message.Add(ex.Message);
                    return result;
                }

                result.Data = new List<TopAssists>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TopAssists record = new TopAssists();
                        record.SeasonId = (int)reader["SeasonId"];
                        record.ClubName = reader["ClubName"].ToString();
                        record.PlayerName = reader["PlayerName"].ToString();
                        record.TotalAssists = (int)reader["TotalAssists"];

                        result.Data.Add(record);
                    };
                }
            }
            result.Success = true;
            return result;
        }

        public Response<List<TopScorer>> getTopScorer(int SeasonId)
        {
            Response<List<TopScorer>> result = new();
            result.Message = new List<string>();

            using (var conn = new SqlConnection(_db.GetConnectionString()))
            {
                var cmd = new SqlCommand("TopScorerForSeason", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@seasonId", SeasonId);
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message.Add(ex.Message);
                    return result;
                }

                result.Data = new List<TopScorer>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TopScorer record = new TopScorer();
                        record.SeasonId = (int)reader["SeasonId"];
                        record.ClubName = reader["ClubName"].ToString();
                        record.PlayerName = reader["PlayerName"].ToString();
                        record.TotalGoals = (int)reader["TotalGoals"];

                        result.Data.Add(record);
                    };
                }
            }
            result.Success = true;
            return result;
        }
    }
}
