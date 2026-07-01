using AutoMapper;
using CIMC.Data;

namespace CimcSite.Web.Models.MapperConfig
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
            CreateMap<Album, AlbumModel>().ReverseMap();
         
            CreateMap<Attachments, AttachmentsModel>().ReverseMap();
           
            CreateMap<Job, JobModel>().ReverseMap();
            CreateMap<MessageBoard, MessageBoardModel>().ReverseMap();
            CreateMap<VideoMedia, VideoMediaModel>().ReverseMap();

            CreateMap<SiteInfo, SiteInfoModel>().ReverseMap();
            CreateMap<FooterInfo, FooterModel>().ReverseMap();
            #endregion

         
        }
    }
}
