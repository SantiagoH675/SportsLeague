namespace SportsLeague.Domain.Entities
{
    public abstract class AuditBase
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
