using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BookStoreAPI.Data.EntityConfigurations
{
    public class AuthorCategoryEntityTypeConfiguration : IEntityTypeConfiguration<AuthorCategory>
    {
        public void Configure(EntityTypeBuilder<AuthorCategory> builder)
        {
            builder.HasOne(ac => ac.Author)
                      .WithMany(a => a.AuthorCategories)
                      .HasForeignKey(ac => ac.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ac => ac.Category)
                   .WithMany(c => c.AuthorCategories)
                   .HasForeignKey(ac => ac.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
