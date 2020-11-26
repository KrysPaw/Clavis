using Clavis.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Data;
using PagedList;

namespace Clavis.Data
{
    public class DbData
    {
        private readonly IConfiguration configuration;
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection con = new SqlConnection();

        public DbData(IConfiguration config)
        {
            configuration = config;
            con.ConnectionString = configuration.GetConnectionString("MyConnection");
        }
        public List<Room> getRooms(string _numer,int _miejsca, string sort)
        {
            
            try
            {
                List<Room> result = new List<Room>();
                con.Open();
                //todo
                if (con.State == ConnectionState.Closed)
                {
                    return null;
                }
                com.Connection = con;
                //
                com.Parameters.AddWithValue("num",_numer);
                com.Parameters.AddWithValue("am", _miejsca);
                com.CommandText = "SELECT * FROM rooms WHERE numer LIKE '%' + @num + '%' AND miejsca >= @am ";
                switch (sort)
                {
                    case "numUp": com.CommandText += " ORDER BY numer ASC"; break;
                    case "numDown": com.CommandText += " ORDER BY numer DESC"; break;
                    case "mieUp": com.CommandText += " ORDER BY miejsca ASC"; break;
                    case "mieDown": com.CommandText += " ORDER BY miejsca DESC"; break;
                }
                Debug.WriteLine(com.CommandText);
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    result.Add(new Room() { numer = dr["numer"].ToString(), 
                        opis = dr["opis"].ToString(), 
                        miejsca = int.Parse(dr["miejsca"].ToString()),
                        uwagi = dr["uwagi"].ToString() });
                }
                con.Close();
                if (result.Count > 0)
                    return result;
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

    }
}
