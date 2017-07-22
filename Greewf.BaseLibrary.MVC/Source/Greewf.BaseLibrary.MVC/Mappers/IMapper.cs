using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Greewf.BaseLibrary.MVC.Mappers
{
    public interface IMapper
    {
        object Map(object source, Type destinationType);
        Z Map<X, Z>(X source);
    }

    public interface IMapper<T, Y> : IMapper
    {
        Y Map(T source);
        T Map(Y source);
    }

}