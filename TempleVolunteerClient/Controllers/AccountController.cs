using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TempleVolunteerClient.Common;
using System.IdentityModel.Tokens.Jwt;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    public class AccountController : CustomController
    {
        public AccountController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings)
            : base(httpContextAccessor, AppSettings)
        {
        }

        #region Register
        [HttpGet]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public IActionResult Register(string? returnUrl = null)
        {
            RegisterViewModel viewModel = new RegisterViewModel();
            viewModel.GenderList = Common.ListHelpers.GenderList;
            viewModel.FirstName = "Gerry";
            viewModel.LastName = "Hazelton";
            viewModel.Address = "123 Main Street";
            viewModel.Address2 = "Apt. #22";
            viewModel.City = "Los Angeles";
            viewModel.State = "CA";
            viewModel.PostalCode = "92001";
            viewModel.Country = "US";
            viewModel.PhoneNumber = "555-555-5555";
            viewModel.EmailAddress = "gerryhazelton@gmail.com";
            viewModel.Password = "111111";
            viewModel.Gender = "Man";
            viewModel.Kriyaban = true;
            viewModel.LessonStudent = true;
            viewModel.CPR = true;
            viewModel.FirstAid = true;
            viewModel.AcceptTerms = true;
            ViewData["ReturnUrl"] = returnUrl;

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult?> Register(RegisterViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newVolunteer = new RegisterRequest
                    {
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        Address = viewModel.Address,
                        Address2 = viewModel.Address2,
                        City = viewModel.City,
                        State = viewModel.State,
                        Country = viewModel.Country,
                        PostalCode = viewModel.PostalCode,
                        EmailAddress = viewModel.EmailAddress,
                        Password = viewModel.Password,
                        PhoneNumber = viewModel.PhoneNumber,
                        Gender = viewModel.Gender,
                        FirstAid = viewModel.FirstAid,
                        CPR = viewModel.CPR,
                        Kriyaban = viewModel.Kriyaban,
                        LessonStudent = viewModel.LessonStudent,
                        AcceptTerms = true,
                        PropertyId = viewModel.TemplePropertyId
                    };

                    using (HttpClient client = new HttpClient())
                    {
                        var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                        client.DefaultRequestHeaders.Accept.Add(contentType);
                        HttpResponseMessage staffResponse = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync", this.Uri));
                        string stringData = staffResponse.Content.ReadAsStringAsync().Result;
                        ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                        var allStaff = JsonConvert.DeserializeObject<List<StaffViewModel>>(data.Data.ToString());

                        foreach (StaffViewModel? staff in allStaff)
                        {
                            if (staff.EmailAddress == viewModel.EmailAddress)
                            {
                                TempData["ModalMessage"] = "Email address is already registered.";

                                return RedirectPermanent("/Account/AccountModalPopUp");
                            }
                        }

                        stringData = JsonConvert.SerializeObject(newVolunteer);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        var response = await client.PostAsync(string.Format("{0}/Account/RegisterAsync", this.Uri), contentData);

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            TempData["ModalMessage"] = "Unable to register user. Please contact support.";

                            return RedirectPermanent("/Account/AccountModalPopUp");
                        }
                    }

                    TempData["ModalMessage"] = "Registration successfully completed.The administrative staff will review your registration and will send you an email shortly. Thank you.";

                    return RedirectPermanent("/Account/AccountModalPopUp");
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to register user: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp");
            }

            return View(viewModel);
        }
        #endregion

        #region Login/Logout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            LoginViewModel viewModel = new LoginViewModel();
            viewModel.EmailAddress = "gerryhazelton@gmail.com";
            viewModel.Password = "Mataji99#";
            viewModel.TemplePropertyList = await this.GetTempleProperties(true, false);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel viewModel, string? returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    JWT jwt = null;

                    using (HttpClient client = new HttpClient())
                    {
                        var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                        client.DefaultRequestHeaders.Accept.Add(contentType);

                        LoginRequest login = new LoginRequest();
                        login.EmailAddress = viewModel.EmailAddress;
                        login.Password = viewModel.Password;
                        login.RememberMe = viewModel.RememberMe;
                        login.PropertyId = viewModel.TemplePropertyId;

                        string stringData = JsonConvert.SerializeObject(login);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        HttpResponseMessage response = client.PostAsync(string.Format("{0}/Account/LoginAsync", this.Uri), contentData).Result;
                        var responseDeserialized = JsonConvert.DeserializeObject<TokenResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                        if (!responseDeserialized.Success)
                        {
                            TempData["ModalMessage"] = string.Format("{0}", responseDeserialized.Message);

                            return RedirectPermanent("/Account/AccountModalPopUp");
                        }

                        string stringJWT = response.Content.ReadAsStringAsync().Result;
                        jwt = JsonConvert.DeserializeObject<JWT>(stringJWT);
                        HttpContext.Session.SetString("token", jwt.AccessToken);

                        if (String.IsNullOrEmpty(jwt.AccessToken))
                        {
                            if (!responseDeserialized.Success)
                            {
                                TempData["ModalMessage"] = string.Format("{0}", responseDeserialized.Message);

                                return RedirectPermanent("/Account/AccountModalPopUp");
                            }

                            TempData["ModalMessage"] = "Error occurred. Unable to get jwt.Data. Please contact support.";

                            return RedirectPermanent("/Account/AccountModalPopUp");
                        }

                        var token = new JwtSecurityToken(jwtEncodedString: jwt.AccessToken);
                        this.SetStringSession("EmailAddress", viewModel.EmailAddress);
                        this.SetStringSession("FirstName", responseDeserialized.FirstName);
                        this.SetStringSession("LastName", responseDeserialized.LastName);
                        this.SetIntSession("IsAdmin", responseDeserialized.IsAdmin ? 1 : 0);
                        this.SetIntSession("StaffId", responseDeserialized.StaffId);
                        this.SetIntSession("PropertyId", viewModel.TemplePropertyId);
                        this.SetStringSession("PropertyName", responseDeserialized.PropertyName);
                        this.SetStringSession("Token", token.RawData);

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData["ModalMessage"] = "Unable to login. Please try again.";

                    return RedirectPermanent("/Account/AccountModalPopUp");
                }
            }
            catch
            {
                return RedirectPermanent("/Account/ServerIsDownModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public IActionResult Logout()
        {
            this.SetStringSession("EmailAddress", string.Empty);
            this.SetStringSession("FirstName", string.Empty);
            this.SetStringSession("LastName", string.Empty);
            this.SetIntSession("IsAdmin", 0);
            this.SetIntSession("StaffId", 0);
            this.SetIntSession("PropertyId", 0);
            this.SetStringSession("PropertyName", string.Empty);
            this.SetStringSession("Token", string.Empty);
            HttpContext.Session.Remove("token");

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Change Password
        [HttpPost]
        public async Task ChangePassword(string newPassword)
        {
            if (!IsAuthenticated())
            {
                throw new Exception("You are not authorized");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    ResetPasswordRequest resetPassword = new ResetPasswordRequest();
                    resetPassword.EmailAddress = this.GetStringSession("EmailAddress");
                    resetPassword.Token = this.GetStringSession("Token");
                    resetPassword.NewPassword = newPassword;
                    resetPassword.PropertyId = this.GetIntSession("PropertyId");

                    var stringData = JsonConvert.SerializeObject(resetPassword);
                    var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                    var response = await client.PostAsync(string.Format("{0}/Account/ResetPasswordAsync", this.Uri), contentData);
                    var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!responseDeserialized.Success)
                    {
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Verify Email Address
        [HttpGet]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> VerifyEmailAddress(string emailAddress, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    VerifyEmailAddressRequest verifyEmailAddress = new VerifyEmailAddressRequest();
                    verifyEmailAddress.EmailAddress = emailAddress;
                    verifyEmailAddress.Token = token;
                    verifyEmailAddress.PropertyId = this.GetIntSession("PropertyId");

                    string stringData = JsonConvert.SerializeObject(verifyEmailAddress);
                    var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                    var response = await client.PostAsync(string.Format("{0}/Account/VerifyEmailAddressAsync", this.Uri), contentData);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        TempData["ModalMessage"] = "Unable to verify email for registration. Please contact support.";

                        return RedirectPermanent("/Account/AccountModalPopUp");
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (responseDeserialized.Message == "Verified")
                    {
                        TempData["ModalMessage"] = "You have already verified your registration.";

                        return RedirectPermanent("/Account/AccountModalPopUp");
                    }

                    if (!responseDeserialized.Success)
                    {
                        TempData["ModalMessage"] = "Unable to verify email for registration. Please contact support.";

                        return RedirectPermanent("/Account/AccountModalPopUp");
                    }
                }

                TempData["ModalMessage"] = "You have successfully verified you email for registration, you may now log in.";

                return RedirectPermanent("/Account/AccountModalPopUp");
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to register user: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp");
            }
        }
        #endregion

        #region Forgot/Reset Password
        [HttpGet]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ForgotPassword()
        {
            ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel();
            viewModel.EmailAddress = "gerryhazelton@gmail.com";
            viewModel.PostalCode = "92009";
            viewModel.TemplePropertyList = await this.GetTempleProperties(true, false);

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                        client.DefaultRequestHeaders.Accept.Add(contentType);

                        ForgotPasswordRequest forgotPassword = new ForgotPasswordRequest();
                        forgotPassword.EmailAddress = viewModel.EmailAddress;
                        forgotPassword.PostalCode = viewModel.PostalCode;
                        forgotPassword.PropertyId = viewModel.TemplePropertyId;

                        var stringData = JsonConvert.SerializeObject(forgotPassword);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        HttpResponseMessage response = await client.PostAsync(string.Format("{0}/Account/ForgotPasswordAsync", this.Uri), contentData);
                        var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                        if (!responseDeserialized.Success)
                        {
                            TempData["ModalMessage"] = string.Format("Unable to find your profile, please try again.");
                            return RedirectPermanent("/Account/AccountModalPopUp");
                        }
                    }

                    TempData["ModalMessage"] = "Please check your email to complete resetting your password.";

                    return RedirectPermanent("/Account/AccountModalPopUp");
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to register user: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp");
            }

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public IActionResult ResetPassword()
        {
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel();
            viewModel.EmailAddress = this.GetStringSession("EmailAddress");
            viewModel.Token = this.GetStringSession("Token");

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                        client.DefaultRequestHeaders.Accept.Add(contentType);

                        ResetPasswordRequest resetPassword = new ResetPasswordRequest();
                        resetPassword.EmailAddress = viewModel.EmailAddress;
                        resetPassword.Token = viewModel.Token;
                        resetPassword.OldPassword = viewModel.OldPassword;
                        resetPassword.NewPassword = viewModel.Password;
                        resetPassword.PropertyId = viewModel.PropertyId;

                        var stringData = JsonConvert.SerializeObject(resetPassword);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        var response = await client.PostAsync(string.Format("{0}/Account/ResetPasswordAsync", this.Uri), contentData);
                        var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                        if (!responseDeserialized.Success)
                        {
                            TempData["ModalMessage"] = string.Format("{0} Please contact support.", responseDeserialized.Message);
                            return RedirectPermanent("/Account/AccountModalPopUp");
                        }
                    }

                    TempData["ModalMessage"] = "Your new password has been created, you can now login";

                    return RedirectPermanent("/Account/AccountModalPopUp");
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to register user: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp");
            }

            return View(viewModel);
        }
        #endregion

        #region Helpers
        public IActionResult AccountModalPopUp()
        {
            return View();
        }

        public IActionResult ExceededLoginsPopUp()
        {
            return View();
        }

        public IActionResult AccountCannotDeleteSelfModalPopUp()
        {
            return View();
        }

        public IActionResult ServerIsDownModalPopUp()
        {
            return View();
        }

        public IActionResult SessionTimedOutPopUp()
        {
            return View();
        }
        #endregion
    }
}