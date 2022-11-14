

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
    public class AreaController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public AreaController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
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
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(int areaId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            AreaViewModel viewModel = new AreaViewModel();

            if (areaId == 0)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");
                viewModel.SupplyItems = await this.GetSupplyItemSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                viewModel.EventTasks = await this.GetEventTaskSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);

                return View(viewModel);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, areaId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<AreaResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in AreaUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
                    }

                    var area = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<AreaViewModel>(JsonConvert.DeserializeObject<AreaRequest>(area.Data.ToString()));
                    viewModel.SupplyItems = await this.GetSupplyItemSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                    viewModel.EventTasks = await this.GetEventTaskSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in AreaUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/AreaModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AreaViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            if (!ModelState.IsValid)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");
                viewModel.SupplyItems = await this.GetSupplyItemSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                viewModel.EventTasks = await this.GetEventTaskSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);

                return View(viewModel);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    AreaRequest areaRequest = _mapper.Map<AreaRequest>(viewModel);
                    string stringData;
                    StringContent contentData;
                    HttpResponseMessage response;

                    if (viewModel.AreaId == 0)
                    {
                        areaRequest.CreatedDate = DateTime.UtcNow;
                        areaRequest.CreatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(areaRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/Area/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        areaRequest.CreatedDate = viewModel.CreatedDate;
                        areaRequest.CreatedBy = viewModel.CreatedBy;
                        areaRequest.UpdatedDate = DateTime.UtcNow;
                        areaRequest.UpdatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(areaRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/Area/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in AreaUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
                    }

                    viewModel.SupplyItems = await this.GetSupplyItemSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                    viewModel.EventTasks = await this.GetEventTaskSelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                }

                TempData["ModalMessage"] = viewModel.AreaId == 0 ? string.Format("Area successfully created.") : string.Format("Area successfully updated.");

                return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Area);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: AreaUpsert(AreaViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AreaGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in AreaGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Area/AreaModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<AreaRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AreaGetById(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            AreaViewModel viewModel = new AreaViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Area/AreaModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Area/AreaModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<AreaRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<AreaRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Area/AreaModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> Delete(int areaId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in AreaGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Area/AreaModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Area/DeleteAsync?areaId={1}&propertyId={2}&userId='{3}'", this.Uri, areaId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in AreaDelete. Please contact support.");

                        return RedirectPermanent("/Area/AreaModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in AreaDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Area/AreaModalPopUp");
            }
        }

        #region Helpers
        public IActionResult AreaModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}