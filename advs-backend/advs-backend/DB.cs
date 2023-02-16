using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Newtonsoft.Json;
using System.Security.Principal;
using advs_backend.DB;

using advs_backend.JSON;

namespace advs_backend
{
    internal class Database
    {
        static public string Connection()
        {
            using (AdvsContext db = new())
            {
                var advs = db.Advs.ToList();
                List<AdvJSON> result = new();
                foreach (Adv adv in advs)
                {
                    AdvJSON advJSON = new()
                    {
                        ID = adv.AdvId,
                        Name = adv.Name,
                        Location = adv.Location,
                        Discription = adv.Discription,
                        Price = adv.Price
                    };
                    result.Add(advJSON);

                }
                return JsonConvert.SerializeObject(result);
                /*
                // получаем объекты из бд и выводим на консоль
                var users = db.Users.ToList();
                Console.WriteLine("Users list:");
                foreach (User u in users)
                {
                    Console.WriteLine($"{u.UserId}. {u.Email} - {u.Password}");
                }
                */
            }
        }
        static public string GetAdv(int ID)
        {
            using (AdvsContext db = new())
            {
                var adv = (from Advs in db.Advs
                           where Advs.AdvId == ID
                           select Advs).First();
                AdvJSON advJSON = new()
                {
                    ID = adv.AdvId,
                    Name = adv.Name,
                    Location = adv.Location,
                    Discription = adv.Discription,
                    Price = adv.Price
                };
                return JsonConvert.SerializeObject(advJSON);
            }
        }

        /*
        static async public Task<string> Connection()
        {
            var connectionString = "Host=localhost;Username=postgres;Password=123;Database=advs";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            try
            {
                string result;

                await using (var cmd = dataSource.CreateCommand("SELECT name FROM advs"))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result = reader.GetString(0);
                        Console.WriteLine(result);
                        return result;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Ошибка");
            }
            
            return "Пусто";
        }
        */

        static public string AddAdv(NewAdvJSON adv)
        {
            using (AdvsContext db = new())
            {
                Adv newAdv = new()
                {
                    Name = adv.Name,
                    Location = adv.Location,
                    Discription = adv.Discription,
                    Price = adv.Price,
                    UserId = adv.UserId
                };
                db.Advs.Add(newAdv);
                db.SaveChanges();

                AdvJSON advJSON = new()
                {
                    ID = newAdv.AdvId,
                    Name = newAdv.Name,
                    Location = newAdv.Location,
                    Discription = newAdv.Discription,
                    Price = newAdv.Price
                };
                return JsonConvert.SerializeObject(advJSON);
            }
        }
        static public string Login(string strUser)
        {
            using (AdvsContext db = new())
            {
                UserJSON userJSON = JsonConvert.DeserializeObject<UserJSON>(strUser);
                var user = (from Users in db.Users
                            where Users.Email == userJSON.Email
                            select Users).FirstOrDefault();
                Result result;

                if (user != null && userJSON.Password == user.Password)
                {
                    Console.WriteLine("User: " + "\n\tID: " + user.UserId + "\n\tEmail: " + user.Email);
                    result = new()
                    {
                        Message = "Authenticated"
                    };
                }
                else
                {
                    result = new()
                    {
                        Message = "Неверный логин или пароль!"
                    };
                }

                return JsonConvert.SerializeObject(result);
            }
        }
    }

    /*
    public class AdvsDB : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Advs> Advs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Database=advs;Username=postgres;Password=123");
    }
    
    public class Users
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
    public class Advs
    {
        public int adv_id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string discription { get; set; }
        public float price { get; set; }
        public int user_id { get; set; }
    }
*/
}