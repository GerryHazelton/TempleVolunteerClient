using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using TempleVolunteerClient.Common;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using static TempleVolunteerClient.Common.ListHelpers;
using iText.Layout.Properties;
using static TempleVolunteerClient.Common.Permissions;

namespace TempleVolunteerClient
{
    public abstract class CustomController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _AppSettings;

        protected CustomController(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> AppSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _AppSettings = AppSettings.Value;
        }

        #region Areas
        protected async Task<IList<AreaRequest>> GetAreas(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<AreaRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<AreaRequest>>(data.Data.ToString());

                    IList<AreaRequest> selectItems = new List<AreaRequest>();

                    foreach (AreaRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new AreaRequest() { AreaId = item.AreaId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetAreaSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<AreaRequest> items = JsonConvert.DeserializeObject<IList<AreaRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (AreaRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.AreaId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Authenticated/Authorized
        protected bool IsAuthenticated()
        {
            if (String.IsNullOrEmpty(MyToken) || String.IsNullOrEmpty(MyUserId) || MyUserId.IndexOf("@") <= 0)
            {
                return false;
            }

            return true;
        }

        protected bool IsAdmin()
        {
            if (Admin == 0)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Categories
        protected async Task<IList<CategoryRequest>> GetCategories(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<CategoryRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<CategoryRequest>>(data.Data.ToString());

                    IList<CategoryRequest> selectItems = new List<CategoryRequest>();

                    foreach (CategoryRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new CategoryRequest() { CategoryId = item.CategoryId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetCategorySelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<CategoryRequest> items = JsonConvert.DeserializeObject<IList<CategoryRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (CategoryRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.CategoryId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Committees
        protected async Task<IList<CommitteeRequest>> GetCommittees(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<CommitteeRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<CommitteeRequest>>(data.Data.ToString());

                    IList<CommitteeRequest> selectItems = new List<CommitteeRequest>();

                    foreach (CommitteeRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new CommitteeRequest() { CommitteeId = item.CommitteeId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetCommitteeSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Committee/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<CommitteeRequest> items = JsonConvert.DeserializeObject<IList<CommitteeRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (CommitteeRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.CommitteeId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Credentials
        protected async Task<IList<CredentialRequest>> GetCredentials(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<CredentialRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<CredentialRequest>>(data.Data.ToString());

                    IList<CredentialRequest> selectItems = new List<CredentialRequest>();

                    foreach (CredentialRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new CredentialRequest() { CredentialId = item.CredentialId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetCredentialSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Credential/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<CredentialRequest> items = JsonConvert.DeserializeObject<IList<CredentialRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (CredentialRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.CredentialId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Documents
        protected async Task<IList<DocumentRequest>> GetDocuments(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<DocumentRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<DocumentRequest>>(data.Data.ToString());

                    IList<DocumentRequest> selectItems = new List<DocumentRequest>();

                    foreach (DocumentRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new DocumentRequest() { DocumentId = item.DocumentId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetDocumentSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<DocumentRequest> items = JsonConvert.DeserializeObject<IList<DocumentRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (DocumentRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.DocumentId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Getters
        protected string Uri 
        { 
            get { return _AppSettings.UriLocal; }
            //get { return _AppSettings.UriHiranyaloka; }
            //get { return _AppSettings.UriProduction; }
        }
        
        protected string ContentType { get { return _AppSettings.ContentType; } }
        protected string MyToken { get { return _httpContextAccessor.HttpContext.Session.GetString("token"); } }
        protected string MyUserId { get { return _httpContextAccessor.HttpContext.Session.GetString("EmailAddress"); } }
        protected int Admin { get { return (int)_httpContextAccessor.HttpContext.Session.GetInt32("IsAdmin"); } }
        protected string TempPassword { get { return _AppSettings.TempPassword; } }
        #endregion

        #region Events
        protected async Task<IList<EventRequest>> GetEvents(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<EventRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<EventRequest>>(data.Data.ToString());

                    IList<EventRequest> selectItems = new List<EventRequest>();

                    foreach (EventRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new EventRequest() { EventId = item.EventId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetEventSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Event/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<EventRequest> items = JsonConvert.DeserializeObject<IList<EventRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (EventRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.EventId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Event Tasks
        protected async Task<IList<EventTaskRequest>> GetEventTasks(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<EventTaskRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<EventTaskRequest>>(data.Data.ToString());

                    IList<EventTaskRequest> selectItems = new List<EventTaskRequest>();

                    foreach (EventTaskRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new EventTaskRequest() { EventTaskId = item.EventTaskId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetEventTaskSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/EventTask/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<EventTaskRequest> items = JsonConvert.DeserializeObject<IList<EventTaskRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (EventTaskRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.EventTaskId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Event Types
        protected async Task<IList<EventTypeRequest>> GetEventTypes(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<EventTypeRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<EventTypeRequest>>(data.Data.ToString());

                    IList<EventTypeRequest> selectItems = new List<EventTypeRequest>();

                    foreach (EventTypeRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new EventTypeRequest() { EventTypeId = item.EventTypeId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetEventTypeSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/EventType/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<EventTypeRequest> items = JsonConvert.DeserializeObject<IList<EventTypeRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (EventTypeRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.EventTypeId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Roles
        protected async Task<IList<RoleRequest>> GetRoles(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<RoleRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<RoleRequest>>(data.Data.ToString());

                    IList<RoleRequest> selectItems = new List<RoleRequest>();

                    foreach (RoleRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new RoleRequest() { RoleId = item.RoleId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetRoleSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Role/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<RoleRequest> items = JsonConvert.DeserializeObject<IList<RoleRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (RoleRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.RoleId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Properties
        protected async Task<IList<PropertyRequest>> GetProperties(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<PropertyRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<PropertyRequest>>(data.Data.ToString());

                    IList<PropertyRequest> selectItems = new List<PropertyRequest>();

                    foreach (PropertyRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new PropertyRequest() { PropertyId = item.PropertyId, Name = item.Name, City = item.City, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetPropertySelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Property/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<PropertyRequest> items = JsonConvert.DeserializeObject<IList<PropertyRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (PropertyRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.PropertyId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Session
        protected void SetStringSession(string key, string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key.ToString(), value);
        }

        protected string GetStringSession(string key)
        {
            return _httpContextAccessor.HttpContext.Session.GetString(key.ToString());
        }

        protected void SetIntSession(string key, int value)
        {
            HttpContext.Session.SetInt32(key.ToString(), value);
        }

        protected int GetIntSession(string key)
        {
            return HttpContext.Session.GetInt32(key.ToString()) == null ? 0 : (int)HttpContext.Session.GetInt32(key.ToString());
        }

        protected int GetPropertyId()
        {
            return Convert.ToInt32(GetStringSession("PropertyId").ToString());
        }
        #endregion

        #region Supply Items
        protected async Task<IList<SupplyItemRequest>> GetSupplyItems(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<SupplyItemRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<SupplyItemRequest>>(data.Data.ToString());

                    IList<SupplyItemRequest> selectItems = new List<SupplyItemRequest>();

                    foreach (SupplyItemRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new SupplyItemRequest() { SupplyItemId = item.SupplyItemId, PropertyId = item.PropertyId, Name = item.Name, Description = item.Description, Note = item.Note, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetSupplyItemSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<SupplyItemRequest> items = JsonConvert.DeserializeObject<IList<SupplyItemRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();
                    
                    foreach (SupplyItemRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.Name, item.SupplyItemId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IActionResult> GetSupplyItem(int id, int propertyId, string userId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&propertyId={2}&UserId='{3}", id, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var item = JsonConvert.DeserializeObject<IList<SupplyItemRequest>>(data.Data.ToString());

                    return (IActionResult)item;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Staff
        protected async Task<IList<StaffRequest>> GetStaff(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return (IList<StaffRequest>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var items = JsonConvert.DeserializeObject<IList<StaffRequest>>(data.Data.ToString());

                    IList<StaffRequest> selectItems = new List<StaffRequest>();

                    foreach (StaffRequest? item in items)
                    {
                        if (isActive)
                        {
                            selectItems.Add(new StaffRequest() { StaffId = item.StaffId, FirstName = item.FirstName, MiddleName = item.MiddleName, LastName = item.LastName, IsActive = isActive });
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetStaffSelectList(int propertyId, string userId, bool isActive, bool isHidden)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                    {
                        TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

                        return ((IList<SelectListItem>)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error));
                    }
                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<StaffRequest> items = JsonConvert.DeserializeObject<IList<StaffRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();

                    foreach (StaffRequest? item in items)
                    {
                        selectItems.Add(new SelectListItem(item.FirstName, item.PropertyId.ToString()));
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Temple Properties
        protected async Task<IList<SelectListItem>> GetTempleProperties(bool isActive, bool isTokenRequired)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    if (isTokenRequired)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));
                        if (client.DefaultRequestHeaders.Authorization.Parameter == null)
                        {
                            throw new Exception("Bearer token is null");
                        }
                    }

                    string email = GetStringSession("EmailAddress");
                    int? propertyId = GetIntSession("PropertyId");

                    MiscRequest misc = new MiscRequest();
                    misc.UserId = !String.IsNullOrEmpty(email) ? email : "srfyssvolunteer@gmail.com";
                    misc.PropertyId = (int)((propertyId == null || propertyId > 0) ? propertyId : 0);
                    misc.DeleteById = 0;
                    misc.GetById = 0;

                    string stringData = JsonConvert.SerializeObject(misc);
                    var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
                    HttpResponseMessage response = await client.PostAsync(string.Format("{0}/Account/GetAllPropertiesAsync", this.Uri), contentData);
                    stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<PropertyViewModel> properties = JsonConvert.DeserializeObject<IList<PropertyViewModel>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();
                    selectItems.Add(new SelectListItem("", ""));
                    foreach (PropertyViewModel property in properties)
                    {
                        if (isActive && property.IsActive)
                        {
                            selectItems.Add(new SelectListItem(property.Name, property.PropertyId.ToString()));
                        }
                    }

                    return selectItems;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Helpers
        public IActionResult CustomModalPopUp(ModalType type)
        {
            ModalViewModel viewModel = new ModalViewModel { ModalType = type };

            return View(viewModel);
        }
        #endregion
    }
}
