using System.ComponentModel.DataAnnotations;

namespace Library.API.Model
{
    public abstract class BookForManilpulationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "Title must have less then 100 characters")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description must have less then 500 characters")]
        public virtual string Description { get; set; }
    }
}