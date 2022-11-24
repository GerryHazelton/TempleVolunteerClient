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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TempleVolunteerClient.Controllers
{
    public class StaffController : CustomController
    {
        private readonly IMapper _mapper;
        private IWebHostEnvironment _environment;

        public StaffController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper, IWebHostEnvironment environment)
            : base(httpContextAccessor, AppSettings)
        {
            _mapper = mapper;
            _environment = environment;
        }

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            ViewData["ModalMessage"] = TempData["ModalMessage"];
            StaffViewModel viewModel = new StaffViewModel();
            viewModel.EmailAddress = GetStringSession("EmailAddress");
            viewModel.LoggedInStaff = this.GetIntSession("StaffId");
            return View(viewModel);
        }

        [HttpGet("StaffUpsert")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> StaffUpsert()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                StaffViewModel viewModel = new StaffViewModel();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int? staffId). Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> StaffUpsert(int staffId)
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
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Bearer token is null. Please contact support.", staffId);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetByIdAsync?id={1}&userId='{2}'", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId). Unable to get StaffId: {0}. Please contact support.", staffId);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    var staff = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<StaffViewModel>(JsonConvert.DeserializeObject<StaffRequest>(staff.Data.ToString()));
                    viewModel.GenderList = Common.ListHelpers.GenderList;
                    //viewModel.RoleList = await this.GetCustomRoles(viewModel.RoleId);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Message: '{1}'. Please contact support.", staffId, ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }

            return View(viewModel);
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<StaffRequest> GetStaff(int staffId)
        {
            if (!IsAuthenticated())
            {
                throw new Exception("You are unauthorized");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        throw new Exception("You are unauthorized");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetByIdAsync?id={1}&userId='{2}'", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        throw new Exception("You are unauthorized");
                    }

                    var staff = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    StaffViewModel viewModel = _mapper.Map<StaffViewModel>(JsonConvert.DeserializeObject<StaffRequest>(staff.Data.ToString()));

                    return _mapper.Map<StaffRequest>(viewModel);
                }
            }
            catch
            {
                throw new Exception("You are unauthorized");
            }
        }

        [HttpPost("StaffUpsert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffUpsert(StaffViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                if (ModelState.IsValid)
                {
                    bool updateImage = viewModel.StaffImage != null ? true : false;
                    MemoryStream ms = null;
                    var staff = _mapper.Map<StaffRequest>(viewModel);

                    if (updateImage)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.StaffImage.FileName);
                        string extension = Path.GetExtension(viewModel.StaffImage.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
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

                        System.IO.File.Delete(path);
                    }

                    if (viewModel.StaffId == 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            staff.CreatedBy = GetStringSession("EmailAddress");
                            staff.CreatedDate = DateTime.Now;
                            staff.Password = this.TempPassword;
                            var data = JsonConvert.SerializeObject(staff);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Bearer token is null. Please contact support.", viewModel.StaffId);

                                return RedirectPermanent("/Staff/StaffModalPopUp");
                            }

                            HttpResponseMessage response = await client.PostAsync(string.Format("{0}/Staff/PostAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New StaffUpsert(StaffViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Staff/StaffModalPopUp");
                            }

                            TempData["ModalMessage"] = "Staff successfully created";
                        }
                    }
                    else
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            staff.UpdatedBy = GetStringSession("EmailAddress");
                            staff.UpdatedDate = DateTime.Now;
                            staff.UnlockUser = viewModel.UnlockUser;
                            var data = JsonConvert.SerializeObject(staff);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(int staffId): {0}. Bearer token is null. Please contact support.", viewModel.StaffId);

                                return RedirectPermanent("/Staff/StaffModalPopUp");
                            }

                            HttpResponseMessage response = await client.PutAsync(string.Format("{0}/Staff/PutAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New StaffUpsert(StaffViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Staff/StaffModalPopUp");
                            }

                            TempData["ModalMessage"] = "Staff successfully updated";
                        }
                    }
                }
                else
                {
                    //viewModel.RoleList = await this.GetCustomRoles(viewModel.RoleId);
                    return View(viewModel);
                }

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(StaffViewModel viewModel): {0}. Message: '{1}'. Please contact support.", viewModel.StaffId, ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> StaffGet(bool registerCheck = false)
        {
            //if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    if (!registerCheck)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                        if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                        {
                            TempData["ModalMessage"] = "Error occurred: StaffGet(). Bearer token is null. Please contact support.";

                            return RedirectPermanent("/Staff/StaffModalPopUp");
                        }
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffGet(). Message: '{0}'. Please contact support.", response.RequestMessage);

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
                TempData["ModalMessage"] = string.Format("Error occurred: StaffGet(). Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
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
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffGet(int staffId): {0}. Bearer token is null. Please contact support.", staffId);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffGet(int staffId): {0}. Message: '{1}'. Please contact support.", staffId, response.RequestMessage);

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
                TempData["ModalMessage"] = string.Format("Error occurred: StaffGet(int staffId): {0}. Message: '{1}'. Please contact support.", staffId, ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> StaffDelete(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                if (this.GetIntSession("StaffId") == staffId)
                {
                    return RedirectPermanent("/Staff/CannotDeleteCurrentUser");
                }

                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffDelete(int staffId): {0}. Bearer token is null. Please contact support.", staffId);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Staff/DeleteAsync?id={1}&userId='{2}'", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: StaffDelete(int staffId): {0}. Message: '{1}'. Please contact support.", staffId, response.RequestMessage);

                        return RedirectPermanent("/Staff/StaffModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffDelete(int staffId): {0}. Message: '{1}'. Please contact support.", staffId, ex.Message);

                return RedirectPermanent("/Staff/StaffModalPopUp");
            }
        }

        #region Helpers
        public IActionResult StaffModalPopUp()
        {
            return View();
        }

        public IActionResult RoleModalNoDataPopUp()
        {
            return View();
        }

        public IActionResult CannotDeleteCurrentUser()
        {
            return View();
        }

        private void AddErrors(string error)
        {
            ModelState.AddModelError(string.Empty, error);
        }
        #endregion
    }
}