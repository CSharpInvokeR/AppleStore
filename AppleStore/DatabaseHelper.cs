using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace AppleStore
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = ConfigHelper.GetConnectionString();

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static bool ValidateUser(string username, string password)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT u.UserID, u.Username, r.RoleName, u.FirstName, u.LastName 
                                   FROM Users u 
                                   INNER JOIN Role r ON u.RoleID = r.RoleID 
                                   WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserSession.UserID = reader.GetInt32(0);
                                UserSession.Username = reader.GetString(1);
                                UserSession.Role = reader.GetString(2);
                                UserSession.FirstName = reader.GetString(3);
                                UserSession.LastName = reader.GetString(4);
                                return true;
                            }
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка подключения к базе данных: " + ex.Message);
                }
            }
        }

        public static List<T> ExecuteReader<T>(string query, Func<SqlDataReader, T> map, SqlParameter[] parameters = null)
        {
            var results = new List<T>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(map(reader));
                        }
                    }
                }
            }
            return results;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }
    }

    public static class UserSession
    {
        public static int UserID { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static bool IsLoggedIn => !string.IsNullOrEmpty(Username);
    }
}