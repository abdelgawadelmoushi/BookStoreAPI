using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStoreAPI.Data.EntityConfigurations
{
    public class BookRatingEntityTypeConfiguration : IEntityTypeConfiguration<BookRating>
    {
        public void Configure(EntityTypeBuilder<BookRating> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.BookId, e.ApplicationUserId })
                   .IsUnique();

        }
    }
}
