using Hangfire;
using HangFireDemo.DbBackUp;
using HangFireDemo.Emailservice;
using HangFireDemo.GoogleDrive;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HangFireDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangFireController : ControllerBase
    {
        private readonly IEmailLogic _emailLogic;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IDataBaseBackUp _dataBaseBackUp;
        public HangFireController(IEmailLogic emailLogic, IGoogleDriveService googleDriveService, IDataBaseBackUp dataBaseBackUp)
        {
            _emailLogic = emailLogic;
            _googleDriveService = googleDriveService;
            _dataBaseBackUp = dataBaseBackUp;
        }
        [HttpPost]
        [Route("welcome")]
        public IActionResult Welcome(string mailAddress)
        {
            //Fire-and-Forget Jobs
            //Fire - and - forget jobs are executed only once and almost immediately after creation.
            var mailSubject = $"Welcome {mailAddress}";
            var mailBody = $"<p>Thanks for join us!</p>";
            var jobId = BackgroundJob.Enqueue(() => _emailLogic.SendEmailAsync(mailAddress, mailSubject, mailBody));

            return Ok($"JobId : {jobId} Completed. Welcome Mail Sent");
        }

        [HttpPost]
        [Route("welcome-delayed")]
        public IActionResult WelcomeDelayed(string mailAddress)
        {
            //Delayed Jobs
            //Delayed jobs are executed only once too, but not immediately, after a certain time interval.
            var mailSubject = $"Welcome {mailAddress}";
            var mailBody = $"<p>Thanks for join us!</p></br><p>This mail is delayed mail.!</p>";
            var jobId = BackgroundJob.Schedule(() => _emailLogic.SendEmailAsync(mailAddress, mailSubject, mailBody), TimeSpan.FromMinutes(1));

            //RecurringJob.AddOrUpdate(() => DeleteInactiveUsers(),Cron.Monthly);

            return Ok($"JobId : {jobId} Completed. Welcome Mail Sent");
        }

        [HttpPost]
        [Route("welcome-recurring")]
        public IActionResult WelcomeRecurring(string mailAddress)
        {
            //Recurring Jobs = Time Schedule
            //Recurring jobs fire many times on the specified CRON schedule.
            var mailSubject = $"Welcome {mailAddress}";
            var mailBody = $"<p>Thanks for join us!</p></br><p>This mail is recurring every minute</p>";
            RecurringJob.AddOrUpdate(() => _emailLogic.SendEmailAsync(mailAddress, mailSubject, mailBody), "*/5 * * * *");
            RecurringJob.AddOrUpdate(() => _dataBaseBackUp.TriggerBackup(), "*/5 * * * *");

            return Ok($"Recurring job started, mails will send in every minute");
        }

        [HttpPost]
        [Route("welcome-continuation")]
        public IActionResult WelcomeContinuation(string mailAddress)
        {
            //Continuations Jobs
            //Continuations are executed when its parent job has been finished.

            //First job
            var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Continuations will start soon..."));

            var mailSubject = $"Welcome {mailAddress}";
            var mailBody = $"<p>Thanks for join us!</p></br><p>This mail is continuations hangfire job example.<br>Job started after NotifySystemAdmin process</p>";
            BackgroundJob.ContinueJobWith(jobId, () => _emailLogic.SendEmailAsync(mailAddress, mailSubject, mailBody));

            return Ok($"Recurring job started, mails will send in every minute");
        }

        [HttpGet("sendgrid")]
        public async Task<IActionResult> SendGrid()
        {
            /* var apikey = "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";
             var client = new SendGridClient(apikey);
             var from = new EmailAddress("marimuthu.slpm@gmail.com", "Mari");
             var to = new EmailAddress("kumarkumaresan135@gmail.com", "kumar");
             var subject = "Send Mail in SendGrid";
             var plaintextcontent = "Hii";
             var htmlcontent = "<strong> Sample integrate sendGrig</strong>";
             var msg = MailHelper.CreateSingleEmail(
                 from,
                 to,
                 subject,
                 plaintextcontent,
                 htmlcontent
                 );
             var responce = await client.SendEmailAsync(msg);
             return Ok(responce);*/


            try
            {
                var apiKey = "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";
                var client = new SendGridClient(apiKey);

                var message = new SendGridMessage();

                /*message.From = new EmailAddress("marimuthu.slpm@gmail.com", "Marimuthu.N");
                message.AddTo(new EmailAddress("kumaresanvaf135@gmail.com", "Recipient's Name"));
                message.Subject = "Your Subject Here";
                message.PlainTextContent = "This is the plain text content of the email.";
                message.HtmlContent = "<p>This is the HTML content of the email.</p>";*/

                var from = new EmailAddress("marimuthu.slpm@gmail.com", "Mari");
                var to = new EmailAddress("kumarkumaresan135@gmail.com", "kumar");
                var subject = "Send Mail in SendGrid";
                var plaintextcontent = "Hii";
                var htmlcontent = "<strong> Sample integrate sendGrig</strong>";
                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plaintextcontent,
                    htmlcontent
                    );

                var response = await client.SendEmailAsync(msg);

                return Ok(new
                {
                    StatusCode = response.StatusCode,
                    ResponseBody = await response.Body.ReadAsStringAsync().ConfigureAwait(false),
                    Headers = response.Headers.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
