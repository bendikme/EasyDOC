using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using EasyDOC.Api.ViewModels;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using WebMatrix.WebData;

namespace EasyDOC.Api.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Login()
        {
            ViewBag.Message = "Sign in to continue to EasyDOC";
            return WebSecurity.IsAuthenticated ? (ActionResult)RedirectToAction("Index", "Home") : View("Login", new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (WebSecurity.Login(model.UserName, model.Password, model.RememberMe))
            {
                return Url.IsLocalUrl(returnUrl) ? (ActionResult)Redirect(returnUrl) : RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Sign in to continue to EasyDOC";
            ModelState.AddModelError("UserName", "Wrong user name or password");
            return View(model);
        }

        public ActionResult Logout()
        {
            WebSecurity.Logout();
            return RedirectToAction("Login");
        }

        public ActionResult ResetPassword()
        {
            ViewBag.Message = "Reset password";
            return View(new PasswordResetModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordResetModel model)
        {
            ViewBag.Message = "Reset password";

            var uow = new UnitOfWork();
            var user = uow.UserRepository.Get(u => u.Email == model.Email).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError("Email", "No user with this email address was found");
                return View(model);
            }

            var token = WebSecurity.GeneratePasswordResetToken(user.Username);
            SendPasswordResetEmailToUser(user, token);

            ViewBag.Message = "A password reset link has been mailed to your registered email account";
            return View("Login", new LoginViewModel());
        }

        private void SendPasswordResetEmailToUser(User user, string token)
        {
            var mail = new MailMessage();
            var server = new SmtpClient("smtp.online.no")
            {
                Credentials = new NetworkCredential("mic-eide", "hjauf776"),
            };

            mail.From = new MailAddress("bendik.eide@dynatec.no");
            mail.To.Add(user.Email);
            mail.Subject = "EasyDOC password reset";
            mail.Body = "Follow this link to reset your password: http://www2.dynatec.no/User/NewPassword?token=" + token;

            server.Send(mail);
        }

        public ActionResult NewPassword(string token)
        {
            ViewBag.Message = "Enter new password";
            if (!string.IsNullOrEmpty(token))
            {
                return View(new NewPasswordModel
                {
                    Token = token
                });
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPassword(NewPasswordModel model)
        {
            if (ModelState.IsValid && WebSecurity.ResetPassword(model.Token, model.Password))
            {
                ViewBag.Message = "Password successfully reset";
                return View("Login", new LoginViewModel());
            }

            ViewBag.Message = "Error resetting password";
            return View(model);
        }
    }
}