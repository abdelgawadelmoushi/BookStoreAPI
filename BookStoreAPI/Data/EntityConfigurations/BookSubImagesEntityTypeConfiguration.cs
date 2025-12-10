using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStoreAPI.Data.EntityConfigurations
{
    public class BookSubImagesEntityTypeConfiguration : IEntityTypeConfiguration<BookSubImages>
    {
        public void Configure(EntityTypeBuilder<BookSubImages> builder)
        {
            builder.HasKey(e => new { e.BookId, e.Img });
        }
    }
}
