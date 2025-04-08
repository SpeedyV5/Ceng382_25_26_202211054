using System.ComponentModel.DataAnnotations;

namespace ClassManagement.Models
{
    public class ClassInformationModel
    {
        private static int _counter = 1;

        public ClassInformationModel()
        {
            Id = _counter++;
        }

        public int Id { get; set; }

        [Required]
        public string? ClassName { get; set; } = string.Empty;

        [Required]
        [Range(1, 500, ErrorMessage = "Student count must be between 1 and 500")]
        public int StudentCount { get; set; }

        public string? Description { get; set; } = string.Empty;
    }
}