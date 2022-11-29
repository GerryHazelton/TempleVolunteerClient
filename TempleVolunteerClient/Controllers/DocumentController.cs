

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
        private IWebHostEnvironment _environment;

        public DocumentController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper, IWebHostEnvironment environment)
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
                    var data = JsonConvert.DeserializeObject<DocumentRequest>(document.Data.ToString());
                    viewModel.DocumentId = data.DocumentId;
                    viewModel.Name = data.Name;
                    viewModel.Description = data.Description;
                    viewModel.Note = data.Note;
                    viewModel.IsActive = data.IsActive;
                    viewModel.IsHidden = data.IsHidden;
                    viewModel.CreatedBy = data.CreatedBy;
                    viewModel.CreatedDate = data.CreatedDate;
                    viewModel.UpdatedDate = data.UpdatedDate;
                    viewModel.UpdatedBy = data.UpdatedBy;
                    viewModel.DocumentFileName = data.DocumentFileName;
                    viewModel.PrevDocumentFileName = viewModel.DocumentFileName;
                    viewModel.DocumentByte = data.DocumentImage;
                    viewModel.PropertyId = data.PropertyId;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in DocumentUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/DocumentModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(DocumentViewModel viewModel)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            try
            {
                bool noFileChange = false;
                DocumentRequest document = new DocumentRequest();

                if (ModelState.IsValid)
                {
                    if (viewModel.DocumentId > 0)
                    {
                        noFileChange = viewModel.PrevDocumentFileName.Trim().ToLower().Equals(viewModel.DocumentFileName.Trim().ToLower());
                    }

                    document.DocumentId = viewModel.DocumentId;
                    document.Name = viewModel.Name;
                    document.Description = viewModel.Description;
                    document.Note = viewModel.Note;
                    document.IsActive = viewModel.IsActive;
                    document.IsHidden = viewModel.IsHidden;
                    document.CreatedBy = viewModel.CreatedBy;
                    document.CreatedDate = viewModel.CreatedDate;
                    document.UpdatedDate = viewModel.UpdatedDate;
                    document.UpdatedBy = viewModel.UpdatedBy;
                    document.DocumentFileName = viewModel.DocumentFileName;
                    document.DocumentImage = viewModel.DocumentByte;
                    document.PropertyId = viewModel.PropertyId;

                    if (!noFileChange)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(viewModel.DocumentImage.FileName);
                        string extension = Path.GetExtension(viewModel.DocumentImage.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "\\img\\", fileName);
                        FileStream fs = null;
                        MemoryStream ms = null;
                        byte[] buffer = new byte[16 * 1024];

                        using (fs = System.IO.File.Create(path))
                        {
                            await viewModel.DocumentImage.CopyToAsync(fs);

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

                        document.DocumentFileName = viewModel.DocumentImage.FileName;
                        document.DocumentImage = ms.ToArray();
                        System.IO.File.Delete(path);
                    }

                    if (viewModel.DocumentId == 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            document.CreatedBy = GetStringSession("EmailAddress");
                            document.CreatedDate = DateTime.Now;
                            var data = JsonConvert.SerializeObject(document);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: DocumentUpsert(int documentId): {0}. Bearer token is null. Please contact support.", viewModel.DocumentId);

                                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PostAsync(string.Format("{0}/Document/PostAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New DocumentUpsert(DocumentViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "Document successfully created";
                        }
                    }
                    else
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            document.UpdatedBy = GetStringSession("EmailAddress");
                            document.UpdatedDate = DateTime.Now;
                            var data = JsonConvert.SerializeObject(document);
                            var content = new StringContent(data, Encoding.UTF8, this.ContentType);

                            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(contentType);
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: DocumentUpsert(int documentId): {0}. Bearer token is null. Please contact support.", viewModel.DocumentId);

                                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                            }

                            HttpResponseMessage response = await client.PutAsync(string.Format("{0}/Document/PutAsync", this.Uri), content);

                            if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                            {
                                TempData["ModalMessage"] = string.Format("Error occurred: New DocumentUpsert(DocumentViewModel viewModel). Message: '{0}'. Please contact support.", response.RequestMessage);

                                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
                            }

                            TempData["ModalMessage"] = "Document successfully updated";
                        }
                    }
                }
                else
                {
                    return View(viewModel);
                }

                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Document);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: DocumentUpsert(DocumentViewModel viewModel): {0}. Message: '{1}'. Please contact support.", viewModel.DocumentId, ex.Message);

                return RedirectPermanent("/Document/DocumentModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync?propertyId={1}&userId={2}", this.Uri, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in DocumentGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Document/DocumentModalPopUp");
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

            DocumentViewModel viewModel = new DocumentViewModel();

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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

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
        public async Task<IActionResult> Delete(int documentId)
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

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Document/DeleteAsync?documentId={1}&propertyId={2}&userId='{3}'", this.Uri, documentId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

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

        private void AddErrors(string error)
        {
            ModelState.AddModelError(string.Empty, error);
        }
        #endregion
    }
}