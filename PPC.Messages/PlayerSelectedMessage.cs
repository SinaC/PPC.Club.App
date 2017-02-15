namespace PPC.Messages
{
    public class PlayerSelectedMessage
    {
        public string DciNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool SwitchToShop { get; set; }
    }
}
