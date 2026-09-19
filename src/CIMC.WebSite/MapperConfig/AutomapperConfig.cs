using AutoMapper;
using CIMC.Data;

namespace MySite.Web.Models.MapperConfig
{
    /// <summary>保存AutoMapper的配置。</summary>
    public class AutoMapperConfig : Profile
    {
        /// <summary>初始化AutoMapper。</summary>
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
