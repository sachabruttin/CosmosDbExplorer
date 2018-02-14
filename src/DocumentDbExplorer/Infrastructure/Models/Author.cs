namespace DocumentDbExplorer.Infrastructure.Models
{
    public class Author
    {
        public Author(string name, string profile)
        {
            Name = name;
            Profile = profile;
        }

        public string Name { get; }
        public string Profile { get; }
    }
}
