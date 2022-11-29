

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
    public class SupplyItemController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;
        private IWebHostEnvironment _environment;

        public SupplyItemController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper, IWebHostEnvironment environment)
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
        public async Task<IActionResult> Upsert(int supplyItemId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            SupplyItemViewModel viewModel = new SupplyItemViewModel();

            if (supplyItemId == 0)
            {
                viewModel.CreatedDate = DateTime.UtcNow;
                viewModel.CreatedBy = GetStringSession("EmailAddress");
                viewModel.PropertyId = GetIntSession("PropertyId");
                viewModel.Categories = await this.GetCategorySelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);

                return View(viewModel);
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, supplyItemId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<SupplyItemResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in SupplyItemUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                    }

                    var supplyItem = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    var data = JsonConvert.DeserializeObject<SupplyItemRequest>(supplyItem.Data.ToString());
                    viewModel.SupplyItemId = data.SupplyItemId;
                    viewModel.Name = data.Name;
                    viewModel.Description = data.Description;
                    viewModel.CategoryId = data.CategoryId;
                    viewModel.Quantity = data.Quantity;
                    viewModel.BinNumber = data.BinNumber;
                    viewModel.Note = data.Note;
                    viewModel.IsActive = data.IsActive;
                    viewModel.IsHidden = data.IsHidden;
                    viewModel.CreatedBy = data.CreatedBy;
                    viewModel.CreatedDate = data.CreatedDate;
                    viewModel.UpdatedDate = data.UpdatedDate;
                    viewModel.UpdatedBy = data.UpdatedBy;
                    viewModel.SupplyItemFileName = data.SupplyItemFileName;
                    viewModel.PrevSupplyItemFileName = viewModel.SupplyItemFileName;
                    viewModel.SupplyItemByte = data.SupplyItemImage;
                    viewModel.PropertyId = data.PropertyId;
                    viewModel.Categories = await this.GetCategorySelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in SupplyItemUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/SupplyItemModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(SupplyItemViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                bool fileChange = false;
                SupplyItemRequest supplyItem = new SupplyItemRequest();

                if (ModelState.IsValid)
                {
                    if (viewModel.SupplyItemId > 0)
                    {
                        if (String.IsNullOrEmpty(viewModel.PrevSupplyItemFileName) && !String.IsNullOrEmpty(viewModel.SupplyItemFileName))
                        {
                            fileChange = true;
                        }

                        if (!String.IsNullOrEmpty(viewModel.PrevSupplyItemFileName) && !String.IsNullOrEmpty(viewModel.SupplyItemFileName))
                        {
                            fileChange = !viewModel.PrevSupplyItemFileName.Trim().ToLower().Equals(viewModel.SupplyItemFileName.Trim().ToLower());
                        }
                    }

                    supplyItem.SupplyItemId = viewModel.SupplyItemId;
                    supplyItem.Name = viewModel.Name;
                    supplyItem.Description = viewModel.Description;
                    supplyItem.CategoryId = viewModel.CategoryId;
                    supplyItem.Quantity = viewModel.Quantity;
                    supplyItem.BinNumber = viewModel.BinNumber;
                    supplyItem.Note = viewModel.Note;
                    supplyItem.IsActive = viewModel.IsActive;
                    supplyItem.IsHidden = viewModel.IsHidden;
                    supplyItem.CreatedBy = viewModel.CreatedBy;
                    supplyItem.CreatedDate = viewModel.CreatedDate;
                    supplyItem.UpdatedDate = viewModel.UpdatedDate;
                    supplyItem.UpdatedBy = viewModel.UpdatedBy;
                    supplyItem.SupplyItemFileName = viewModel.SupplyItemFileName;
                    supplyItem.SupplyItemImage = viewModel.SupplyItemByte;
                    supplyItem.PropertyId = viewModel.PropertyId;

                    if (fileChange)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.SupplyItemImage.FileName);
                        string extension = Path.GetExtension(viewModel.SupplyItemImage.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
                        MemoryStream ms = null;
                        byte[] buffer = new byte[16 * 1024];

                        using (fs = System.IO.File.Create(path))
                        {
                            await viewModel.SupplyItemImage.CopyToAsync(fs);

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

                        supplyItem.SupplyItemFileName = viewModel.SupplyItemImage.FileName;
                        supplyItem.SupplyItemImage = ms.ToArray();
                        System.IO.File.Delete(path);
                    }

                    if (viewModel.SupplyItemId == 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            supplyItem.CreatedBy = GetStringSession("EmailAddress");
                            supplyItem.CreatedDate = DateTime.Now;
                            var data = JsonConvert.SerializeObject(supplyItem);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Bearer token is null. Please contact support.", viewModel.SupplyItemId);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PostAsync(string.Format("{0}/SupplyItem/PostAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New SupplyItemUpsert(SupplyItemViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "SupplyItem successfully created";
                        }
                    }
                    else
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            supplyItem.UpdatedBy = GetStringSession("EmailAddress");
                            supplyItem.UpdatedDate = DateTime.Now;
                            var data = JsonConvert.SerializeObject(supplyItem);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Bearer token is null. Please contact support.", viewModel.SupplyItemId);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PutAsync(string.Format("{0}/SupplyItem/PutAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New SupplyItemUpsert(SupplyItemViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "SupplyItem successfully updated";
                        }
                    }
                }
                else
                {
                    viewModel.Categories = await this.GetCategorySelectList(GetIntSession("PropertyId"), GetStringSession("EmailAddress"), true, false);

                    return View(viewModel);
                }

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.SupplyItem);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(SupplyItemViewModel viewModel): {0}. Message: '{1}'. Please contact support.", viewModel.SupplyItemId, ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in SupplyItemGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&userId={2}", this.Uri, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<SupplyItemRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemGetById(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            SupplyItemViewModel viewModel = new SupplyItemViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<SupplyItemRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<SupplyItemRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> Delete(int supplyItemId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in SupplyItemGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/SupplyItem/DeleteAsync?supplyItemId={1}&propertyId={2}&userId='{3}'", this.Uri, supplyItemId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in SupplyItemDelete. Please contact support.");

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in SupplyItemDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        #region Helpers
        public IActionResult SupplyItemModalPopUp(ModalType type)
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