using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerc519.API.DataAsecc.EntityConfigurations
{
    public class ProductColorEntityTypeConfiguration : IEntityTypeConfiguration<ProductColor>
    {
        public void Configure(EntityTypeBuilder<ProductColor> builder)
        {
            builder.HasKey(e => new { e.ProductId, e.Color });
        }
    }
}
