namespace ComputerStore.Models
{
    public class Role
    {
        public string Id { get; set; } = null!;

        public string RoleName { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
