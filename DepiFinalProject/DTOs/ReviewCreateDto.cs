using System.ComponentModel.DataAnnotations;
namespace DepiFinalProject.DTOs.Reviews
{
    public class ReviewCreateDto
    {
        public int ProductID { get; set; }
        public int Rating { get; set; }  // 1–5
        public string Comment { get; set; }
    }
    public class ReviewUpdateDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
    public class ReviewResponseDto
    {
        public int ReviewID { get; set; }
        public int ProductID { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
