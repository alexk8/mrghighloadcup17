using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using shared.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace shared.Entities
{
    public class Visit 
    {
        [JsonIgnore]
        public bool Valid => id != 0
            && location != uint.MaxValue
            && user != uint.MaxValue
            && visited_at != int.MaxValue
            && valid.Range(mark, 0, 5)
            && valid.Range(visited_at, 946684800, 1420070400)
            ;
        public uint id;// - уникальный внешний id посещения.Устанавливается тестирующей системой. 32-разрядное целое число.
        public uint location= uint.MaxValue;// - id достопримечательности. 32-разрядное целое число.
        public uint user= uint.MaxValue;// - id путешественника. 32-разрядное целое число.
        [Range(946684800, 1420070400)]
        public int visited_at = int.MaxValue;// - дата посещения, timestamp с ограничениями: снизу 01.01.2000, а сверху 01.01.2015.
        [Range(0,5)]
        public int mark=-1;// - оценка посещения от 0 до 5 включительно.Целое число.

        //[NonSerialized]
        [JsonIgnore]
        public Location LocationRef { get; set; }
        //[NonSerialized]
        [JsonIgnore]
        public User UserRef{ get; set; }


    public void updateFrom(JObject val,Location newLoc,User newUser)
        {
            foreach (var prop in val)
            {
                if (prop.Key=="location"){
                    uint oldVal = location;
                    location = prop.Value.Value<uint>();
                    if (oldVal != location)
                    {
                        LocationRef.RemoveVisit(this);
                        LocationRef = newLoc;
                        LocationRef.AddVisit(this);
                    }
                }  
                else if (prop.Key=="user"){
                    uint oldVal = user;
                    user = prop.Value.Value<uint>();
                    if (oldVal != user)
                    {
                        UserRef.RemoveVisit(this);
                        UserRef = newUser;
                        UserRef.AddVisit(this);
                    }
                }
                else if (prop.Key=="visited_at") visited_at = prop.Value.Value<int>();
                else if (prop.Key=="mark") mark = prop.Value.Value<int>();
            }
        }
    }
}
