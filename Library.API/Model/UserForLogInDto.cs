namespace Library.API.Model
{
    public class UserForLogInDto
    {
        public UserForLogInDto(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}