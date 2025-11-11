using CoreBanking.Application.Common;


namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IEmailSenderr
    {
        Task SendEmailAsync(Message message);
    }

}
