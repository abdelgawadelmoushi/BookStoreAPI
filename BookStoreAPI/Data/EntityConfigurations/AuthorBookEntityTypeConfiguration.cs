using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStoreAPI.Data.EntityConfigurations
{
    public class AuthorBookEntityTypeConfiguration : IEntityTypeConfiguration<AuthorBook>
    {
        public void Configure(EntityTypeBuilder<AuthorBook> builder)
        {
            builder.HasKey(e => new { e.BookId, e.AuthorId });
        }
    }
}
