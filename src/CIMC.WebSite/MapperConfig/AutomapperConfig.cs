using AutoMapper;
using CIMC.Data;

namespace MySite.Web.Models.MapperConfig
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //admin Model
            #region admin

            CreateMap<Admin, LoginAdminModel>().ReverseMap();
            CreateMap<Menu, MenuModel>().ReverseMap();

            CreateMap<Article, ArticleModel>().ReverseMap();
            #endregion

         
        }
    }
}
