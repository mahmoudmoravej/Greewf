using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Greewf.BaseLibrary.MVC.Mappers
{
    public interface IModelMapperController
    {
        IMapper ModelMapper
        {
            get;
        }
    }

    
}