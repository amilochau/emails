namespace Milochau.Emails.Options
{
    public class DatabaseOptions
    {
        public string AccountEndpoint { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string? ConnectionString { get; set; }
    }
}
