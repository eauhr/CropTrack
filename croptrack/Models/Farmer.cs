namespace croptrack.Models
{
    public class Farmer
    {
        public int FarmerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<Field> Fields { get; set; }

    }
}
