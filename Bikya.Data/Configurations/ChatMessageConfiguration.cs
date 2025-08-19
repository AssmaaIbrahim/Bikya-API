using Bikya.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        { // تعريف الـ Key (لو عندك PK)
            builder.HasKey(m => m.Id);

            // العلاقة مع Session
            builder.HasOne(m => m.Session)
                   .WithMany(s => s.Messages)
                   .HasForeignKey(m => m.SessionId);

        }
    }

}