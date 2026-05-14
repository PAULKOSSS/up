using System;

namespace up.Models
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public int BookId { get; set; }
        public DateTime Date { get; set; }
    }
}