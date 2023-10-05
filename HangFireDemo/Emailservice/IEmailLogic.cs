namespace HangFireDemo.Emailservice
{
    public interface IEmailLogic
    {
        public Task SendEmailAsync(string username, string subject, string mailBody);
    }
}
