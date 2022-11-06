

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
    public class RoleController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public RoleController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
            : base(httpContextAccessor, AppSettings)
        {
            _mapper = mapper;
            _token = httpContextAccessor.HttpContext.Session.GetString("token");
            _userId = httpContextAccessor.HttpContext.Session.GetString("EmailAddress");
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Upserts
        [HttpGet("RoleUpsert")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(int roleTaskId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            RoleViewModel viewModel = new RoleViewModel();

            if (roleTaskId == 0)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");

                return View(viewModel);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Role/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, roleTaskId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<RoleResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in RoleUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
                    }

                    var roleTask = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<RoleViewModel>(JsonConvert.DeserializeObject<RoleRequest>(roleTask.Data.ToString()));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in RoleUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/RoleModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost("RoleUpsert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RoleViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            if (!ModelState.IsValid)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response;

                    if (viewModel.RoleId == 0)
                    {
                        var roleTaskRequest = _mapper.Map<RoleRequest>(viewModel);
                        string stringData = JsonConvert.SerializeObject(roleTaskRequest);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/Role/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        string stringData = JsonConvert.SerializeObject(viewModel);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/Role/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in RoleUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(StaffViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RoleGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in RoleGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Role/StaffModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<RoleRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RoleGetById(int staffId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Role/RoleModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Role/RoleModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<RoleRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<RoleRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Role/RoleModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> RoleDelete(int roleTaskId, int propertyId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in RoleGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Role/RoleModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Role/DeleteAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, roleTaskId, propertyId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in RoleDelete. Please contact support.");

                        return RedirectPermanent("/Role/RoleModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in RoleDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Role/RoleModalPopUp");
            }
        }

        #region Helpers
        public IActionResult RoleModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}