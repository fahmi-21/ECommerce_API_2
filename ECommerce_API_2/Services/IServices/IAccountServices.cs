namespace ECommerce_API_2.Services.IServices
{
    public interface IAccountServices
    {
        Task SendEmailAsync ( EmailType emailType, string msg ,  ApplicationUser applicationUser);
    }
}
