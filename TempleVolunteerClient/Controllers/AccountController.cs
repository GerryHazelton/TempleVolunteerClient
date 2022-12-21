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
using Org.BouncyCastle.Crypto.Macs;
using iTextSharp.text.pdf.codec;

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

            if (1==1)
            {
                viewModel.FirstName = "Jane";
                viewModel.LastName = "Doe";
                viewModel.Address = "123 Main Street";
                viewModel.Address2 = "";
                viewModel.City = "Burbank";
                viewModel.State = "CA";
                viewModel.PostalCode = "90058";
                viewModel.Country = "US";
                viewModel.PhoneNumber = "213-555-5555";
                viewModel.EmailAddress = "hazelton1@yahoo.com";
                viewModel.Password = "111111";
                viewModel.Gender = "Man";
            }
            else
            {
                viewModel.FirstName = "Seannie";
                viewModel.LastName = "Gibson";
                viewModel.Address = "924 Elyria Drive";
                viewModel.Address2 = "";
                viewModel.City = "Los Angeles";
                viewModel.State = "CA";
                viewModel.PostalCode = "90065";
                viewModel.Country = "US";
                viewModel.PhoneNumber = "323-394-5332";
                viewModel.EmailAddress = "seanniegibson@gmail.com";
                viewModel.Password = "Master1952!";
                viewModel.Gender = "Femail";
            }

            viewModel.GenderList = Common.ListHelpers.GenderList;
            viewModel.Kriyaban = true;
            viewModel.LessonStudent = true;
            viewModel.CPR = true;
            viewModel.FirstAid = true;
            viewModel.AcceptTerms = true;
            viewModel.CanSendMessages = true;
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
                        PropertyId = viewModel.TemplePropertyId,

                        // testing only
                        CanSendMessages = viewModel.CanSendMessages,
                        CanViewDocuments = viewModel.CanViewDocuments,
                        EmailConfirmed = viewModel.EmailConfirmed,
                        IsVerified = viewModel.IsVerified,
                        IsActive = viewModel.IsActive,
                        VerifiedDate = viewModel.VerifiedDate
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
        public async Task<IActionResult> MyProfile()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            MyProfileViewModel viewModel = new MyProfileViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, GetIntSession("StaffId"), GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: MyProfile. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Account/AccountModalPopUp");
                    }

                    var staff = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    var staffObj = JsonConvert.DeserializeObject<StaffRequest>(staff.Data.ToString());

                    viewModel.StaffId = staffObj.StaffId;
                    viewModel.FirstName = staffObj.FirstName;
                    viewModel.MiddleName = staffObj.MiddleName;
                    viewModel.LastName = staffObj.LastName;
                    viewModel.Address = staffObj.Address;
                    viewModel.Address2 = staffObj.Address2;
                    viewModel.City = staffObj.City;
                    viewModel.State = staffObj.State;
                    viewModel.Country = staffObj.Country;
                    viewModel.PostalCode = staffObj.PostalCode;
                    viewModel.PhoneNumber = staffObj.PhoneNumber;
                    viewModel.Gender = staffObj.Gender;
                    viewModel.FirstAid = (bool)staffObj.FirstAid;
                    viewModel.CPR = (bool)staffObj.CPR;
                    viewModel.Kriyaban = (bool)staffObj.Kriyaban;
                    viewModel.LessonStudent = (bool)staffObj.LessonStudent;
                    viewModel.EmailAddress = staffObj.EmailAddress;
                    viewModel.RoleName = this.Admin == 1 ? "Admin" : "Volunteer";
                    viewModel.StaffFileName = staffObj.StaffFileName;
                    viewModel.StaffByte = staffObj.StaffImage;
                    viewModel.StaffByteString = staffObj.StaffImage != null ? Convert.ToBase64String(staffObj.StaffImage) : "";
                    viewModel.RemovePhoto = false;
                    viewModel.GenderList = Common.ListHelpers.GenderList;
                    viewModel.Countries = Common.ListHelpers.Countries;
                    viewModel.States = Common.ListHelpers.States;
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
                bool fileChange = false;
                MyProfileRequest myProfile = null;

                if (ModelState.IsValid)
                {
                    myProfile = new MyProfileRequest();

                    if (viewModel.staffImage != null)
                    {
                        if (String.IsNullOrEmpty(viewModel.PrevStaffFileName) && !String.IsNullOrEmpty(viewModel.StaffFileName))
                        {
                            fileChange = true;
                        }

                        if (!String.IsNullOrEmpty(viewModel.PrevStaffFileName) && !String.IsNullOrEmpty(viewModel.StaffFileName))
                        {
                            fileChange = !viewModel.PrevStaffFileName.Trim().ToLower().Equals(viewModel.StaffFileName.Trim().ToLower());
                        }
                    }

                    myProfile.FirstName = viewModel.FirstName;
                    myProfile.MiddleName = viewModel.MiddleName;
                    myProfile.LastName = viewModel.LastName;
                    myProfile.Address = viewModel.Address;
                    myProfile.Address2 = viewModel.Address2;
                    myProfile.City = viewModel.City;
                    myProfile.State = viewModel.Country == "US" ? viewModel.State : null;
                    myProfile.Country = viewModel.Country;
                    myProfile.PostalCode = viewModel.PostalCode;
                    myProfile.PhoneNumber = viewModel.PhoneNumber;
                    myProfile.Gender = viewModel.Gender;
                    myProfile.FirstAid = viewModel.FirstAid;
                    myProfile.CPR = viewModel.CPR;
                    myProfile.Kriyaban = viewModel.Kriyaban;
                    myProfile.LessonStudent = viewModel.LessonStudent;
                    myProfile.EmailAddress = viewModel.EmailAddress;
                    myProfile.PropertyId = this.GetIntSession("PropertyId");
                    myProfile.StaffFileName = viewModel.StaffFileName;

                    if (fileChange)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.staffImage.FileName);
                        string extension = Path.GetExtension(viewModel.staffImage.FileName);
                        fileName = fileName + DateTime.UtcNow.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
                        MemoryStream ms = null;
                        byte[] buffer = new byte[16 * 1024];

                        using (fs = System.IO.File.Create(path))
                        {
                            await viewModel.staffImage.CopyToAsync(fs);

                            using (ms = new MemoryStream())
                            {
                                int read;
                                fs.Position = 0;
                                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                            }
                        }

                        myProfile.StaffFileName = viewModel.staffImage.FileName;
                        myProfile.StaffImage = ms.ToArray();
                        System.IO.File.Delete(path);
                    }

                    using (HttpClient client = new HttpClient())
                    {
                        myProfile.RemovePhoto = viewModel.RemovePhoto;
                        myProfile.UpdatedBy = GetStringSession("EmailAddress");
                        myProfile.UpdatedDate = DateTime.UtcNow;
                        myProfile.EmailAddress = GetStringSession("EmailAddress");
                        myProfile.PropertyId = GetIntSession("PropertyId");
                        var data = JsonConvert.SerializeObject(myProfile);
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