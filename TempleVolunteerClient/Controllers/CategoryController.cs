

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
    public class CategoryController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public CategoryController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
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
        public async Task<IActionResult> Upsert(int categoryId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            CategoryViewModel viewModel = new CategoryViewModel();

            if (categoryId == 0)
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, categoryId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<CategoryResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CategoryUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
                    }

                    var category = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<CategoryViewModel>(JsonConvert.DeserializeObject<CategoryRequest>(category.Data.ToString()));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in CategoryUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/CategoryModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CategoryViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            if (!ModelState.IsValid)
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

                    CategoryRequest categoryRequest = _mapper.Map<CategoryRequest>(viewModel);
                    string stringData;
                    StringContent contentData;
                    HttpResponseMessage response;

                    if (viewModel.CategoryId == 0)
                    {
                        categoryRequest.CreatedDate = DateTime.UtcNow;
                        categoryRequest.CreatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(categoryRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/Category/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        categoryRequest.CreatedDate = viewModel.CreatedDate;
                        categoryRequest.CreatedBy = viewModel.CreatedBy;
                        categoryRequest.UpdatedDate = DateTime.UtcNow;
                        categoryRequest.UpdatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(categoryRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/Category/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CategoryUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
                    }
                }

                TempData["ModalMessage"] = viewModel.CategoryId == 0 ? string.Format("Category successfully created.") : string.Format("Category successfully updated.");

                return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Category);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: CategoryUpsert(CategoryViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CategoryGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in CategoryGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetAllAsync?propertyId={1}&userId={2}", this.Uri, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Category/CategoryModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<CategoryRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CategoryGetById(int categoryId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            CategoryViewModel viewModel = new CategoryViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Category/CategoryModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetByIdAsync?id={1}&propertyId={2}&userId={3}", this.Uri, categoryId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Category/CategoryModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<CategoryRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<CategoryRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Category/CategoryModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> Delete(int categoryId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in CategoryGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Category/CategoryModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Category/DeleteAsync?categoryId={1}&propertyId={2}&userId='{3}'", this.Uri, categoryId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CategoryDelete. Please contact support.");

                        return RedirectPermanent("/Category/CategoryModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in CategoryDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Category/CategoryModalPopUp");
            }
        }

        #region Helpers
        public IActionResult CategoryModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}