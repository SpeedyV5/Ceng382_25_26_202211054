namespace ClassManagement.Models
{
    public class ClassInformationTable
    {
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public string Description { get; set; }
        public int Id { get; set; } // UI'de gösterilmeyecek, işlemler için kullanılacak
    }
}