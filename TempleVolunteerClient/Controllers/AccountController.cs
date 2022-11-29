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
using AutoMapper;
using static TempleVolunteerClient.Common.Permissions;
using static TempleVolunteerClient.Common.ListHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Bcpg;

namespace TempleVolunteerClient
{
    public class AccountController : CustomController
    {
        private IWebHostEnvironment _environment;
        private IMapper _mapper;

        public AccountController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IWebHostEnvironment environment, IMapper mapper)
            : base(httpContextAccessor, AppSettings)
        {
            _environment = environment;
            _mapper = mapper;
        }

        #region Register
        [HttpGet]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult?> Register(string? returnUrl = null)
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
            viewModel.TemplePropertyList = await this.GetTempleProperties(true, false);

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
                        HttpResponseMessage staffResponse = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?propertyId={1}&userId='{2}'", this.Uri, viewModel.TemplePropertyId, viewModel.EmailAddress));
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

                    return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to register user: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
            }

            return View(viewModel);
        }
        #endregion

        #region My Profile
        [HttpGet]
        public IActionResult MyProfile()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            MyProfileViewModel viewModel = new MyProfileViewModel();

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    MiscRequest request = new MiscRequest();
                    request.PropertyId = this.GetIntSession("PropertyId");
                    request.GetById = this.GetIntSession("StaffId");
                    request.UserId = this.GetStringSession("EmailAddress");

                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    string stringData = JsonConvert.SerializeObject(request);
                    var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                    HttpResponseMessage response = client.PostAsync(string.Format("{0}/Staff/GetByIdAsync", this.Uri), contentData).Result;
                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: MyProfile. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Account/AccountModalPopUp");
                    }

                    var staff = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<MyProfileViewModel>(JsonConvert.DeserializeObject<StaffRequest>(staff.Data.ToString()));
                    viewModel.GenderList = Common.ListHelpers.GenderList;
                    viewModel.Countries = Common.ListHelpers.Countries;
                    viewModel.States = Common.ListHelpers.States;
                    viewModel.StaffPrevImage = viewModel.StaffImage;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: Unable to view profile: {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(MyProfileViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                if (ModelState.IsValid)
                {
                    bool updateImage = viewModel.StaffImageFile != null ? true : false;
                    var myProfileRequest = _mapper.Map<MyProfileRequest>(viewModel);
                    MemoryStream ms = null;

                    if (updateImage)
                    {
                        using var dataStream = new MemoryStream();
                        await viewModel.StaffImageFile.CopyToAsync(dataStream);
                        byte[] imageBytes = dataStream.ToArray();
                        viewModel.StaffImage = Convert.ToBase64String(imageBytes);
                        viewModel.StaffPrevImage = viewModel.StaffImage;
                        myProfileRequest.StaffImage = viewModel.StaffImage;
                        myProfileRequest.StaffImageFileName = viewModel.StaffImageFile.FileName;
                    }
                    else
                    {
                        if (viewModel.StaffImage != null)
                        {
                            viewModel.StaffPrevImage = viewModel.StaffImage;
                            viewModel.StaffFileName = viewModel.StaffFileName;
                        }
                    }

                    using (HttpClient client = new HttpClient())
                    {
                        myProfileRequest.UpdatedBy = GetStringSession("EmailAddress");
                        myProfileRequest.UpdatedDate = DateTime.Now;
                        myProfileRequest.EmailAddress = GetStringSession("EmailAddress");
                        myProfileRequest.PropertyId = GetIntSession("PropertyId");
                        var data = JsonConvert.SerializeObject(myProfileRequest);
                        var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                        var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                        client.DefaultRequestHeaders.Accept.Add(contentType);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                        if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                        {
                            TempData["ModalMessage"] = string.Format("Error occurred: MyProfile: {0}. Bearer token is null. Please contact support.", this.GetStringSession("EmailAddress"));

                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                        }

                        HttpResponseMessage response = await client.PutAsync(string.Format("{0}/Account/MyProfileAsync", this.Uri), content);

                        if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                        {
                            TempData["ModalMessage"] = string.Format("Error occurred: MyProfile. Message: '{0}'. Please contact support.", response.RequestMessage);

                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                        }

                        TempData["ModalMessage"] = "Profile successfully updated";
                    }
                }
                else
                {
                    return View(viewModel);
                }

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Profile);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: Unable to view profile: {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        #region Login/Logout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            LoginViewModel viewModel = new LoginViewModel();
            viewModel.EmailAddress = "gerryhazelton@gmail.com";
            viewModel.Password = "11111111";
            viewModel.TemplePropertyId = 1;
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
                        login.CreatedBy = viewModel.EmailAddress;

                        string stringData = JsonConvert.SerializeObject(login);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        HttpResponseMessage response = client.PostAsync(string.Format("{0}/Account/LoginAsync", this.Uri), contentData).Result;
                        var responseDeserialized = JsonConvert.DeserializeObject<TokenResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                        if (!responseDeserialized.Success)
                        {
                            TempData["ModalMessage"] = string.Format("{0}", responseDeserialized.Message);

                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                        }

                        string stringJWT = response.Content.ReadAsStringAsync().Result;
                        jwt = JsonConvert.DeserializeObject<JWT>(stringJWT);
                        HttpContext.Session.SetString("token", jwt.AccessToken);

                        if (String.IsNullOrEmpty(jwt.AccessToken))
                        {
                            if (!responseDeserialized.Success)
                            {
                                TempData["ModalMessage"] = string.Format("{0}", responseDeserialized.Message);

                                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "Error occurred. Unable to get jwt.Data. Please contact support.";

                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
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

                    return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to login: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
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

        #region Verify Email Address
        [HttpGet]
        [AllowAnonymous]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> VerifyEmailAddress(string emailAddress, int propertyId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    VerifyEmailAddressRequest verifyEmailAddress = new VerifyEmailAddressRequest();
                    verifyEmailAddress.EmailAddress = emailAddress;
                    //verifyEmailAddress.Token = token;
                    verifyEmailAddress.PropertyId = propertyId;

                    string stringData = JsonConvert.SerializeObject(verifyEmailAddress);
                    var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                    var response = await client.PostAsync(string.Format("{0}/Account/VerifyEmailAddressAsync", this.Uri), contentData);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        TempData["ModalMessage"] = "Unable to verify email for registration. Please contact support.";

                        return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (responseDeserialized.Message == "Verified")
                    {
                        TempData["ModalMessage"] = "You have already verified your registration.";

                        return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
                    }

                    if (!responseDeserialized.Success)
                    {
                        TempData["ModalMessage"] = "Unable to verify email for registration. Please contact support.";

                        return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                    }
                }

                TempData["ModalMessage"] = "You have successfully verified you email for registration, you may now log in.";

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to verify email address: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
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
                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.ForgotPassword);
                        }
                    }

                    TempData["ModalMessage"] = "Please check your email to complete resetting your password.";

                    return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to reset password: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
            }

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetForgottenPassword(string EmailAddress, int PropertyId, ResetForgottenPasswordViewModel viewModel = null)
        {
            if (String.IsNullOrEmpty(viewModel.Password))
            {
                var temples = await this.GetTempleProperties(true, false);
                viewModel.EmailAddress = EmailAddress;
                viewModel.PropertyId = PropertyId;
                viewModel.TempleName = temples.First(x => x.Value == PropertyId.ToString()).Text;

                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    ResetForgottenPasswordRequest resetPassword = new ResetForgottenPasswordRequest();
                    resetPassword.EmailAddress = viewModel.EmailAddress;
                    resetPassword.Password = viewModel.Password;
                    resetPassword.PropertyId = viewModel.PropertyId;

                    var stringData = JsonConvert.SerializeObject(resetPassword);
                    var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                    var response = await client.PostAsync(string.Format("{0}/Account/ResetForgottenPasswordAsync", this.Uri), contentData);
                    var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!responseDeserialized.Success)
                    {
                        TempData["ModalMessage"] = string.Format("{0} Please contact support.", responseDeserialized.Message);

                        return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                    }
                }

                TempData["ModalMessage"] = "Your new password has been updated, you can now login";

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
            }
            else
            {
                var temples = await this.GetTempleProperties(true, false);
                viewModel.EmailAddress = EmailAddress;
                viewModel.PropertyId = PropertyId;
                viewModel.TempleName = temples.First(x => x.Value == PropertyId.ToString()).Text;

                return View(viewModel);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public IActionResult ResetPassword()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            ResetPasswordViewModel viewModel = new ResetPasswordViewModel();
            viewModel.EmailAddress = this.GetStringSession("EmailAddress");
            viewModel.Token = this.GetStringSession("Token");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

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
                        resetPassword.PropertyId = this.GetIntSession("PropertyId");

                        var stringData = JsonConvert.SerializeObject(resetPassword);
                        var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                        var response = await client.PostAsync(string.Format("{0}/Account/ResetPasswordAsync", this.Uri), contentData);
                        var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                        if (!responseDeserialized.Success)
                        {
                            TempData["ModalMessage"] = string.Format("{0} Please contact support.", responseDeserialized.Message);

                            return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Error);
                        }
                    }

                    TempData["ModalMessage"] = "Your new password has been created, you can now login";

                    return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred. Unable to reset password: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AccountModalPopUp?type=" + ModalType.Login);
            }

            return View(viewModel);
        }
        #endregion

        #region Helpers
        public IActionResult AccountModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };
            
            return View(viewModel);
        }

        public IActionResult ErrorModalPopUp()
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = ModalType.Error };
            TempData["ModalMessage"] = string.Format("A system error occurred. Please contact one of the temple admins.");

            return View(viewModel);
        }
        #endregion
    }
}