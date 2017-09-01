using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using shared.Services;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace shared.Entities
{
    public class User : IEntity
    {
        [JsonIgnore]
        public bool Valid => id != 0
            && !string.IsNullOrEmpty(email) && valid.MaxLenghth(email, 100) && valid.EMail(email)
            && !string.IsNullOrEmpty(first_name) && valid.MaxLenghth(first_name, 50)
            && !string.IsNullOrEmpty(last_name) && valid.MaxLenghth(last_name, 50)
            && !string.IsNullOrEmpty(gender) && valid.MaxLenghth(gender, 1) && valid.Expr(gender, "^[mf]$")
            && valid.Range(birth_date, -1262304000, 915148800)
            ;


        //        id - уникальный внешний идентификатор пользователя.Устанавливается тестирующей системой и используется затем, для проверки ответов сервера. 32-разрядное целое число.
        public uint id { get; set; }

        //email - адрес электронной почты пользователя. Тип - unicode-строка длиной до 100 символов.Гарантируется уникальность.
        [MaxLength(100)]
        [EmailAddress]
        [Required]
        public string email;
        //first_name и last_name - имя и фамилия соответственно. Тип - unicode-строки длиной до 50 символов.
        [Required]
        [MaxLength(50)]
        public string first_name;
        [Required]
        [MaxLength(50)]
        public string last_name;
        //gender - unicode-строка "m" означает мужской пол, а "f" - женский.
        [Required]
        [MaxLength(1)]
        [RegularExpression("^[mf]$")]
        public string gender;
        //birth_date - дата рождения, записанная как число секунд от начала UNIX-эпохи по UTC (другими словами - это timestamp). Ограничено снизу 01.01.1930 и сверху 01.01.1999-ым.
        [Required]
        [Range(-1262304000, 915148800)]
        public int birth_date = int.MaxValue;

        [JsonIgnore]
        public byte[] jsonCached {get; set;}

        //[NonSerialized]
        //public DateTimeOffset birthDate => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(birth_date);

        public void updateFrom(JObject val)
        {
            foreach (var prop in val)
            {
                switch (prop.Key) {
                    case "email":
                        this.email = prop.Value.Value<string>();
                        break;
                    case "first_name":
                        this.first_name = prop.Value.Value<string>();
                        break;
                    case "last_name":
                        this.last_name = prop.Value.Value<string>();
                        break;
                    case "gender":
                        this.gender = prop.Value.Value<string>();
                        break;
                    case "birth_date":
                        this.birth_date = prop.Value.Value<int>();
                        break;
                }
            }
            jsonCached = Encoding.UTF8.GetBytes(JsonSerializers.Serialize(this));

        }

        List<Visit> visits;

        static readonly Visit[] empty = new Visit[0];
        public IEnumerable<Visit> GetVisits()
        {
            if (visits != null) return visits;
            else return empty;
        }
        public void AddVisit(Visit visit)
        {
            if (visits == null) lock (this) if (visits == null) visits = new List<Visit>(15);
            visits.Add(visit);
        }
        public void RemoveVisit(Visit visit){
            visits.Remove(visit);
        }

    }
}
