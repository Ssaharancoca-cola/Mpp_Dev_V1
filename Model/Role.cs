namespace Model
{
    public class ROLE
    {
        public ROLE()
        {
            this.ROLE_NAME = string.Empty;
        }

        public ROLE(int _ROLE_ID, string _ROLE_NAME)
        {
            this.ROLE_ID = _ROLE_ID;
            this.ROLE_NAME = _ROLE_NAME;
        }

        public int ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
    }
}