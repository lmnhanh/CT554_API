using AutoMapper;
using CT554_API.Models;
using CT554_Entity.Entity;

namespace CT554_API.Config.Mapper
{
    public class ConfigMapper : Profile
    {
        public ConfigMapper() {
            CreateMap<Vender, VenderInfo>();
            CreateMap<Category, CategoryInfo>();
            CreateMap<Product, ProductInfo>();
            CreateMap<ProductDetail, ProductDetailInfo>();
            CreateMap<Invoice, InvoiceInfo>()
                .ForMember(info => info.InvoiceDetails, option => option.MapFrom(invoice => invoice.Details!.AsEnumerable()));
            CreateMap<InvoiceDetail, InvoiceDetailInfo>();

            CreateMap<InvoiceDTO, Invoice>();
            CreateMap<Stock, StockDTO>().ReverseMap();
        }
    }
}
