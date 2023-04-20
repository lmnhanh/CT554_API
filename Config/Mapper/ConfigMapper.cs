using AutoMapper;
using CT554_API.Models;
using CT554_Entity.Entity;

namespace CT554_API.Config.Mapper
{
    public class ConfigMapper : Profile
    {
        public ConfigMapper() {
            CreateMap<Vender, VenderInfo>();
            CreateMap<Cart, CartInfo>();
            CreateMap<Order, OrderInfo>();
            CreateMap<User, UserInfo>();
            CreateMap<Category, CategoryInfo>();
            CreateMap<Product, ProductInfo>();
            CreateMap<ProductDetail, ProductDetailInfo>();
            CreateMap<Price, PriceInfo>();
            CreateMap<Invoice, InvoiceInfo>()
                .ForMember(info => info.InvoiceDetails, option => option.MapFrom(invoice => invoice.Details!.AsEnumerable()));
            CreateMap<InvoiceDetail, InvoiceDetailInfo>();

            CreateMap<Stock, StockCombineModel>()
                .ForMember(model => model.Type, option => option.MapFrom(stock =>
                    stock.IsManualUpdate ? "Cập nhật số lượng thủ công" : "Cập nhật từ đơn hàng, nhập hàng"))
                 .ForMember(model => model.Description, option => option.MapFrom(stock => stock.Description))
                .ForMember(model => model.Value, option => option.MapFrom(stock => stock.ManualValue));
            CreateMap<InvoiceDetail, StockCombineModel>()
                 .ForMember(model => model.Type, option => option.MapFrom(invoice => "Đơn nhập hàng"))
                 .ForMember(model => model.Value, option => option.MapFrom(invoice => invoice.Quantity))
                 .ForMember(model => model.Description, option => option.MapFrom(invoice => $"Từ {invoice.GetVender}"))
                 .ForMember(model => model.DateUpdate, option => option.MapFrom(invoice => invoice.GetDateCreate()))
                 .ForMember(model => model.Id, option => option.MapFrom(invoice => invoice.InvoiceId));
            CreateMap<Cart, StockCombineModel>()
                 .ForMember(model => model.Type, option => option.MapFrom(order => "Đơn bán hàng"))
                 .ForMember(model => model.Value, option => option.MapFrom(cart => cart.RealQuantity))
                 .ForMember(model => model.Description, option => option.MapFrom(cart => $"Đơn hàng {cart.OrderId}"))
                 .ForMember(model => model.DateUpdate, option => option.MapFrom(cart => cart.GetDateCreate()))
				 .ForMember(model => model.Id, option => option.MapFrom(cart => cart.OrderId));

			CreateMap<InvoiceDTO, Invoice>();
            CreateMap<Stock, StockDTO>().ReverseMap();
            CreateMap<CartDTO, Cart>().ReverseMap();
            CreateMap<OrderDTO, Order>().ReverseMap();
            CreateMap<PromotionDTO, Promotion>();
            CreateMap<Promotion, PromotionInfo>();
			CreateMap<ProductDTO, Product>();
		}
    }
}
