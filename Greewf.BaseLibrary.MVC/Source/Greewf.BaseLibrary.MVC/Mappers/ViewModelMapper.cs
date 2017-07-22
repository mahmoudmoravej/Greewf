using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Greewf.BaseLibrary.MVC.Mappers
{
    public class ViewModelMapper<T, Y> : IMapper<T, Y>
    {
        static IMapper<T, Y> _instance;
        private Type _Ttype = typeof(T);
        private Type _Ytype = typeof(Y);

        static ViewModelMapper()
        {
            _instance = new ViewModelMapper<T, Y>();
            Mapper.CreateMap<T, Y>();
            Mapper.CreateMap<Y, T>();
        }

        public object Map(object source, Type destinationType)
        {
            if (_Ttype.IsInstanceOfType(source) && destinationType == _Ytype)
                return Map((T)source);
            else if (_Ytype.IsInstanceOfType(source) is Y && destinationType == _Ttype)
                return Map((Y)source);
            else
                return Mapper.Map(source, destinationType);
        }

        public static IMapper<T, Y> GetStaticInstance()
        {
            return _instance;
        }

        public Y Map(T source)
        {
            return Mapper.Map<T, Y>(source);
        }

        public T Map(Y source)
        {
            return Mapper.Map<Y, T>(source);
        }


        public Z Map<X, Z>(X source)
        {
            return ViewModelMapper<X, Z>.GetStaticInstance().Map(source);
        }
    }
}