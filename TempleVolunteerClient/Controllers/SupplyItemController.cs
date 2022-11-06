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
    public class SupplyItemController : CustomController
    {
        private readonly IMapper _mapper;
        private IWebHostEnvironment _environment;

        public SupplyItemController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper, IWebHostEnvironment environment)
            : base(httpContextAccessor, AppSettings)
        {
            _mapper = mapper;
            _environment = environment;
        }

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            ViewData["ModalMessage"] = TempData["ModalMessage"];
            SupplyItemViewModel viewModel = new SupplyItemViewModel();
            //viewModel.EmailAddress = GetStringSession("EmailAddress");
            //viewModel.LoggedInSupplyItem = this.GetIntSession("SupplyItemId");
            return View(viewModel);
        }

        [HttpGet("SupplyItemUpsert")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemUpsert()
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                SupplyItemViewModel viewModel = new SupplyItemViewModel();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int? supplyItemId). Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemUpsert(int supplyItemId)
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
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Bearer token is null. Please contact support.", supplyItemId);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&userId='{2}'", this.Uri, supplyItemId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId). Unable to get SupplyItemId: {0}. Please contact support.", supplyItemId);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    var supplyItem = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<SupplyItemViewModel>(JsonConvert.DeserializeObject<SupplyItemRequest>(supplyItem.Data.ToString()));
                    //viewModel.GenderList = Common.ListHelpers.GenderList;
                    //viewModel.RoleList = await this.GetCustomRoles(viewModel.RoleId);
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Message: '{1}'. Please contact support.", supplyItemId, ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }

            return View(viewModel);
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<SupplyItemRequest> GetSupplyItem(int supplyItemId)
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&userId='{2}'", this.Uri, supplyItemId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        throw new Exception("You are unauthorized");
                    }

                    var supplyItem = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    SupplyItemViewModel viewModel = _mapper.Map<SupplyItemViewModel>(JsonConvert.DeserializeObject<SupplyItemRequest>(supplyItem.Data.ToString()));

                    return _mapper.Map<SupplyItemRequest>(viewModel);
                }
            }
            catch
            {
                throw new Exception("You are unauthorized");
            }
        }

        //[HttpGet]
        //public async Task<IList<SelectListItem>> GetAllSupplyItem()
        //{
        //    if (!IsAuthenticated()) return (IList<SelectListItem>)RedirectPermanent("/Account/LogOut");

        //    return await this.GetCustomSupplyItem(0);
        //}

        [HttpPost("SupplyItemUpsert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplyItemUpsert(SupplyItemViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                if (ModelState.IsValid)
                {
                    bool updateImage = viewModel.SupplyItemImageFile != null ? true : false;
                    MemoryStream ms = null;
                    var supplyItem = _mapper.Map<SupplyItemRequest>(viewModel);

                    if (updateImage)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.SupplyItemImageFile.FileName);
                        string extension = Path.GetExtension(viewModel.SupplyItemImageFile.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
                        byte[] buffer = new byte[16 * 1024];

                        using (fs = System.IO.File.Create(path))
                        {
                            //await viewModel.SupplyItemImageFile.CopyToAsync(fs);

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
                    else
                    {
                        if (viewModel.SupplyItemImageFile != null)
                        {
                            //viewModel.SupplyItemPrevImage = viewModel.SupplyItemImage;
                        }
                    }

                    if (viewModel.SupplyItemId == 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            if (updateImage)
                            {
                                //supplyItem.SupplyItemImageFileName = viewModel.SupplyItemImageFile.FileName;
                                //supplyItem.SupplyItemImage = ms.ToArray();
                            }

                            supplyItem.CreatedBy = GetStringSession("EmailAddress");
                            supplyItem.CreatedDate = DateTime.Now;
                            //supplyItem.Password = this.TempPassword;
                            var data = JsonConvert.SerializeObject(supplyItem);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Bearer token is null. Please contact support.", viewModel.SupplyItemId);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                            }

                            HttpResponseMessage response = await client.PostAsync(string.Format("{0}/SupplyItem/PostAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New SupplyItemUpsert(SupplyItemViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                            }

                            TempData["ModalMessage"] = "SupplyItem successfully created";
                        }
                    }
                    else
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            if (!updateImage)
                            {
                                //if (viewModel.SupplyItemPrevImage != null)
                                //{
                                //    supplyItem.SupplyItemImageFileName = viewModel.SupplyItemFileName;
                                //    //supplyItem.SupplyItemImage = viewModel.SupplyItemPrevImage;
                                //}
                            }
                            else
                            {
                                //supplyItem.SupplyItemImageFileName = viewModel.SupplyItemImageFile.FileName;
                                //supplyItem.SupplyItemImage = ms.ToArray();
                            }

                            supplyItem.UpdatedBy = GetStringSession("EmailAddress");
                            supplyItem.UpdatedDate = DateTime.Now;
                            //supplyItem.UnlockUser = viewModel.UnlockUser;
                            var data = JsonConvert.SerializeObject(supplyItem);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(int supplyItemId): {0}. Bearer token is null. Please contact support.", viewModel.SupplyItemId);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                            }

                            HttpResponseMessage response = await client.PutAsync(string.Format("{0}/SupplyItem/PutAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New SupplyItemUpsert(SupplyItemViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                            }

                            TempData["ModalMessage"] = "SupplyItem successfully updated";
                        }
                    }
                }
                else
                {
                    //viewModel.RoleList = await this.GetCustomRoles(viewModel.RoleId);
                    return View(viewModel);
                }

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemUpsert(SupplyItemViewModel viewModel): {0}. Message: '{1}'. Please contact support.", viewModel.SupplyItemId, ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemGet(bool registerCheck = false)
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
                            TempData["ModalMessage"] = "Error occurred: SupplyItemGet(). Bearer token is null. Please contact support.";

                            return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                        }
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?userId={1}", this.Uri, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemGet(). Message: '{0}'. Please contact support.", response.RequestMessage);

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
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemGet(). Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SupplyItemGetById(int supplyItemId)
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
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemGet(int supplyItemId): {0}. Bearer token is null. Please contact support.", supplyItemId);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem?id={1}&&userId", this.Uri, supplyItemId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemGet(int supplyItemId): {0}. Message: '{1}'. Please contact support.", supplyItemId, response.RequestMessage);

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
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemGet(int supplyItemId): {0}. Message: '{1}'. Please contact support.", supplyItemId, ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> SupplyItemDelete(int supplyItemId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                if (this.GetIntSession("SupplyItemId") == supplyItemId)
                {
                    return RedirectPermanent("/SupplyItem/CannotDeleteCurrentUser");
                }

                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemDelete(int supplyItemId): {0}. Bearer token is null. Please contact support.", supplyItemId);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/SupplyItem/DeleteAsync?id={1}&userId='{2}'", this.Uri, supplyItemId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemDelete(int supplyItemId): {0}. Message: '{1}'. Please contact support.", supplyItemId, response.RequestMessage);

                        return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: SupplyItemDelete(int supplyItemId): {0}. Message: '{1}'. Please contact support.", supplyItemId, ex.Message);

                return RedirectPermanent("/SupplyItem/SupplyItemModalPopUp");
            }
        }

        #region Helpers
        public IActionResult SupplyItemModalPopUp()
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