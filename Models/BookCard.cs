namespace up.Models
{
    public class BookCard
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string ImageUrl { get; set; }
        public double? Rating { get; set; }
        public int? CurrentStatusId { get; set; }

        // ✅ ДОБАВЛЕНО: статус заморозки
        public bool IsFrozen { get; set; }
    }
}