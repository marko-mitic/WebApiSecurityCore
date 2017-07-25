using System.ComponentModel.DataAnnotations;

namespace Library.API.Model
{
    public class BookForUpdateDto : BookForManilpulationDto
    {
        [Required(ErrorMessage = "You should fill out a description")]
        public override string Description { get; set; }
    }
}