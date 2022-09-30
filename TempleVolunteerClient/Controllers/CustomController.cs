using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using TempleVolunteerClient.Common;
using Newtonsoft.Json;
using System.Data;
using System.Text;

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
                    IList<PropertyRequest> properties = JsonConvert.DeserializeObject<IList<PropertyRequest>>(data.Data.ToString());

                    IList<SelectListItem> selectItems = new List<SelectListItem>();
                    selectItems.Add(new SelectListItem("", ""));
                    foreach (PropertyRequest property in properties)
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
        //#region Categories
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
        //#endregion

        //#region Roles
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
        //#endregion

        //#region Properties
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
        //#endregion

        //#region Program Events
        //protected async Task<IList<ProgramEvent>> GetCustomProgramEvents()
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/ProgramEvent/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            List<ProgramEvent> programEvents = JsonConvert.DeserializeObject<List<ProgramEvent>>(data.Data.ToString());

        //            return programEvents.FindAll(x => x.IsActive);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomProgramEvents(int programEventId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/ProgramEvent/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<ProgramEvent> programEvents = JsonConvert.DeserializeObject<IList<ProgramEvent>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (ProgramEvent? programEvent in programEvents)
        //            {
        //                if (programEvent.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (programEvent.ProgramEventId == programEventId)
        //                {
        //                    selectItems.Add(new SelectListItem(programEvent.Theme, programEvent.ProgramEventId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(programEvent.Theme, programEvent.ProgramEventId.ToString()));
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

        //protected async Task<ProgramEvent> GetCustomProgramEvent(int programEventId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/ProgramEvent/GetByIdAsync?id={1}&userId='{2}'", this.Uri, programEventId, this.GetStringSession("EmailAddress")));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            ProgramEvent? programEvent = JsonConvert.DeserializeObject<ProgramEvent>(data.Data.ToString());

        //            return programEvent;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //#endregion

        //#region Areas
        //protected async Task<IList<Area>> GetCustomAreas()
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Area> areas = JsonConvert.DeserializeObject<IList<Area>>(data.Data.ToString());

        //            return (IList<Area>)areas.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomAreas(int areaId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Area> areas = JsonConvert.DeserializeObject<IList<Area>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (Area? area in areas)
        //            {
        //                if (area.IsActive != true)
        //                {
        //                    continue;
        //                }

        //                if (area.AreaId == areaId)
        //                {
        //                    selectItems.Add(new SelectListItem(area.Name, area.AreaId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(area.Name, area.AreaId.ToString()));
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

        //protected async Task<Area> GetCustomArea(int areaId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Area/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Area> areas = JsonConvert.DeserializeObject<IList<Area>>(data.Data.ToString());
        //            Area returnArea = null;

        //            foreach (Area? area in areas)
        //            {
        //                if (area.AreaId == areaId && area.IsActive == true)
        //                {
        //                    returnArea = area;
        //                }
        //            }

        //            return returnArea;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomProgramEventAreas(IList<ProgramEventArea> programEventAreas, bool selected)
        //{
        //    try
        //    {
        //        var areas = await GetCustomAreas(0);
        //        IList<SelectListItem> selectItems;

        //        if (selected)
        //        {
        //            selectItems = (from a in areas where (from p in programEventAreas select p.AreaId.ToString()).Contains(a.Value) select a).ToList();
        //        }
        //        else
        //        {
        //            selectItems = (from a in areas where !(from p in programEventAreas select p.AreaId.ToString()).Contains(a.Value) select a).ToList();
        //        }

        //        return selectItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        //#endregion

        //#region Documents
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
        //#endregion

        //#region Items
        //protected async Task<IList<Item>> GetCustomItems()
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Item/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Item> items = JsonConvert.DeserializeObject<IList<Item>>(data.Data.ToString());

        //            return (IList<Item>)items.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomSelectListItems(int itemId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Item/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Item> items = JsonConvert.DeserializeObject<IList<Item>>(data.Data.ToString());

        //            IList<SelectListItem> selectItems = new List<SelectListItem>();
        //            foreach (Item? item in items)
        //            {
        //                if (!item.IsActive)
        //                {
        //                    continue;
        //                }

        //                if (item.ItemId == itemId)
        //                {
        //                    selectItems.Add(new SelectListItem(item.Name, item.ItemId.ToString(), true));
        //                }
        //                else
        //                {
        //                    selectItems.Add(new SelectListItem(item.Name, item.ItemId.ToString()));
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

        //protected async Task<IList<Item>> GetCustomItems(int itemId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Item/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Item> items = JsonConvert.DeserializeObject<IList<Item>>(data.Data.ToString());

        //            return (IList<Item>)items.Select(x => x.IsActive == true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<Item> GetCustomItem(int itemId)
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

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Item/GetAllAsync", this.Uri));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
        //            IList<Item> items = JsonConvert.DeserializeObject<IList<Item>>(data.Data.ToString());
        //            Item returnItem = null;

        //            foreach (Item? item in items)
        //            {
        //                if (item.ItemId == itemId && item.IsActive)
        //                {
        //                    returnItem = item;
        //                }
        //            }

        //            return returnItem;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<SelectListItem>> GetCustomSelectListAreaItems(IList<AreaItem> programEventAreaItems)
        //{
        //    try
        //    {
        //        var items = await GetCustomSelectListItems(0);
        //        IList<SelectListItem> selectItems;

        //        selectItems = (from i in items where !(from a in programEventAreaItems select a.ItemId.ToString()).Contains(i.Value) select i).ToList();

        //        return selectItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        //protected async Task<IList<AreaItem>> GetCustomSelectedAreaItem(IList<AreaItem> programEventAreaItems)
        //{
        //    try
        //    {
        //        var items = await GetCustomItems(0);
        //        IList<AreaItem> areaItems = new List<AreaItem>();

        //        foreach (AreaItem ai in programEventAreaItems)
        //        {
        //            Item i = items.ToList().Find(x => x.ItemId == ai.ItemId && x.IsActive);
        //            areaItems.Add(new AreaItem { AreaId = ai.AreaId, ItemId = ai.ItemId, Item = i });
        //        }

        //        return areaItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        //#endregion

        #region Staff
        protected async Task<IList<StaffViewModel>> GetCustomStaff()
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
                        throw new Exception("Bearer token is null");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId=''", this.Uri));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    List<StaffViewModel> staff = JsonConvert.DeserializeObject<List<StaffViewModel>>(data.Data.ToString());

                    return staff.FindAll(x => x.IsActive);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<SelectListItem>> GetCustomStaff(int staffId)
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
                        throw new Exception("Bearer token is null");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync?userId=''", this.Uri));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<StaffViewModel> staff = JsonConvert.DeserializeObject<IList<StaffViewModel>>(data.Data.ToString());

                    IList<SelectListItem> selectStaff = new List<SelectListItem>();
                    foreach (StaffViewModel st in staff)
                    {
                        if (!st.IsActive)
                        {
                            continue;
                        }

                        if (st.StaffId == staffId)
                        {
                            selectStaff.Add(new SelectListItem(st.FirstName + " " + st.LastName, st.StaffId.ToString(), true));
                        }
                        else
                        {
                            selectStaff.Add(new SelectListItem(st.FirstName + " " + st.LastName + " (" + st.EmailAddress + ")", st.StaffId.ToString()));
                        }
                    }

                    return selectStaff;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<IList<StaffViewModel>> GetCustomStaffs(int staffId)
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
                        throw new Exception("Bearer token is null");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync", this.Uri));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    List<StaffViewModel> staff = JsonConvert.DeserializeObject<List<StaffViewModel>>(data.Data.ToString());

                    return staff.FindAll(x => x.IsActive);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected async Task<StaffViewModel> GetCustomStaff2(int staffId)
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
                        throw new Exception("Bearer token is null");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync", this.Uri));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<StaffViewModel> staff = JsonConvert.DeserializeObject<IList<StaffViewModel>>(data.Data.ToString());
                    StaffViewModel returnStaff = null;

                    foreach (StaffViewModel st in staff)
                    {
                        if (st.StaffId == staffId && st.IsActive)
                        {
                            returnStaff = st;
                        }
                    }

                    return returnStaff;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected async Task<IList<SelectListItem>> GetCustomSelectListStaff(int staffId)
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
                        throw new Exception("Bearer token is null");
                    }

                    HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Staff/GetAllAsync", this.Uri));

                    string stringData = response.Content.ReadAsStringAsync().Result;
                    ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());
                    IList<StaffViewModel> staffs = JsonConvert.DeserializeObject<IList<StaffViewModel>>(data.Data.ToString());

                    IList<SelectListItem> selectStaff = new List<SelectListItem>();
                    foreach (StaffViewModel? staff in staffs)
                    {
                        if (!staff.IsActive)
                        {
                            continue;
                        }

                        if (staff.StaffId == staffId)
                        {
                            selectStaff.Add(new SelectListItem(staff.FirstName + " " + staff.LastName, staff.StaffId.ToString(), true));
                        }
                        else
                        {
                            selectStaff.Add(new SelectListItem(staff.FirstName + " " + staff.LastName, staff.StaffId.ToString()));
                        }
                    }

                    return selectStaff;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
