using Microsoft.AspNetCore.Mvc;

namespace TempleVolunteerClient.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task SendMessage(int staffId, string message)
        //{
        //    if (!IsAuthenticated())
        //    {
        //        throw new Exception("You are not authorized");
        //    }

        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);

        //            Staff? staff = await this.GetCustomStaff2(staffId);
        //            MessageResponse email = new MessageResponse();
        //            email.StaffId = staffId;
        //            email.From = this.GetStringSession("EmailAddress");
        //            email.To = staff.EmailAddress;
        //            email.Subject = string.Format("Message from: '{0}' (Summer Day Portal)", email.From);
        //            email.MessageSent = message;

        //            var stringData = JsonConvert.SerializeObject(email);
        //            var contentData = new StringContent(stringData, Encoding.UTF8, this.ContentType);
        //            var response = await client.PostAsync(string.Format("{0}/Account/SendMessageAsync", this.Uri), contentData);
        //            var responseDeserialized = JsonConvert.DeserializeObject<ServiceResponse>((JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result.ToString())).ToString());

        //            if (!responseDeserialized.Success)
        //            {
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //[HttpGet]
        //[AutoValidateAntiforgeryToken]
        //public async Task<List<MessageResponse>> GetAllMessagesById(int staffId)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            var contentType = new MediaTypeWithQualityHeaderValue(this.ContentType);
        //            client.DefaultRequestHeaders.Accept.Add(contentType);
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

        //            HttpResponseMessage response = await client.GetAsync(string.Format("{0}/Account/GetAllMessagesByIdAsync?userId={1}", this.Uri, this.GetStringSession("EmailAddress")));

        //            string stringData = response.Content.ReadAsStringAsync().Result;
        //            ServiceResponse data = JsonConvert.DeserializeAnonymousType<ServiceResponse>(stringData, new ServiceResponse());

        //            var messages = JsonConvert.DeserializeObject<List<MessageResponse>>(data.Data.ToString());

        //            return messages.OrderByDescending(d => d.DateTime).ToList();
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
    }
}
