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

        #region Authenticated / Authorized
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
            catch
            {
                throw;
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

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/EventTask/GetAllAsync?propertyId={1}&UserId='{2}", this.Uri, propertyId, userId));

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

        protected async Task<EventTaskRequest> GetEventTask(int id, int propertyId, string userId)
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

                        return (EventTaskRequest)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&propertyId={2}&UserId='{3}", id, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var item = JsonConvert.DeserializeObject<IList<EventTaskRequest>>(data.Data.ToString());

                    return (EventTaskRequest)item;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Categories
        //protected async Task<IList<Category>> GetCustomCategories()
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Category> categories = JsonConvert.DeserializeObject<IList<Category>>(data.Data.ToString());

        //            return (IList<Category>)categories.Select(x=>x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomCategories(int categoryId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Category> categories = JsonConvert.DeserializeObject<IList<Category>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (Category category in categories)
        //            {
        //                if (category.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (category.CategoryId == categoryId)
        //                {
        //                    selectItems.Add(new SelectListItem(category.Name, category.CategoryId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(category.Name, category.CategoryId.ToString()));
        //                }
        //            }

        //            return selectItems;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<Category> GetCustomCategory(int categoryId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Category/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Category> categories = JsonConvert.DeserializeObject<IList<Category>>(data.Data.ToString());
        //            Category returnCategory = null;

        //            foreach (Category category in categories)
        //            {
        //                if (category.CategoryId == categoryId && category.IsActive == true)
        //                {
        //                    returnCategory = category;
        //                }
        //            }

        //            return returnCategory;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        #endregion

        #region Roles
        //protected async Task<IList<SelectListItem>> GetCustomRoles(int roleId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Role/GetAllAsync?userId=''", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Role> roles = JsonConvert.DeserializeObject<IList<Role>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (Role role in roles)
        //            {
        //                if (role.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (role.RoleId == roleId)
        //                {
        //                    selectItems.Add(new SelectListItem(role.Name, role.RoleId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(role.Name, role.RoleId.ToString()));
        //                }
        //            }

        //            return selectItems;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<Role> GetCustomRole(int roleId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Role/GetAllAsync?userId=''", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Role> roles = JsonConvert.DeserializeObject<IList<Role>>(data.Data.ToString());
        //            Role returnRole = null;

        //            foreach (Role role in roles)
        //            {
        //                if (role.RoleId == roleId && role.IsActive == true)
        //                {
        //                    returnRole = role;
        //                }
        //            }

        //            return returnRole;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<int> GetCustomGuestRole()
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Role/GetAllAsync?userId=''", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Role> roles = JsonConvert.DeserializeObject<IList<Role>>(data.Data.ToString());
        //            int returnRole = 0;

        //            foreach (Role role in roles)
        //            {
        //                if (role.Name == "Guest")
        //                {
        //                    returnRole = role.RoleId;
        //                }
        //            }

        //            return returnRole;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        #endregion

        #region Properties
        //protected async Task<IList<Property>> GetCustomProperties()
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Property/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Property> properties = JsonConvert.DeserializeObject<IList<Property>>(data.Data.ToString());

        //            return (IList<Property>)properties.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomProperties(int propertyId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Property/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Property> properties = JsonConvert.DeserializeObject<IList<Property>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (Property? property in properties)
        //            {
        //                if (property.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (property.PropertyId == propertyId)
        //                {
        //                    selectItems.Add(new SelectListItem(property.Name, property.PropertyId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(property.Name, property.PropertyId.ToString()));
        //                }
        //            }

        //            return selectItems;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<Property> GetCustomProperty(int propertyId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Property/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Property> properties = JsonConvert.DeserializeObject<IList<Property>>(data.Data.ToString());
        //            Property returnProperty = null;

        //            foreach (Property? property in properties)
        //            {
        //                if (property.PropertyId == propertyId && property.IsActive == true)
        //                {
        //                    returnProperty = property;
        //                }
        //            }

        //            return returnProperty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        #endregion

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

        //protected async Task<AreaRequest> GetAreaItem(int id, int propertyId, string userId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                TempData["ModalMessage"] = "You are not authorized. Contact the Adiministrator if you feel this is an error.";

        //                return RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&propertyId={2}&UserId='{3}", id, propertyId, userId));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            var item = JsonConvert.DeserializeObject<IList<AreaRequest>>(data.Data.ToString());

        //            return (AreaRequest)item;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        #endregion

        #region Documents
        //protected async Task<IList<Document>> GetCustomDocuments()
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Document> documents = JsonConvert.DeserializeObject<IList<Document>>(data.Data.ToString());

        //            return (IList<Document>)documents.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomSelectListDocuments(int documentId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Document> documents = JsonConvert.DeserializeObject<IList<Document>>(data.Data.ToString());

        //            IList<SelectListItem> selectDocuments = new List<SelectListItem>();
        //            foreach (Document? document in documents)
        //            {
        //                if (document.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (document.DocumentId == documentId)
        //                {
        //                    selectDocuments.Add(new SelectListItem(document.Name, document.DocumentId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectDocuments.Add(new SelectListItem(document.Name, document.DocumentId.ToString()));
        //                }
        //            }

        //            return selectDocuments;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<Document>> GetCustomDocuments(int documentId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Document> documents = JsonConvert.DeserializeObject<IList<Document>>(data.Data.ToString());

        //            return (IList<Document>)documents.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<Document> GetCustomDocument(int documentId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            if (client.DefaultRequestHeaders.Authorization.Parameter == null)
        //            {
        //                throw new Exception("Bearer token is null");
        //            }

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Document/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Document> documents = JsonConvert.DeserializeObject<IList<Document>>(data.Data.ToString());
        //            Document returnDocument = null;

        //            foreach (Document? document in documents)
        //            {
        //                if (document.DocumentId == documentId && document.IsActive == true)
        //                {
        //                    returnDocument = document;
        //                }
        //            }

        //            return returnDocument;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomSelectListAreaDocuments(IList<AreaDocument> programEventAreaDocuments)
        //{
        //    try
        //    {
        //        var documents = await GetCustomSelectListDocuments(0);
        //        IList<SelectListItem> selectItems;

        //        selectItems = (from d in documents where !(from a in programEventAreaDocuments select a.DocumentId.ToString()).Contains(d.Value) select d).ToList();

        //        return selectItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<AreaDocument>> GetCustomSelectedAreaDocument(IList<AreaDocument> programEventAreaDocuments)
        //{
        //    try
        //    {
        //        var documents = await GetCustomDocuments(0);
        //        IList<AreaDocument> areaDocuments = new List<AreaDocument>();

        //        foreach (AreaDocument ad in programEventAreaDocuments)
        //        {
        //            Document d = documents.ToList().Find(x => x.DocumentId == ad.DocumentId && x.IsActive);
        //            areaDocuments.Add(new AreaDocument { AreaId = ad.AreaId, DocumentId = ad.DocumentId, Document = d });
        //        }

        //        return areaDocuments;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
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

        protected async Task<SupplyItemRequest> GetSupplyItem(int id, int propertyId, string userId)
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

                        return (SupplyItemRequest)RedirectPermanent("/Custom/ModalPopUp?type=" + ModalType.Error);
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/SupplyItem/GetByIdAsync?id={1}&propertyId={2}&UserId='{3}", id, propertyId, userId));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    var item = JsonConvert.DeserializeObject<IList<SupplyItemRequest>>(data.Data.ToString());

                    return (SupplyItemRequest)item;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Staff
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
