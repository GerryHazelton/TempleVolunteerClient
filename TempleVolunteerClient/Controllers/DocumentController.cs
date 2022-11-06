

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
    public class DocumentController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public DocumentController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
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
        [HttpGet("DocumentUpsert")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(int documentId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            DocumentViewModel viewModel = new DocumentViewModel();

            if (documentId == 0)
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, documentId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<DocumentResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in DocumentUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                    }

                    var document = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<DocumentViewModel>(JsonConvert.DeserializeObject<DocumentRequest>(document.Data.ToString()));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in DocumentUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/DocumentModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost("DocumentUpsert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(DocumentViewModel viewModel)
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

                    if (viewModel.DocumentId == 0)
                    {
                        var documentRequest = _mapper.Map<DocumentRequest>(viewModel);
                        string stringData = JsonConvert.SerializeObject(documentRequest);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/Document/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        string stringData = JsonConvert.SerializeObject(viewModel);
                        var contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/Document/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in DocumentUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: StaffUpsert(StaffViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        //[AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DocumentGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in DocumentGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Document/StaffModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<DocumentRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DocumentGetById(int staffId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Document/DocumentModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Document/DocumentModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<DocumentRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<DocumentRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> DocumentDelete(int documentId, int propertyId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Document/DocumentModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Document/DeleteAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, documentId, propertyId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in DocumentDelete. Please contact support.");

                        return RedirectPermanent("/Document/DocumentModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in DocumentDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Document/DocumentModalPopUp");
            }
        }

        #region Helpers
        public IActionResult DocumentModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}