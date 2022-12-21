

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TempleVolunteerClient.Common;
using static TempleVolunteerClient.Common.ListHelpers;

namespace TempleVolunteerClient
{
    public class StaffController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;
        private IWebHostEnvironment _environment;

        public StaffController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper, IWebHostEnvironment environment)
            : base(httpContextAccessor, AppSettings)
        {
            _mapper = mapper;
            _token = httpContextAccessor.HttpContext.Session.GetString("token");
            _userId = httpContextAccessor.HttpContext.Session.GetString("EmailAddress");
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Upserts
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(int staffId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            StaffViewModel viewModel = new StaffViewModel();

            if (staffId == 0)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");
                viewModel.Roles = await this.GetRoleSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                viewModel.GenderList = Common.ListHelpers.GenderList;
                viewModel.States = Common.ListHelpers.States;
                viewModel.Countries = Common.ListHelpers.Countries;

                return View(viewModel);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, staffId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<StaffResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in StaffUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                    }

                    var staff = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    var data = JsonConvert.DeserializeObject<StaffRequest>(staff.Data.ToString());
                    viewModel.StaffId = data.StaffId;
                    viewModel.FirstName = data.FirstName;
                    viewModel.MiddleName = data.MiddleName;
                    viewModel.LastName = data.LastName;
                    viewModel.Address = data.Address;
                    viewModel.Address2 = data.Address2;
                    viewModel.City = data.City;
                    viewModel.State = data.State;
                    viewModel.PostalCode = data.PostalCode;
                    viewModel.Country = data.Country;
                    //viewModel.RoleId = (int)data.RoleId;
                    viewModel.EmailAddress = data.EmailAddress;
                    viewModel.PhoneNumber = data.PhoneNumber;
                    viewModel.Gender = data.Gender;
                    viewModel.FirstAid = (bool)data.FirstAid;
                    viewModel.CPR = (bool)data.CPR;
                    viewModel.Kriyaban = (bool)data.Kriyaban;
                    viewModel.LessonStudent = (bool)data.LessonStudent;
                    viewModel.AcceptTerms = (bool)data.AcceptTerms;
                    viewModel.Notes = data.Notes;
                    //viewModel.CanSchedule = (bool)data.CanSchedule;
                    //viewModel.CanOrderSupplies = (bool)data.CanOrderSupplyItems;
                    //viewModel.CanViewReports = (bool)data.CanViewReports;
                    //viewModel.CanSendMessages = (bool)data.CanSendMessages;
                    //viewModel.IsVerified = (bool)data.IsVerified;
                    viewModel.VerifiedDate = (DateTime)data.VerifiedDate;
                    viewModel.RememberMe = data.RememberMe;
                    viewModel.CredentialIds = data.CredentialIds;
                    //viewModel.RoleIds = data.RoleIds;
                    viewModel.IsActive = data.IsActive;
                    viewModel.IsHidden = data.IsHidden;
                    viewModel.CreatedBy = data.CreatedBy;
                    viewModel.CreatedDate = data.CreatedDate;
                    viewModel.UpdatedDate = data.UpdatedDate;
                    viewModel.UpdatedBy = data.UpdatedBy;
                    viewModel.StaffFileName = data.StaffFileName;
                    viewModel.PrevStaffFileName = viewModel.StaffFileName;
                    viewModel.StaffByte = data.StaffImage;
                    viewModel.PropertyId = data.PropertyId;
                    viewModel.Roles = await this.GetRoleSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                    viewModel.GenderList = Common.ListHelpers.GenderList;
                    viewModel.States = Common.ListHelpers.States;
                    viewModel.Countries = Common.ListHelpers.Countries;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in StaffUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/StaffModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(StaffViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                bool fileChange = false;
                StaffRequest staff = new StaffRequest();

                if (ModelState.IsValid)
                {
                    if (viewModel.StaffId > 0)
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

                    staff.StaffId = viewModel.StaffId;
                    staff.FirstName = viewModel.FirstName;
                    staff.MiddleName = viewModel.MiddleName;
                    staff.LastName = viewModel.LastName;
                    staff.Address = viewModel.Address;
                    staff.Address2 = viewModel.Address2;
                    staff.City = viewModel.City;
                    staff.State = viewModel.State;
                    staff.PostalCode = viewModel.PostalCode;
                    staff.Country = viewModel.Country;
                    //staff.RoleId = viewModel.RoleId;
                    staff.EmailAddress = viewModel.EmailAddress;
                    staff.PhoneNumber = viewModel.PhoneNumber;
                    staff.Gender = viewModel.Gender;
                    staff.FirstAid = viewModel.FirstAid;
                    staff.CPR = viewModel.CPR;
                    staff.Kriyaban = viewModel.Kriyaban;
                    staff.LessonStudent = viewModel.LessonStudent;
                    staff.AcceptTerms = viewModel.AcceptTerms;
                    staff.Notes = viewModel.Notes;
                    staff.CanSchedule = viewModel.CanSchedule;
                    staff.CanOrderSupplyItems = viewModel.CanOrderSupplies;
                    staff.CanViewReports = viewModel.CanViewReports;
                    staff.CanSendMessages = viewModel.CanSendMessages;
                    staff.IsVerified = viewModel.IsVerified;
                    staff.VerifiedDate = viewModel.VerifiedDate;
                    staff.RememberMe = viewModel.RememberMe;
                    staff.CredentialIds = viewModel.CredentialIds;
                    //staff.RoleIds = viewModel.RoleIds;
                    staff.IsActive = viewModel.IsActive;
                    staff.IsHidden = viewModel.IsHidden;
                    staff.CreatedBy = viewModel.CreatedBy;
                    staff.CreatedDate = viewModel.CreatedDate;
                    staff.UpdatedDate = viewModel.UpdatedDate;
                    staff.UpdatedBy = viewModel.UpdatedBy;
                    staff.StaffFileName = viewModel.StaffFileName;
                    staff.StaffImage = viewModel.StaffByte;
                    staff.PropertyId = viewModel.PropertyId;

                    if (fileChange)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.StaffImage.FileName);
                        string extension = Path.GetExtension(viewModel.StaffImage.FileName);
                        fileName = fileName + DateTime.UtcNow.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
                        MemoryStream ms = null;
                        byte[] buffer = new byte[16 * 1024];

                        using (fs = System.IO.File.Create(path))
                        {
                            await viewModel.StaffImage.CopyToAsync(fs);

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

                        staff.StaffFileName = viewModel.StaffImage.FileName;
                        staff.StaffImage = ms.ToArray();
                        System.IO.File.Delete(path);
                    }

                    if (viewModel.StaffId == 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            staff.CreatedBy = GetStringSession("EmailAddress");
                            staff.CreatedDate = DateTime.UtcNow;
                            var data = JsonConvert.SerializeObject(staff);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Bearer token is null. Please contact support.", viewModel.StaffId);

                                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PostAsync(string.Format("{0}/Staff/PostAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New StaffUpsert(StaffViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "Staff successfully created";
                        }
                    }
                    else
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            staff.UpdatedBy = GetStringSession("EmailAddress");
                            staff.UpdatedDate = DateTime.UtcNow;
                            var data = JsonConvert.SerializeObject(staff);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Bearer token is null. Please contact support.", viewModel.StaffId);

                                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PutAsync(string.Format("{0}/Staff/PutAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New StaffUpsert(StaffViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "Staff successfully updated";
                        }
                    }
                }
                else
                {
                    viewModel.Roles = await this.GetRoleSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                    viewModel.GenderList = Common.ListHelpers.GenderList;
                    viewModel.States = Common.ListHelpers.States;
                    viewModel.Countries = Common.ListHelpers.Countries;

                    return View(viewModel);
                }

                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Staff);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(StaffViewModel viewModel): {0}. Message: '{1}'. Please contact support.", viewModel.StaffId, ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> StaffGet(bool isActive = true)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "Error occuured in StaffGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?propertyId={1}&userId={2}", this.Uri, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<StaffRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> StaffGetById(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            StaffViewModel viewModel = new StaffViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<StaffRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<StaffRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> Delete(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in StaffGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Staff/DeleteAsync?staffId={1}&propertyId={2}&userId='{3}'", this.Uri, staffId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in StaffDelete. Please contact support.");

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in StaffDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
        }

        #region Helpers
        public IActionResult StaffModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }

        private void AddErrors(string error)
        {
            ModelState.AddModelError(string.Empty, error);
        }
        #endregion
    }
}