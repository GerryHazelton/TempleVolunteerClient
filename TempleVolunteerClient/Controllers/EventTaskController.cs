

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
    public class EventTaskController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public EventTaskController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
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
        [HttpGet("EventTaskUpsert")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(int eventTaskTaskId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            EventTaskViewModel viewModel = new EventTaskViewModel();

            if (eventTaskTaskId == 0)
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/EventTask/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, eventTaskTaskId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<EventTaskResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in EventTaskUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
                    }

                    var eventTaskTask = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<EventTaskViewModel>(JsonConvert.DeserializeObject<EventTaskRequest>(eventTaskTask.Data.ToString()));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in EventTaskUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/EventTaskModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost("EventTaskUpsert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(EventTaskViewModel viewModel)
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

                    if (viewModel.EventTaskId == 0)
                    {
                        var eventTaskTaskRequest = _mapper.Map<EventTaskRequest>(viewModel);
                        string stringData = JsonConvert.SerializeObject(eventTaskTaskRequest);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/EventTask/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        string stringData = JsonConvert.SerializeObject(viewModel);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/EventTask/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in EventTaskUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(StaffViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EventTaskGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in EventTaskGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/EventTask/StaffModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<EventTaskRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EventTaskGetById(int staffId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<EventTaskRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<EventTaskRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/EventTask/EventTaskModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> EventTaskDelete(int eventTaskTaskId, int propertyId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in EventTaskGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/EventTask/DeleteAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, eventTaskTaskId, propertyId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in EventTaskDelete. Please contact support.");

                        return RedirectPermanent("/EventTask/EventTaskModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in EventTaskDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/EventTask/EventTaskModalPopUp");
            }
        }

        #region Helpers
        public IActionResult EventTaskModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}