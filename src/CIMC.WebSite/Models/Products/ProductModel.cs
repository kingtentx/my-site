namespace MySite.Web.Models
{
    public class ProductModel
    {
        /// <summary>
        /// 
        /// </summary>        
        public int Id { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>       
        public string ProductName { get; set; }
        /// <summary>
        /// 产品编码（后续下单使用该字段，请保证唯一）
        /// </summary>     
        public string ProductSn { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductNo { get; set; }
        /// <summary>
        /// 产品封面图
        /// </summary>    
        public string CoverImage { get; set; }
        /// <summary>
        /// 产品详情图片；以英文半角逗号分隔的图片
        /// </summary>      
        public string ImageList { get; set; }
        /// <summary>
        /// 对中台商品的成本价（单位为元）
        /// </summary>
        public decimal ApplyPrice { get; set; }
        /// <summary>
        /// 对用户的显示价格（单位为元）
        /// </summary>
        public decimal ShowPrice { get; set; }
        /// <summary>
        /// 对用户的售价（单位为元）
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 当前产品库存库存
        /// </summary>
        public long TotalCount { get; set; }
        /// <summary>
        /// 产品状态。1:启用，0：禁用
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 物品是否需要邮寄0:不需要邮寄；1：需要邮寄
        /// </summary>
        public int NeedMail { get; set; }
        /// <summary>
        /// 产品备注（给后台人员查看的，不超过500字符）
        /// </summary>      
        public string Remark { get; set; }
        /// <summary>
        /// 购买须知（给客户查看的，不超过500字符）
        /// </summary>     
        public string BuyNotice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] ImageArray { get; set; } = new string[] { };
    }
}
