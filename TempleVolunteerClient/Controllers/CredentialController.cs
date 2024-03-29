﻿

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
    public class CredentialController : CustomController
    {
        private readonly IMapper _mapper;
        private string _token;
        private string _userId;

        public CredentialController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings, IMapper mapper)
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
        public async Task<IActionResult> Upsert(int credentialId = 0)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            CredentialViewModel viewModel = new CredentialViewModel();

            if (credentialId == 0)
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Credential/GetByIdAsync?id={1}&propertyId={2}&userId='{3}'", this.Uri, credentialId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));
                    var responseDeserialized = JsonConvert.DeserializeObject<CredentialResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CredentialUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
                    }

                    var credential = JsonConvert.DeserializeObject<ServiceResponse>(response.Content.ReadAsStringAsync().Result);
                    viewModel = _mapper.Map<CredentialViewModel>(JsonConvert.DeserializeObject<CredentialRequest>(credential.Data.ToString()));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in CredentialUpsert. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Account/CredentialModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CredentialViewModel viewModel)
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

                    CredentialRequest credentialRequest = _mapper.Map<CredentialRequest>(viewModel);
                    string stringData;
                    StringContent contentData;
                    HttpResponseMessage response;

                    if (viewModel.CredentialId == 0)
                    {
                        credentialRequest.CreatedDate = DateTime.UtcNow;
                        credentialRequest.CreatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(credentialRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PostAsync(string.Format("{0}/Credential/PostAsync", this.Uri), contentData).Result;
                    }
                    else
                    {
                        credentialRequest.CreatedDate = viewModel.CreatedDate;
                        credentialRequest.CreatedBy = viewModel.CreatedBy;
                        credentialRequest.UpdatedDate = DateTime.UtcNow;
                        credentialRequest.UpdatedBy = this.GetStringSession("EmailAddress");
                        stringData = JsonConvert.SerializeObject(credentialRequest);
                        contentData = new StringContent(stringData, Encoding.UTF8, contentType.ToString());
                        response = client.PutAsync(string.Format("{0}/Credential/PutAsync", this.Uri), contentData).Result;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<MyProfileResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CredentialUpsert. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
                    }
                }

                TempData["ModalMessage"] = viewModel.CredentialId == 0 ? string.Format("Credential successfully created.") : string.Format("Credential successfully updated.");

                return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Credential);
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred: CredentialUpsert(CredentialViewModel viewModel): {0}. Message: '{1}'. Please contact support.", this.GetStringSession("EmailAddress"), ex.Message);

                return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
            };
        }
        #endregion

        #region Getters
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CredentialGet(bool isActive = true)
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
                        TempData["ModalMessage"] = "Error occuured in CredentialGet. Bearer token is null. Please contact support.";

                        return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Credential/GetAllAsync?propertyId={1}&userId={2}", this.Uri, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Credential/CredentialModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

                    return Json(
                        new { data = JsonConvert.DeserializeObject<List<CredentialRequest>>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
            }
        }

        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CredentialGetById(int staffId)
        {
            if (!IsAuthenticated()) return RedirectPermanent("/Account/LogOut");

            CredentialViewModel viewModel = new CredentialViewModel();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Credential/CredentialModalPopUp");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Credential?id={1}&&userId", this.Uri, staffId, GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Message: '{0}'. Please contact support.", response.RequestMessage);

                        return RedirectPermanent("/Credential/CredentialModalPopUp");
                    }

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var json = Json(new { data = JsonConvert.DeserializeObject<CredentialRequest>(data.Data.ToString()) });

                    return Json(
                        new { data = JsonConvert.DeserializeObject<CredentialRequest>(data.Data.ToString()) }
                    );
                }
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Credential/CredentialModalPopUp?type=" + ModalType.Error);
            }
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> Delete(int credentialId)
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
                        TempData["ModalMessage"] = string.Format("Error occuured in CredentialGet. Bearer token is null. Please contact support.");

                        return RedirectPermanent("/Credential/CredentialModalPopUp");
                    }

                    HttpResponseMessage response = await client.DeleteAsync(string.Format("{0}/Credential/DeleteAsync?credentialId={1}&propertyId={2}&userId='{3}'", this.Uri, credentialId, GetIntSession("PropertyId"), GetStringSession("EmailAddress")));

                    if (!response.IsSuccessStatusCode || String.IsNullOrEmpty(response.Content.ReadAsStringAsync().Result))
                    {
                        TempData["ModalMessage"] = string.Format("Error occurred in CredentialDelete. Please contact support.");

                        return RedirectPermanent("/Credential/CredentialModalPopUp");
                    }
                }

                return Json(new { success = true, message = "Delete successful" });
            }
            catch (Exception ex)
            {
                TempData["ModalMessage"] = string.Format("Error occurred in CredentialDelete. Message: '{0}'. Please contact support.", ex.Message);

                return RedirectPermanent("/Credential/CredentialModalPopUp");
            }
        }

        #region Helpers
        public IActionResult CredentialModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}