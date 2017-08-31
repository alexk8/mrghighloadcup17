using shared.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace shared.Entities
{
    public class Location 
    {
        [JsonIgnore]
        public bool Valid => id != 0 
            && !string.IsNullOrEmpty(place) 
            && !string.IsNullOrEmpty(country) && valid.MaxLenghth(country, 50)
            && !string.IsNullOrEmpty(city) && valid.MaxLenghth(city, 50)
            && distance >= 0;


        public uint id;// - уникальный внешний id достопримечательности.Устанавливается тестирующей системой. 32-разрядное целое число.
        
        [Required]
        public string place;// - описание достопримечательности. Текстовое поле неограниченной длины.
        [Required]
        [MaxLength(50)]
        public string country;// - название страны расположения.unicode-строка длиной до 50 символов.
        [Required]
        [MaxLength(50)]
        public string city;// - название города расположения.unicode-строка длиной до 50 символов.
        [Required]
        public int distance=-1;// - расстояние от города по прямой в километрах. 32-разрядное целое число.


        List<Visit> visits;


        public IEnumerable<Visit> GetVisits()
        {
            if (visits != null) return visits;
            else return new Visit[] { };
        }
        public void AddVisit(Visit visit)
        {
            if(visits==null) lock(this) if(visits==null) visits = new List<Visit>(15);
            visits.Add(visit);
        }

        public void RemoveVisit(Visit visit){
            visits.Remove(visit);
        }

        public void updateFrom(JObject val)
        {
            foreach (var prop in val)
            {
                if (prop.Key=="place") this.place = prop.Value.Value<string>();
                else if (prop.Key == "country") this.country = prop.Value.Value<string>();
                else if (prop.Key == "city") this.city = prop.Value.Value<string>();
                else if (prop.Key == "distance") this.distance = prop.Value.Value<int>();
            }
        }
    }
}
