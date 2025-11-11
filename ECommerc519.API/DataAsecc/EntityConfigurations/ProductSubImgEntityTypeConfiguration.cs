using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerc519.API.DataAsecc.EntityConfigurations
{
    public class ProductSubImgEntityTypeConfiguration : IEntityTypeConfiguration<ProductSubImg>
    {
        public void Configure(EntityTypeBuilder<ProductSubImg> builder)
        {
           builder.HasKey(e => new { e.ProductId, e.Img });
        }
    }
}
