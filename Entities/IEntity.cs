using System;
using System.Collections.Generic;
using System.Text;

namespace shared.Entities
{
    public interface IEntity
    {
        uint id { get; }
        byte[] jsonCached{get;}

    }
}
