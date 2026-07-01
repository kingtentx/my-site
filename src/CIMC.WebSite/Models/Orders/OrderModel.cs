using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CimcSite.Web.Models
{
    public class OrderModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>       
        public long OrderId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>       
        public string OrderNo { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>      
        public string DeliverName { get; set; }
        /// <summary>
        /// 收货人手机号码
        /// </summary>

        public string DeliverPhone { get; set; }
        /// <summary>
        ///  省份
        /// </summary>

        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>

        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>

        public string Region { get; set; }
        /// <summary>
        /// 地址
        /// </summary>

        public string Address { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// 运费
        /// </summary>
        public decimal Freight { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime BuyTime { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliveryTime { get; set; }
        /// <summary>
        /// 订单状态：0-未支付  5-已支付  10-已发货  20-已完成 80-退货
        /// </summary>
        public int OrderStatus { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public int PaymentCode { get; set; }
        /// <summary>
        /// 商家Id
        /// </summary>
        public int TenantId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>        
        public string Remarks { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public bool IsDelete { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>      
        [ForeignKey("OrderId")]
        public virtual List<OrderGoodsModel> OrderGoodsList { get; set; }

    }

    public class OrderGoodsModel
    {
        /// <summary>
        /// 
        /// </summary>        
        public long SubId { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>       
        public long OrderId { get; set; }
        /// <summary>
        /// 商品SKU
        /// </summary>
        public long SkuId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>       
        public string ProductName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 结算价
        /// </summary>
        public decimal SettlementPrice { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>      
        public string LogisticCode { get; set; }
        /// <summary>
        /// 物流公司代码
        /// </summary>      
        public string LogisticsCompanyCode { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>       
        public string LogisticsCompanyName { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliveryTime { get; set; }
        /// <summary>
        /// 产品编号（后续下单使用该字段，请保证唯一）
        /// </summary>     
        public string ProductSn { get; set; }
    }
}
